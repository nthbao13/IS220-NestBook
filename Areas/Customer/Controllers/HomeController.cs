using BookNest.Data;
using BookNest.Models;
using BookNest.Models.ViewModel;
using BookNest.Models.ViewModel.HomeHandleViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookNest.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            HomeViewModel model = await LoadData();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<HomeViewModel> LoadData()
        {
            List<ParentCategoryViewModel> _ParentCategories =
            [
                new ParentCategoryViewModel { Id = 1, Type = "Văn học", ImageUrl = "/images/vanhoc.png" },
                new ParentCategoryViewModel { Id = 2, Type = "Kinh tế", ImageUrl = "/images/kinhte.png" },
                new ParentCategoryViewModel { Id = 5, Type = "Tâm lý", ImageUrl = "/images/tamly.png" },
                new ParentCategoryViewModel { Id = 7, Type = "Thiếu nhi", ImageUrl = "/images/thieunhi.png" },
                new ParentCategoryViewModel { Id = 3, Type = "Khoa học", ImageUrl = "/images/khoahoc.png" },
                new ParentCategoryViewModel { Id = 10, Type = "Công nghệ", ImageUrl = "/images/congnghe.png" },
            ];


            var _HotBooks = await _context.Books
            .OrderBy(x => Guid.NewGuid())
            .Take(10)
            .Select(b => new BookViewModel
            {
                Id = b.Id,
                BookName = b.BookName,
                Author = b.Author,
                ImageUrl = b.ImageUrl,
                FirstPrice = (decimal)b.FirstPrice,
                SecondPrice = (decimal)b.SecondPrice
            }).ToListAsync();

            var _LiteratureBooks = await GetBooksByParentCategoryId(1);
            var _EconomyBooks = await GetBooksByParentCategoryId(2);
            var _PsychoBooks = await GetBooksByParentCategoryId(5);
            var _ChildrenBooks = await GetBooksByParentCategoryId(7);
            var _ScienceBooks = await GetBooksByParentCategoryId(3);
            var _TechnologyBooks = await GetBooksByParentCategoryId(10);


            HomeViewModel _home = new HomeViewModel
            {
                ParentCategories = _ParentCategories,
                HotBooks = _HotBooks,
                LiteratureBooks = _LiteratureBooks,
                EconomyBooks = _EconomyBooks,
                PsychoBooks = _PsychoBooks,
                ChildrenBooks = _ChildrenBooks,
                ScienceBooks = _ScienceBooks,
                TechnologyBooks = _TechnologyBooks
            };

            return _home;
        }

        private async Task<List<BookViewModel>> GetBooksByParentCategoryId(int parentCategoryId)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.Category.ParentCategoryId == parentCategoryId)
                .OrderBy(b => b.Id)
                .Take(6)
                .Select(b => new BookViewModel
                {
                    Id = b.Id,
                    BookName = b.BookName,
                    Author = b.Author,
                    ImageUrl = b.ImageUrl,
                    FirstPrice = (decimal)b.FirstPrice,
                    SecondPrice = (decimal)b.SecondPrice
                })
                .ToListAsync();
        }

    }
}
