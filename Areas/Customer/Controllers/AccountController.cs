using BookNest.Data;
using BookNest.Models;
using BookNest.Models.MappingDBModel;
using BookNest.Models.ViewModel.AccountHandleViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Runtime;
using System.Security.Claims;

namespace BookNest.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AccountController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public AccountController(
            IMemoryCache cache,
            UserManager<AspNetUser> userManager,
            SignInManager<AspNetUser> signInManager,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _cache = cache;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Tài khoản không tồn tại";
                Console.WriteLine("User khong ton tai");
                return View(model);
            }

            if (!user.EmailConfirmed)
            {
                ViewBag.ErrorMessage = "Hãy xác nhận tài khoản qua email trước khi đăng nhập.";
                Console.WriteLine("Xac nhan tai khoan di");
                return View(model);
            }

            // Kiểm tra đăng nhập
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, true, false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Mật khẩu không đúng.";
                Console.WriteLine($"Login failed. Succeeded: {result.Succeeded}, IsLockedOut: {result.IsLockedOut}, IsNotAllowed: {result.IsNotAllowed}, RequiresTwoFactor: {result.RequiresTwoFactor}");

                return View(model);
            }
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var identityUser = new AspNetUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ProfileImage = "https://i.pinimg.com/originals/26/b1/59/26b159318870d081d2fa3efa1c36b74a.jpg"
                };

                try
                {
                    var result = await _userManager.CreateAsync(identityUser, model.Password);

                    if (result.Succeeded)
                    {

                        var roleResult = await _userManager.AddToRoleAsync(identityUser, "User");
                        if (!roleResult.Succeeded)
                        {
                            Console.WriteLine("Failed to add role - cleaning up user");
                            await _userManager.DeleteAsync(identityUser);
                            ViewBag.ErrorMessage = string.Join("<br/>", result.Errors.Select(e => e.Description));

                            return View(model);
                        }

                        // Bước 4: Gửi email
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                        var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                            new { userId = identityUser.Id, token }, Request.Scheme);

                        try
                        {
                            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailConfirmation.html");
                            string htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
                            htmlTemplate = htmlTemplate.Replace("{confirmationLink}", confirmationLink);

                            await _emailSender.SendEmailAsync(model.Email, "Xác nhận email", htmlTemplate);
                            Console.WriteLine("Email sent successfully");
                        }
                        catch (Exception emailEx)
                        {
                            Console.WriteLine($"Email sending failed but user created: {emailEx.Message}");
                        }

                        return RedirectToAction("Login", "Account");
                    }

                    ViewBag.ErrorMessage = string.Join("<br/>", result.Errors.Select(e => e.Description));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in registration: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult EnterEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnterEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Email không tồn tại";
                return View();
            }

            await SendEmail(email);

            HttpContext.Session.SetString("email", email);

            return RedirectToAction("EnterOTP");
        }

        [HttpGet]
        public IActionResult EnterOTP()
        {
            return View();
        }

        public async Task<IActionResult> EnterOTP(EnterOTPViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = HttpContext.Session.GetString("email");
            var user = await _userManager.FindByEmailAsync(email);

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Phiên làm việc hết hạn, vui lòng thử lại");
                return View(model);
            }

            var cachOtp = _cache.Get<string>(email);
            if (cachOtp == null)
            {
                ModelState.AddModelError("", "OTP đã hết hạn hoặc không hợp lệ");
                return View(model);
            }

            if (model.Otp != cachOtp)
            {
                ModelState.AddModelError("Otp", "OTP không đúng");
                return View(model);
            }

            _cache.Remove(email);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            HttpContext.Session.SetString("ResetToken", token);

            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            Console.WriteLine("OK4");

            if (ModelState.IsValid)
            {
                Console.WriteLine("OK2");

                string email = HttpContext.Session.GetString("email");

                if (email == null)
                {
                    ModelState.AddModelError("", "Hết phiên làm việc");
                    return RedirectToAction("Login");
                }

                Console.WriteLine("OK2");

                var user = await _userManager.FindByEmailAsync(email);
                var token = HttpContext.Session.GetString("ResetToken");
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

                if (result.Succeeded)
                {
                    Console.WriteLine("OK1");

                    HttpContext.Session.Remove("email");
                    HttpContext.Session.Remove("ResetToken");

                    Console.WriteLine(user.Id);

                    Console.WriteLine("OK");

                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }

            return View(model);
        }


        [HttpGet]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // Google callback trả về sau khi đăng nhập thành công
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction("Login");

            // Thử đăng nhập user với external login info
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
            if (signInResult.Succeeded)
            {
                // Đăng nhập thành công
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Nếu user chưa có thì tạo user mới
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email != null)
                {
                    // Tìm user trong db theo email
                    var existingUser = await _userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        // Liên kết external login nếu chưa liên kết
                        var logins = await _userManager.GetLoginsAsync(existingUser);
                        if (!logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
                        {
                            var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                        }
                        // Đăng nhập user này
                        await _signInManager.SignInAsync(existingUser, isPersistent: true);
                        return RedirectToAction("Index", "Home");
                    }

                    var identityUser = new AspNetUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname)
                    };
                    var createResult = await _userManager.CreateAsync(identityUser);
                    if (createResult.Succeeded)
                    {
                        var roleResult = await _userManager.AddToRoleAsync(identityUser, "User");

                        var addLoginResult = await _userManager.AddLoginAsync(identityUser, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(identityUser, isPersistent: false);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ViewBag.ErrorMessage = "Đăng nhập Google thất bại";
                return View("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return View("Error", new ErrorViewModel { Description = "Không tìm thấy người dùng." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            return View("Error", new ErrorViewModel { Description = "Xác nhận email thất bại." });
        }

        public async Task SendEmail(string email)
        {
            var rng = new Random();
            var otp = rng.Next(100000, 999999).ToString();

            _cache.Set(email, otp, TimeSpan.FromMinutes(5));

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "OTPConfirm.html");
            string htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
            htmlTemplate = htmlTemplate.Replace("{OTP}", otp);

            await _emailSender.SendEmailAsync(email, "OTP đổi mật khẩu", htmlTemplate);
        }

        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest();

            await SendEmail(email);
            TempData["email"] = email;
            return RedirectToAction("EnterOTP");
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Orders()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserFullName = $"{user.FirstName} {user.LastName}";

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.CreateAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmReceived(int orderId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var order = await _context.Orders
                        .Include(o => o.Payments)
                        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId.ToString() == userId && o.Status == "DELIVERED");

                    if (order == null)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Đơn hàng không hợp lệ" });
                    }

                    order.Status = "COMPLETED";
                    foreach (Payment payment in order.Payments)
                    {
                        payment.Status = "COMPLETED";
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Xác nhận nhận hàng thành công" });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw; 
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserReviews()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new List<object>());
            }

            try
            {
                // Lấy các đánh giá của user
                var reviews = await (from br in _context.BookRatings
                                     join bc in _context.BookComments on new { br.BookId, br.UserId } equals new { bc.BookId, bc.UserId } into comments
                                     from bc in comments.DefaultIfEmpty()
                                     join b in _context.Books on br.BookId equals b.Id
                                     where br.UserId.ToString() == userId
                                     select new
                                     {
                                         bookId = br.BookId,
                                         bookTitle = b.BookName,
                                         bookImage = b.ImageUrl,
                                         rating = br.Rating,
                                         content = bc != null ? bc.Content : "",
                                         createAt = br.CreateAt
                                     }).ToListAsync();

                return Json(reviews);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int orderId, int bookId, int rating, string content)
        {
            // Thêm logging để debug
            var userId = _userManager.GetUserId(User);

            // Log để debug
            Console.WriteLine($"AddReview called: OrderId={orderId}, BookId={bookId}, Rating={rating}, UserId={userId}");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User not authenticated");
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            // Validation input
            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Rating phải từ 1 đến 5 sao" });
            }

            try
            {
                // Kiểm tra đơn hàng có thuộc về user và đã hoàn thành
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId.ToString() == userId && o.Status == "COMPLETED");

                if (order == null)
                {
                    Console.WriteLine($"Order not found or not completed: OrderId={orderId}, UserId={userId}");
                    return Json(new { success = false, message = "Đơn hàng không hợp lệ hoặc chưa hoàn thành" });
                }

                // Kiểm tra sách có trong đơn hàng
                var orderDetail = order.OrderDetails.FirstOrDefault(od => od.BookId == bookId);
                if (orderDetail == null)
                {
                    Console.WriteLine($"Book not found in order: BookId={bookId}, OrderId={orderId}");
                    return Json(new { success = false, message = "Sách không có trong đơn hàng" });
                }

                // Kiểm tra đã đánh giá chưa
                var existingRating = await _context.BookRatings
                    .FirstOrDefaultAsync(br => br.BookId == bookId && br.UserId.ToString() == userId);

                if (existingRating != null)
                {
                    return Json(new { success = false, message = "Bạn đã đánh giá sách này rồi" });
                }

                // Thêm rating
                var bookRating = new BookRating
                {
                    BookId = bookId,
                    UserId = int.Parse(userId),
                    Rating = rating,
                    CreateAt = DateTime.Now
                };
                _context.BookRatings.Add(bookRating);

                // Thêm comment nếu có
                if (!string.IsNullOrEmpty(content))
                {
                    var comment = new BookComment
                    {
                        BookId = bookId,
                        UserId = int.Parse(userId),
                        Content = content,
                        CreateAt = DateTime.Now
                    };
                    _context.BookComments.Add(comment);
                }

                await _context.SaveChangesAsync();

                // Cập nhật rating trung bình cho sách
                var avgRating = await _context.BookRatings
                    .Where(br => br.BookId == bookId)
                    .AverageAsync(br => (double)br.Rating);

                var book = await _context.Books.FindAsync(bookId);
                if (book != null)
                {
                    book.Rating = (decimal)avgRating;
                    await _context.SaveChangesAsync();
                }

                Console.WriteLine("Review added successfully");
                return Json(new { success = true, message = "Đánh giá thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding review: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPendingReviews()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new List<object>());
            }

            try
            {
                // Lấy các sách từ đơn hàng đã hoàn thành nhưng chưa được đánh giá
                var pendingReviews = await (from od in _context.OrderDetails
                                            join o in _context.Orders on od.OrderId equals o.Id
                                            join b in _context.Books on od.BookId equals b.Id
                                            where o.UserId.ToString() == userId
                                                  && o.Status == "COMPLETED"
                                                  && !_context.BookRatings.Any(br => br.BookId == od.BookId && br.UserId.ToString() == userId)
                                            select new
                                            {
                                                orderId = o.Id,
                                                orderDate = o.CreateAt,
                                                bookId = b.Id,
                                                bookTitle = b.BookName,
                                                bookAuthor = b.Author,
                                                bookImage = b.ImageUrl,
                                                bookPrice = b.SecondPrice ?? 0
                                            }).ToListAsync();

                return Json(pendingReviews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending reviews: {ex.Message}");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAccountInfo()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                return Json(new
                {
                    email = user.Email,
                    firstName = user.FirstName, // Tùy thuộc vào model User của bạn
                    lastName = user.LastName,
                    phone = user.PhoneNumber,
                    image = user.ProfileImage // hoặc trường chứa ảnh đại diện
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

    }
}