using BookNest.Data;
using BookNest.Models.MappingDBModel;
using BookNest.Models.ViewModel;
using BookNest.Models.ViewModel.BookHandleViewModel;
using BookNest.Models.ViewModel.HomeHandleViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Linq;

namespace BookNest.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BooksController : Controller
    {
        public readonly ApplicationDbContext _context;
        private readonly UserManager<AspNetUser> _userManager;
        private const int PageSize = 21;

        public BooksController(ApplicationDbContext context, UserManager<AspNetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(
            ParentCategoryViewModel? parentCategoryId,
            CategoryViewModel? categoryId,
            List<int>? publisherId,
            List<int>? rangePrice,
            string? keyword,
            int page = 1)
        {
            page = page < 1 ? 1 : page;

            if (parentCategoryId != null && parentCategoryId.Id == 0)
            {
                parentCategoryId = null;
            }
            else if (parentCategoryId != null && string.IsNullOrEmpty(parentCategoryId.Type))
            {
                var parent = _context.ParentCategories.FirstOrDefault(p => p.Id == parentCategoryId.Id);
                if (parent != null)
                {
                    parentCategoryId.Type = parent.Type;
                }
            }

            if (categoryId != null && categoryId.Id == 0)
            {
                categoryId = null;
            }
            else if (categoryId != null && string.IsNullOrEmpty(categoryId.Name))
            {
                var category = _context.Categories.FirstOrDefault(p => p.Id == categoryId.Id);
                if (category != null)
                {
                    categoryId.Name = category.Name;
                }
            }

            var BookQuery = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Where(b => b.Quantity > 0)
                .AsQueryable();

            if (parentCategoryId != null)
            {
                var subCategoryIds = _context.Categories
                    .Where(c => c.ParentCategoryId == parentCategoryId.Id)
                    .Select(c => c.Id)
                    .ToList();

                BookQuery = BookQuery.Where(b => subCategoryIds.Contains((int)b.CategoryId));
            }


            if (categoryId != null)
            {
                BookQuery = BookQuery.Where(b => b.CategoryId == categoryId.Id);
            }

            if (publisherId != null && publisherId.Any())
            {
                BookQuery = BookQuery.Where(b => publisherId.Contains((int)b.PublisherId));
            }

            if (rangePrice != null && rangePrice.Any())
            {
                BookQuery = BookQuery.Where(b =>
                    rangePrice.Contains(0) && b.SecondPrice < 80000 ||
                    rangePrice.Contains(1) && b.SecondPrice >= 80000 && b.SecondPrice < 140000 ||
                    rangePrice.Contains(2) && b.SecondPrice >= 140000 && b.SecondPrice < 250000 ||
                    rangePrice.Contains(3) && b.SecondPrice >= 250000
                );
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                BookQuery = BookQuery.Where(b =>
                    b.BookName.ToLower().Contains(keyword.ToLower()) ||
                    b.Author.ToLower().Contains(keyword.ToLower()));
            }

            int totalBooks = BookQuery.Count();
            int totalPages = (int)Math.Ceiling(totalBooks / (double)PageSize);

            var books = BookQuery
                .OrderBy(b => b.BookName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var viewModel = new BookIndexViewModel
            {
                Books = books.Select(b => new BookViewModel
                {
                    Id = b.Id,
                    BookName = b.BookName,
                    FirstPrice = (decimal)b.FirstPrice,
                    SecondPrice = (decimal)b.SecondPrice,
                    Author = b.Author,
                    ImageUrl = b.ImageUrl
                }).ToList(),

                ParentCategories = _context.ParentCategories
                .Select(c => new ParentCategoryViewModel2
                {
                    Id = c.Id,
                    Type = c.Type,
                    CategoryViewModels = _context.Categories
                        .Where(cv => cv.ParentCategoryId == c.Id)
                        .Select(cv => new CategoryViewModel
                        {
                            Id = cv.Id,
                            Name = cv.Name
                        }).ToList()
                }).ToList(),

                PublisherCategories = _context.Publishers
                .Select(s => new PublisherViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList(),

                Page = page,
                TotalPages = totalPages,

                ParentCategoryId = parentCategoryId,
                CategoryId = categoryId,
                PublisherId = publisherId,
                RangePrice = rangePrice
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int Id)
        {
            // Lấy thông tin sách chi tiết
            var Book = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookComments)
                .Include(b => b.BookRatings)
                .FirstOrDefault(c => c.Id == Id);

            if (Book == null)
            {
                return NotFound();
            }

            // Lấy danh sách sách liên quan (random 4 cuốn)
            var ListBook = _context.Books
                .Where(c => c.Id != Id && c.CategoryId == Book.CategoryId) // Cùng category
                .OrderBy(c => Guid.NewGuid())
                .Take(4)
                .Select(b => new BookViewModel
                {
                    Id = b.Id,
                    BookName = b.BookName,
                    Author = b.Author,
                    ImageUrl = b.ImageUrl,
                    FirstPrice = (decimal)b.FirstPrice,
                    SecondPrice = (decimal)b.SecondPrice
                }).ToList();

            var ratingList = new List<RatingViewModel>();

            foreach (var comment in Book.BookComments.Where(c => !string.IsNullOrEmpty(c.Content)))
            {
                // Lấy thông tin user qua UserManager
                var user = await _userManager.FindByIdAsync(comment.UserId.ToString());
                var rating = Book.BookRatings.FirstOrDefault(r => r.UserId == comment.UserId);

                ratingList.Add(new RatingViewModel
                {
                    userId = user.Id, // Chuyển string sang int nếu cần
                    username = user?.FirstName + " " + user?.LastName ?? "Anonymous",
                    imageurl = user?.ProfileImage ?? "/images/default-avatar.png",
                    rating = rating?.Rating ?? 0,
                    content = comment.Content,
                    CreateAt = (DateTime)comment.CreateAt
                });
            }

            var ratingStats = new RatingStatistics();

            if (ratingList != null && ratingList.Any())
            {
                ratingStats.TotalCount = ratingList.Count;
                ratingStats.FiveStarCount = ratingList.Count(r => r.rating == 5);
                ratingStats.FourStarCount = ratingList.Count(r => r.rating == 4);
                ratingStats.ThreeStarCount = ratingList.Count(r => r.rating == 3);
                ratingStats.TwoStarCount = ratingList.Count(r => r.rating == 2);
                ratingStats.OneStarCount = ratingList.Count(r => r.rating == 1);
            }

            // Tạo ViewModel
            BookDetailViewModel bookDetailViewModel = new BookDetailViewModel
            {
                Book = Book,
                ListBook = ListBook,
                Rating = ratingList,
                AverageRating = (decimal)(Book.BookRatings.Any() ? Book.BookRatings.Average(r => r.Rating) : 0),
                TotalRatings = Book.BookRatings.Count()
            };

            return View(bookDetailViewModel);
        }
    }

}
