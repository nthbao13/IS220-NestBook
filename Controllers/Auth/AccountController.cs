using BookNest.Data.AuthInfomation;
using BookNest.Models;
using BookNest.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace BookNest.Controllers.Auth
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IEmailSender _emailSender;

		public AccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager, 
			IEmailSender emailSender)
		{
			_emailSender = emailSender;
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View();
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
				var user = new ApplicationUser
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					Email = model.Email,
					PhoneNumber = model.Phone,
					RoleId = 1,
					UserName = model.Email
				};

				var result = await _userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

					var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
						new { userId = user.Id, token = token }, Request.Scheme);

                    string htmlTemplate = await System.IO.File.ReadAllTextAsync("Templates/EmailConfirmation.html");
                    htmlTemplate = htmlTemplate.Replace("{confirmationLink}", confirmationLink);

                    await _emailSender.SendEmailAsync(user.Email, "Xác nhận email", htmlTemplate);

                    return RedirectToAction("Login", "Account");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return View(model);
			}

			return View(model);
		}

		[HttpGet]
		public IActionResult EnterEmail()
		{
			return View();
		}

		[HttpGet]
		public IActionResult EnterOTP()
		{
			return View();
		}

		[HttpGet]
		public IActionResult ResetPassword() { return View(); }

		[HttpGet]
		public async Task<IActionResult> ConfirmEmail(int userId, string token)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null) return NotFound();

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				return View("Error");
			}
		}


    }

}
