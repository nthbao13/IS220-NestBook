using BookNest.Models.MappingDBModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookNest.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "User")]
    public abstract class BaseCustomerController : Controller
    {
        protected string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        protected async Task<AspNetUser> GetCurrentUserAsync()
        {
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<AspNetUser>>();
            return await userManager.GetUserAsync(User);
        }
    }
}