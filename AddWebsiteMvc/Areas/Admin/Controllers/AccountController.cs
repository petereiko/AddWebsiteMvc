using AddWebsiteMvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public AccountController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public IActionResult Login()
        {
            LoginRequest model = new();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _configuration["LoginEndpoint"]);
            var payload = new
            {
                email = model.Email,
                password = model.Password
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), null, "application/json");
            request.Content = content;
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            LoginResponse? loginResponse = JsonConvert.DeserializeObject<LoginResponse>(json); ;
            if (loginResponse.statusCode==200)
            { 
                // Sign in the user
                await SignInUser(loginResponse.data.user, true, loginResponse.data.accessToken);

                // Redirect to return URL or home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Contestant", new { area = "Admin" });
            }
            else
            {
                ModelState.AddModelError(string.Empty, loginResponse.errors.FirstOrDefault() ?? "Invalid login attempt.");
                return View(model);
            }

        }


        private async Task SignInUser(UserData user, bool rememberMe, string token = null)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
            new Claim(ClaimTypes.Name, user.email),
            new Claim("FullName", user.firstName+" "+user.lastName)
        };

            // Add roles as claims
            if (user.userRoles != null)
            {
                foreach (var role in user.userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                }
            }

            // Optionally store the API token
            if (!string.IsNullOrEmpty(token))
            {
                claims.Add(new Claim("ApiToken", token));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(24),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
