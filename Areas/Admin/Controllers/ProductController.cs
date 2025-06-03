using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookNest.Models.MappingDBModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BookNest.Data;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;

namespace BookNest.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public ProductController(ApplicationDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<IActionResult> Index(string searchString, string parentCategoryId, string publisherId, int page = 1, int pageSize = 10)
        {
            // Validate and set page size (limit to reasonable values)
            pageSize = pageSize switch
            {
                10 or 25 or 50 or 100 => pageSize,
                _ => 10 // Default to 10 if invalid value
            };

            int pageNumber = page > 0 ? page : 1;

            var books = _context.Books
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Include(b => b.Publisher)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.BookName.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            // Filter by ParentCategory
            if (!string.IsNullOrEmpty(parentCategoryId) && int.TryParse(parentCategoryId, out int parentCatId))
            {
                books = books.Where(b => b.Category.ParentCategoryId == parentCatId);
                ViewBag.ParentCategoryId = parentCategoryId;
            }

            // Filter by Publisher
            if (!string.IsNullOrEmpty(publisherId) && int.TryParse(publisherId, out int pubId))
            {
                books = books.Where(b => b.PublisherId == pubId);
                ViewBag.PublisherId = publisherId;
            }

            // Get total count before pagination
            var totalItems = await books.CountAsync();

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Ensure page number is within valid range
            if (pageNumber > totalPages && totalPages > 0)
            {
                pageNumber = totalPages;
            }

            // Apply pagination
            var pagedBooks = await books
                .OrderBy(b => b.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Populate ViewBag for dropdowns and pagination
            ViewBag.ParentCategories = await _context.ParentCategories
                .OrderBy(pc => pc.Type)
                .ToListAsync();

            ViewBag.Publishers = await _context.Publishers
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Pagination data
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            // Keep search parameters for pagination links
            ViewBag.CurrentFilters = new
            {
                SearchString = searchString,
                ParentCategoryId = parentCategoryId,
                PublisherId = publisherId,
                PageSize = pageSize
            };

            return View(pagedBooks);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .ThenInclude(c => c.ParentCategory)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync();
            ViewBag.Publishers = await _context.Publishers.ToListAsync();

            return View(book);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Edit(Book book, IFormFile ImageFile)
        {
            if (book == null)
            {
                return NotFound();
            }

            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy entity hiện tại từ database
                    var existingBook = await _context.Books.FindAsync(book.Id);
                    if (existingBook == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính
                    existingBook.BookName = book.BookName;
                    existingBook.Isbn = book.Isbn;
                    existingBook.Author = book.Author;
                    existingBook.YearPublished = book.YearPublished;
                    existingBook.Pages = book.Pages;
                    existingBook.ImportPrice = book.ImportPrice;
                    existingBook.FirstPrice = book.FirstPrice;
                    existingBook.SecondPrice = book.SecondPrice;
                    existingBook.Quantity = book.Quantity;
                    existingBook.CategoryId = book.CategoryId;
                    existingBook.PublisherId = book.PublisherId;
                    existingBook.Description = book.Description;

                    // Xử lý upload ảnh
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        using (var stream = ImageFile.OpenReadStream())
                        {
                            var uploadParam = new ImageUploadParams()
                            {
                                File = new FileDescription(ImageFile.FileName, stream),
                                Folder = "Book_cover"
                            };
                            var uploadResult = _cloudinary.Upload(uploadParam);
                            existingBook.ImageUrl = uploadResult.SecureUrl.ToString();
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            // Nếu ModelState không valid, load lại data cho dropdown
            ViewBag.Categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync();
            ViewBag.Publishers = await _context.Publishers.ToListAsync();
            return View("Details", book);
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync();
            ViewBag.Publishers = await _context.Publishers.ToListAsync();
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using (var stream = ImageFile.OpenReadStream())
                    {
                        var uploadParam = new ImageUploadParams()
                        {
                            File = new FileDescription(ImageFile.FileName, stream),
                            Folder = "Book_cover"
                        };

                        var uploadResult = _cloudinary.Upload(uploadParam);
                        book.ImageUrl = uploadResult.SecureUrl.ToString();
                    }
                }
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync();
            ViewBag.Publishers = await _context.Publishers.ToListAsync();
            return View(book);
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}