using BookNest.Areas.Admin.Models;
using BookNest.Data;
using BookNest.Models.MappingDBModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var today = DateTime.Now;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                startOfWeek = startOfWeek.AddDays(-7); 
            }

            int newCustomers = 0;
            decimal totalRevenue = 0;
            int totalOrders = 0;



            ViewBag.Title = "Trang chủ";    
            List<Order> orders = _context.Orders
                .Where(o=>o.CreateAt >= startOfWeek)
                .Where(o => o.Status != "CANCELLED")
                .Include(o => o.OrderDetails)
                .ThenInclude(o => o.Book)
                .Include(o => o.Payments.Where(p => p.Status == "COMPLETED"))
                .ToList();

            var bestSellingBooks = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.Book)
                .Select(g => new
                {
                    Book = g.Key,
                    TotalQuantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            int delta = DayOfWeek.Monday - today.DayOfWeek;
            DateTime weekStart = today.AddDays(delta); // Thứ Hai
            DateTime weekEnd = weekStart.AddDays(6);   // Chủ Nhật

            // Lọc payments từ các order trong tuần
            var weeklyRevenue = orders
                .SelectMany(o => o.Payments)
                .Where(p => p.Status == "COMPLETED" && p.CreateAt.HasValue)
                .Where(p => p.CreateAt.Value.Date >= weekStart && p.CreateAt.Value.Date <= weekEnd)
                .GroupBy(p => p.CreateAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.TotalPrice ?? 0)
                })
                .ToDictionary(g => g.Date, g => g.Revenue);

            var daysOfWeek = Enumerable.Range(0, 7)
                .Select(i => weekStart.AddDays(i))
                .ToList();

            var chartLabels = daysOfWeek.Select(d => d.ToString("dddd (dd/MM)")).ToList(); 
            var chartData = daysOfWeek.Select(d => weeklyRevenue.ContainsKey(d) ? weeklyRevenue[d] : 0).ToList();

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;



            foreach (Order order in orders)
            {
                if (_context.Orders.Count(o => o.UserId == order.UserId) == 1)
                {
                    newCustomers++;
                }

                totalRevenue += order.Payments.Sum(p => p.TotalPrice ?? 0);
                totalOrders++;
            }

            var dashboardData = new DashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                NewCustomers = newCustomers,
                RecentOrders = orders.Select(o => new RecentOrderViewModel
                {
                    OrderId = o.Id,
                    CustomerName = o.Name,
                    Amount = (decimal)o.Payments.Sum(p => p.TotalPrice),
                    OrderDate = (DateTime)o.CreateAt,
                    Status = o.Status
                }).ToList(),
                TopProducts = bestSellingBooks.Select(b => new TopProductViewModel
                {
                    ProductId = b.Book.Id,
                    ProductName = b.Book.BookName,
                    Author = b.Book.Author,
                    SoldQuantity = (int)b.TotalQuantity
                }).ToList(),
            };

            return View(dashboardData);
        }
    }
}
