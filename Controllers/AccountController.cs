using BookNest.Data;
using BookNest.Models;
using BookNest.Models.MappingDBModel;
using BookNest.Models.ViewModel.AccountHandleViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime;
using System.Security.Claims;

namespace BookNest.Controllers
{
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
                };

                try
                {
                    // Bước 1: Tạo Identity User (không dùng transaction)
                    var result = await _userManager.CreateAsync(identityUser, model.Password);

                    if (result.Succeeded)
                    {

                        // Bước 2: Thêm role (không dùng transaction)
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

                    var identityUser = new AspNetUser { 
                        UserName = email, 
                        Email = email, 
                        EmailConfirmed = true, 
                        FirstName =  info.Principal.FindFirstValue(ClaimTypes.GivenName),
                        LastName =  info.Principal.FindFirstValue(ClaimTypes.Surname)
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

    }
}