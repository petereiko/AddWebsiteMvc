using AddWebsiteMvc.Business.Entities.Identity;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly IUserManagementService _userManagementService;

        public AccountController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequest model = new();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model, CancellationToken cancellationToken, string? returnUrl = null)
        {
            AuthResponse loginResponse = await _userManagementService.LoginAsync(model, cancellationToken);
            if (loginResponse.Success)
            {
               
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Contestant", new { area = "Admin" });
            }
            else
            {
                ModelState.AddModelError(string.Empty, loginResponse.Message!);
                return View(model);
            }
        }


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
