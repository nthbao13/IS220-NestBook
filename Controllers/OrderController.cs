using BookNest.Data;
using BookNest.Data.DTO;
using BookNest.Models.MappingDBModel;
using BookNest.Models.ViewModel;
using BookNest.Models.VnPayModel;
using BookNest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IVnPayService _vnPayService;

        public OrderController(
            ApplicationDbContext context,
            UserManager<AspNetUser> userManager,
            IVnPayService vnPayService)
        {
            _context = context;
            _userManager = userManager;
            _vnPayService = vnPayService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            if (request == null || request.Products == null || !request.Products.Any())
            {
                TempData["OrderMessage"] = "Bạn chưa chọn sản phẩm nào để đặt hàng.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["OrderMessage"] = "Chức năng này yêu cầu đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = new Order
                {
                    UserId = user.Id,
                    Status = "PENDING",
                    CreateAt = DateTime.Now,
                    From = request.From
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in request.Products)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        BookId = item.productId,
                        Quantity = item.quantity
                    };
                    _context.OrderDetails.Add(orderDetail);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Tạo đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Tạo đơn hàng thất bại: " + ex.Message
                });
            }
        }

        public async Task<IActionResult> Payment()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Order Order = _context.Orders.Where(o => o.Status == "PENDING")
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderDetails)
                .ThenInclude(o => o.Book)
                .FirstOrDefault();

            if (Order == null)
            {
                TempData["OrderMessage"] = "Có lỗi khi tạo đơn hàng";
                return RedirectToAction("Index", "Cart");
            }

            PaymentViewModel PaymentViewModel = new PaymentViewModel
            {
                OrderId = Order.Id,
                OrderDetails = Order.OrderDetails.ToList()
            };

            return View(PaymentViewModel);
        }

        [HttpPost]
        [Route("Order/CancelOrder/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);
                if (order == null)
                {
                    return NotFound();
                }

                if (order.Status == "PENDING")
                {
                    order.Status = "CANCELLED";
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Đơn hàng đã được hủy." });
            }
            catch (Exception ex)
            {
                return Ok();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckVoucher([FromBody] VoucherRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.VoucherCode))
                {
                    return Json(new { success = false, message = "Mã voucher không được để trống" });
                }

                var voucher = await _context.Vouchers
                    .FirstOrDefaultAsync(v => v.VoucherCode == request.VoucherCode);

                if (voucher == null)
                {
                    return Json(new { success = false, message = "Mã voucher không tồn tại" });
                }

                // Kiểm tra voucher còn hiệu lực
                if (voucher.ExpiredAt.HasValue && voucher.ExpiredAt < DateTime.Now)
                {
                    return Json(new { success = false, message = "Mã voucher đã hết hạn" });
                }

                // Kiểm tra số lần sử dụng
                if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit)
                {
                    return Json(new { success = false, message = "Mã voucher đã hết lượt sử dụng" });
                }

                // Tính toán số tiền giảm giá
                decimal discountAmount = CalculateDiscount(voucher);

                return Json(new
                {
                    success = true,
                    message = "Voucher hợp lệ",
                    discountAmount = discountAmount,
                    voucherId = voucher.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra voucher" });
            }
        }

        // Helper methods
        private decimal CalculateDiscount(Voucher voucher)
        {
            decimal subtotal = GetOrderSubtotal();
            return CalculateDiscountAmount(voucher, subtotal);
        }

        private decimal CalculateDiscountForOrder(Voucher voucher, Order order)
        {
            decimal subtotal = order.OrderDetails.Sum(od => (od.Book?.SecondPrice ?? 0) * (od.Quantity ?? 0));
            return CalculateDiscountAmount(voucher, subtotal);
        }

        private decimal CalculateDiscountAmount(Voucher voucher, decimal subtotal)
        {
            if (voucher.Type == true) // Percentage discount
            {
                decimal discountAmount = subtotal * (decimal)(voucher.Value ?? 0);
                return discountAmount;
            }
            else // Fixed amount discount
            {
                return (decimal)(voucher.Value ?? 0);
            }
        }

        private bool IsVoucherValid(Voucher voucher)
        {
            if (voucher.ExpiredAt.HasValue && voucher.ExpiredAt < DateTime.Now)
                return false;

            if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit)
                return false;

            return true;
        }

        private decimal GetOrderSubtotal()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var order = _context.Orders
                .Where(o => o.Status == "PENDING" && o.UserId == user.Id)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .FirstOrDefault();

            return order?.OrderDetails?.Sum(od => (od.Book?.SecondPrice ?? 0) * (od.Quantity ?? 0)) ?? 0;
        }

        private decimal CalculateOrderTotal(Order order)
        {
            decimal subtotal = order.OrderDetails.Sum(od => (od.Book?.SecondPrice ?? 0) * (od.Quantity ?? 0));
            decimal shipping = 25000; // Updated to match view
            decimal discount = 0;

            foreach (var voucher in order.Vouchers)
            {
                discount += CalculateDiscountForOrder(voucher, order);
            }

            return subtotal + shipping - discount;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Get the pending order
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                    .Include(o => o.Vouchers)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == user.Id && o.Status == "PENDING");

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Update order information
                order.Name = request.Name;
                order.Phone = request.Phone;
                order.Address = request.Address;

                // Apply voucher if provided
                if (!string.IsNullOrEmpty(request.VoucherCode))
                {
                    var voucher = await _context.Vouchers
                        .FirstOrDefaultAsync(v => v.VoucherCode == request.VoucherCode);

                    if (voucher != null && IsVoucherValid(voucher))
                    {
                        // Check if voucher is already applied to this order
                        if (!order.Vouchers.Any(v => v.Id == voucher.Id))
                        {
                            order.Vouchers.Add(voucher);
                            // Increment usage count
                            voucher.UsedCount = (voucher.UsedCount ?? 0) + 1;
                            _context.Vouchers.Update(voucher);
                        }
                    }
                }

                // Calculate total amount
                decimal totalAmount = CalculateOrderTotal(order);

                // Determine payment type (1 = COD, 2 = VNPay)
                int paymentTypeId = request.PaymentMethod == "vnpay" ? 2 : 1;

                // Generate unique transaction reference - FIXED: Use consistent format
                string txnRef = DateTime.Now.Ticks.ToString();

                // Create payment record
                var payment = new Payment
                {
                    OrderId = order.Id,
                    PaymentTypeId = paymentTypeId,
                    TotalPrice = (int)totalAmount,
                    Status = "PENDING",
                    CreateAt = DateTime.Now,
                    TransactionRef = txnRef // Store the transaction reference
                };

                _context.Payments.Add(payment);
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                if (request.PaymentMethod == "vnpay")
                {
                    // Create VNPay payment URL with the same transaction reference
                    var paymentInfo = new PaymentInformationModel
                    {
                        OrderType = "other",
                        Amount = (double)totalAmount,
                        Name = request.Name,
                        OrderDescription = "Thanh toán đơn hàng BookNest",
                        TxnRef = txnRef  // Pass the transaction reference to ensure consistency
                    };

                    var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);

                    return Json(new
                    {
                        success = true,
                        paymentMethod = "vnpay",
                        paymentUrl = paymentUrl
                    });
                }
                else
                {
                    // COD payment - complete the order immediately
                    await CompleteOrder(order.Id);

                    return Json(new
                    {
                        success = true,
                        paymentMethod = "cod",
                        message = "Đặt hàng thành công"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            try
            {
                // Log all query parameters for debugging
                var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                Console.WriteLine("=== VNPay Callback Debug Info ===");
                foreach (var param in queryParams)
                {
                    Console.WriteLine($"{param.Key}: {param.Value}");
                }

                var response = _vnPayService.PaymentExecute(Request.Query);
                var txnRef = Request.Query["vnp_TxnRef"].ToString();

                Console.WriteLine($"TxnRef from VNPay: {txnRef}");
                Console.WriteLine($"VNPay Response Success: {response.Success}");
                Console.WriteLine($"VNPay Response Code: {response.VnPayResponseCode}");

                if (string.IsNullOrEmpty(txnRef))
                {
                    Console.WriteLine("ERROR: TxnRef is null or empty");
                    TempData["ErrorMessage"] = "Không thể xác định giao dịch";
                    return RedirectToAction("PaymentFailure");
                }

                // Find the payment using the stored transaction reference
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.TransactionRef == txnRef && p.Status == "PENDING");

                Console.WriteLine($"Payment found: {payment != null}");
                if (payment != null)
                {
                    Console.WriteLine($"Payment ID: {payment.Id}, Order ID: {payment.OrderId}, Status: {payment.Status}");
                }
                else
                {
                    // Debug: Check all payments to see what transaction refs exist
                    var allPendingPayments = await _context.Payments
                        .Where(p => p.Status == "PENDING")
                        .Select(p => new { p.Id, p.TransactionRef, p.OrderId })
                        .ToListAsync();

                    Console.WriteLine("=== All Pending Payments ===");
                    foreach (var p in allPendingPayments)
                    {
                        Console.WriteLine($"Payment ID: {p.Id}, TxnRef: {p.TransactionRef}, OrderID: {p.OrderId}");
                    }
                }

                if (payment == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán";
                    return RedirectToAction("PaymentFailure");
                }

                if (response.Success && response.VnPayResponseCode == "00")
                {
                    Console.WriteLine("Payment successful - updating status");

                    // Payment successful
                    payment.Status = "COMPLETED";
                    payment.VnpTransactionNo = Request.Query["vnp_TransactionNo"].ToString();
                    payment.VnpResponseCode = response.VnPayResponseCode;
                    _context.Payments.Update(payment);

                    // Complete the order
                    await CompleteOrder(payment.OrderId.Value);

                    TempData["SuccessMessage"] = "Thanh toán thành công";
                    return RedirectToAction("Success", new { id = payment.OrderId });
                }
                else
                {
                    Console.WriteLine($"Payment failed - Response code: {response.VnPayResponseCode}");

                    // Payment failed
                    payment.Status = "FAILED";
                    payment.VnpResponseCode = response.VnPayResponseCode;
                    payment.Order.Status = "CANCELLED";

                    _context.Payments.Update(payment);
                    _context.Orders.Update(payment.Order);
                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] = $"Thanh toán thất bại. Mã lỗi: {response.VnPayResponseCode}";
                    return RedirectToAction("PaymentFailure");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== Exception in PaymentCallbackVnpay ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Return a simple error page instead of redirect to avoid further issues
                ViewBag.ErrorMessage = $"Có lỗi xảy ra trong quá trình xử lý thanh toán: {ex.Message}";
                return View("PaymentFailure");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        public IActionResult PaymentFailure()
        {
            return View();
        }

        private async Task CompleteOrder(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    Console.WriteLine($"CompleteOrder: Order {orderId} not found");
                    return;
                }

                Console.WriteLine($"CompleteOrder: Processing order {orderId}");

                // Update order status
                order.Status = "COMPLETED";

                // Reduce book quantities
                foreach (var orderDetail in order.OrderDetails)
                {
                    if (orderDetail.Book != null)
                    {
                        var currentQuantity = orderDetail.Book.Quantity ?? 0;
                        var orderQuantity = orderDetail.Quantity ?? 0;

                        orderDetail.Book.Quantity = Math.Max(0, currentQuantity - orderQuantity);
                        _context.Books.Update(orderDetail.Book);

                        Console.WriteLine($"Updated book {orderDetail.BookId}: quantity {currentQuantity} -> {orderDetail.Book.Quantity}");
                    }
                }

                // Clear user's cart for items that were ordered (only if order came from cart)
                if (order.From == 1)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var bookIds = order.OrderDetails.Select(od => od.BookId).ToList();
                        var cartItems = await _context.Carts
                            .Where(c => c.UserId == user.Id && bookIds.Contains(c.BookId))
                            .ToListAsync();

                        Console.WriteLine($"Found {cartItems.Count} cart items to update");

                        foreach (var cartItem in cartItems)
                        {
                            var orderedQuantity = order.OrderDetails
                                .FirstOrDefault(od => od.BookId == cartItem.BookId)?.Quantity ?? 0;

                            if (cartItem.Quantity <= orderedQuantity)
                            {
                                _context.Carts.Remove(cartItem);
                                Console.WriteLine($"Removed cart item for book {cartItem.BookId}");
                            }
                            else
                            {
                                cartItem.Quantity -= orderedQuantity;
                                _context.Carts.Update(cartItem);
                                Console.WriteLine($"Updated cart item for book {cartItem.BookId}: quantity reduced by {orderedQuantity}");
                            }
                        }
                    }
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                Console.WriteLine($"CompleteOrder: Successfully completed order {orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CompleteOrder Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw; // Re-throw to be handled by caller
            }
        }
    }
}