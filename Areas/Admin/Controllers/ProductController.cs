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

        public async Task<IActionResult> Index(string searchString, string parentCategoryId, string publisherId, int page = 1)
        {
            int pageSize = 10;
            int pageNumber = page > 0 ? page : 1;

            var books = _context.Books
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Include(b => b.Publisher)
                .AsQueryable();

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

            // Pagination
            var totalItems = await books.CountAsync();
            var pagedBooks = await books
                .OrderBy(b => b.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); 

            ViewBag.ParentCategories = await _context.ParentCategories.ToListAsync();
            ViewBag.Publishers = await _context.Publishers.ToListAsync();
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Book book, IFormFile ImageFile)
        {
            if (book == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
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
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

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