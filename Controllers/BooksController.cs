using Microsoft.AspNetCore.Mvc;

namespace BookNest.Controllers
{
    public class BooksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details()
        {
            return View();
        }
    }

}
