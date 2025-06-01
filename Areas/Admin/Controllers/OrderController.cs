using BookNest.Data;
using BookNest.Helper;
using BookNest.Models.MappingDBModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<AspNetUser> _userManager;

        public OrderController(ApplicationDbContext context, IEmailSender emailSender, UserManager<AspNetUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Where(o => o.Status == "PENDING")
                .ToListAsync();
            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(m => m.Id == id && m.Status == "PENDING");

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Deliver/5
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Deliver(int id)
        {
            var order = await _context.Orders.Include(o=>o.OrderDetails)
                .ThenInclude(o => o.Book)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null || order.Status != "PENDING")
            {
                return NotFound();
            }

            order.Status = "DELIVERED";
            await _context.SaveChangesAsync();

            SendDeliveryNotificationEmail(order);

            return RedirectToAction(nameof(Index));
        }

        private async Task SendDeliveryNotificationEmail(Order order)
        {
            try
            {
                string subject = $"📦 Đơn hàng #{order.Id} đã được vận chuyển - BookNest";
                string htmlBody = GenerateDeliveryEmailHtml(order);

                int userId = (int)order.UserId;
                var user = await _userManager.FindByIdAsync(userId.ToString());

                string customerEmail = user?.Email; // Thay bằng email thật

                await _emailSender.SendEmailAsync(customerEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến flow chính
                Console.WriteLine($"Failed to send delivery email: {ex.Message}");
            }
        }

        private string GenerateDeliveryEmailHtml(Order order)
        {
            decimal total = order.OrderDetails.Sum(od => (od.Quantity ?? 0) * (od.Book?.SecondPrice ?? 0));

            var html = $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Thông báo vận chuyển - BookNest</title>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 15px; box-shadow: 0 10px 30px rgba(0,0,0,0.1); overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #4CAF50, #45a049); color: white; padding: 30px; text-align: center; }}
                        .success-icon {{ font-size: 48px; margin-bottom: 15px; }}
                        .header h1 {{ font-size: 24px; margin: 0 0 10px 0; }}
                        .order-id {{ background: rgba(255,255,255,0.2); padding: 8px 16px; border-radius: 20px; display: inline-block; }}
                        .content {{ padding: 30px; }}
                        .message {{ text-align: center; margin-bottom: 30px; color: #666; line-height: 1.6; }}
                        .order-details {{ background: #f8f9fa; border-radius: 10px; padding: 20px; }}
                        .section-title {{ font-size: 18px; color: #333; margin-bottom: 20px; padding-bottom: 10px; border-bottom: 2px solid #4CAF50; }}
                        .book-item {{ display: flex; align-items: center; padding: 15px 0; border-bottom: 1px solid #e9ecef; }}
                        .book-item:last-child {{ border-bottom: none; }}
                        .book-image {{ width: 60px; height: 80px; object-fit: cover; border-radius: 5px; margin-right: 15px; }}
                        .book-info {{ flex: 1; }}
                        .book-title {{ font-weight: 600; color: #333; margin-bottom: 5px; }}
                        .book-meta {{ color: #666; font-size: 14px; }}
                        .book-price {{ color: #4CAF50; font-weight: 600; text-align: right; }}
                        .total-section {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 10px; text-align: center; margin-top: 20px; }}
                        .total-amount {{ font-size: 24px; font-weight: bold; margin-top: 10px; }}
                        .footer {{ text-align: center; padding: 20px; background: #f8f9fa; color: #666; font-size: 14px; }}
                        .company-name {{ color: #4CAF50; font-weight: 600; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='success-icon'>✅</div>
                            <h1>Đơn hàng đã được vận chuyển!</h1>
                            <div class='order-id'>Đơn hàng #{order.Id}</div>
                        </div>
        
                        <div class='content'>
                            <div class='message'>
                                <p>Chào <strong>{order.Name}</strong>!</p>
                                <p>Đơn hàng của bạn đã được đóng gói và bắt đầu hành trình đến tay bạn. Dự kiến giao hàng trong <strong>2-3 ngày làm việc</strong>.</p>
                            </div>
            
                            <div class='order-details'>
                                <div class='section-title'>📚 Chi tiết đơn hàng</div>";

                            foreach (var detail in order.OrderDetails)
                            {
                                var book = detail.Book;
                                var itemTotal = (detail.Quantity ?? 0) * (book?.SecondPrice ?? 0);
                                var imageUrl = !string.IsNullOrEmpty(book?.ImageUrl) ? book.ImageUrl : "https://via.placeholder.com/60x80?text=Book";

                                html += $@"
                                <div class='book-item'>
                                    <img src='{imageUrl}' alt='{book?.BookName}' class='book-image' />
                                    <div class='book-info'>
                                        <div class='book-title'>{book?.BookName}</div>
                                        <div class='book-meta'>Số lượng: {detail.Quantity} × {book?.SecondPrice:N0}₫</div>
                                    </div>
                                    <div class='book-price'>{itemTotal:N0}₫</div>
                                </div>";
                            }

                            html += $@"
                            </div>
            
                            <div class='total-section'>
                                <div>Tổng cộng đơn hàng</div>
                                <div class='total-amount'>{total:N0}₫</div>
                            </div>
                        </div>
        
                        <div class='footer'>
                            <p>Cảm ơn bạn đã mua sắm tại <span class='company-name'>BookNest</span></p>
                            <p>Địa chỉ giao hàng: {order.Address}</p>
                            <p>Số điện thoại: {order.Phone}</p>
                        </div>
                    </div>
                </body>
                </html>";

            return html;
        }
    }
}
