using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using System.Security.Claims;

namespace AddWebsiteMvc.Services
{
    public class AuthUser : IAuthUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
        public string Email => User?.FindFirst(ClaimTypes.Email)?.Value;

        public string FullName => User?.FindFirst("FullName")?.Value;

        public string Token => User?.FindFirst("ApiToken")?.Value;

        public string UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public List<string> GetRoles()
        {
            return User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? new List<string>();
        }

        public bool IsInRole(string role)
        {
            return User?.IsInRole(role) ?? false;
        }

        public bool IsAuthenticated()
        {
            return User?.Identity?.IsAuthenticated ?? false;
        }

        public string GetClaim(string claimType)
        {
            return User?.FindFirst(claimType)?.Value;
        }

    }
}
