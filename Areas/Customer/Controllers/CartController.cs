using BookNest.Data;
using BookNest.Data.DTO;
using BookNest.Models.MappingDBModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Areas.Customer.Controllers
{
    [Authorize]
    public class CartController : BaseCustomerController 
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AspNetUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<AspNetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = user.Id;

            List<Cart> Carts = _context.Carts
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToList();

            return View(Carts);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity([FromBody] UpdateCartRequest request)
        {
            if (request == null || request.ItemId <= 0 || request.Quantity < 1)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập!" });
            }

            var cartItem = _context.Carts
                .Include(c => c.Book)
                .FirstOrDefault(c => c.UserId == user.Id && c.Id == request.ItemId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng!" });
            }

            try
            {
                cartItem.Quantity = request.Quantity;
                await _context.SaveChangesAsync();

                var newTotal = await _context.Carts
                    .Where(c => c.UserId == user.Id)
                    .Include(c => c.Book)
                    .SumAsync(c => (c.Book.SecondPrice ?? c.Book.FirstPrice) * c.Quantity);

                return Json(new
                {
                    success = true,
                    message = "Cập nhật thành công",
                    newTotal
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveCartRequest request)
        {
            if (request == null || request.ItemId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập!" });
            }

            var cartItem = _context.Carts
                .FirstOrDefault(c => c.UserId == user.Id && c.Id == request.ItemId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng!" });
            }

            try
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa sản phẩm!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
            {
            if (request == null || request.productId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập!" });
            }

            try
            {
                _context.Carts.Add(new Cart
                {
                    UserId = user.Id,
                    BookId = request.productId,
                    Quantity = request.quantity
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã tạo sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo sản phẩm!" });
            }
        }
    }
}
