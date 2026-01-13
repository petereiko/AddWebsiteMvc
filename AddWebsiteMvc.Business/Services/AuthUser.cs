using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Persistence;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Services
{
    public class AuthUser : IAuthUser
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly VoteDbContext _context;

        public AuthUser(IHttpContextAccessor accessor, VoteDbContext context)
        {
            _accessor = accessor;
            _context = context;
        }
        public string UserName => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value!;
        public string Email => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value!;
        public Guid UserId => Guid.Parse(_accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        public string Roles => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value!;
        public string PriviledgedMenus => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Webpage)?.Value!;

        public string FullName => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Surname)?.Value!;
        
        public string BaseUrl
        {
            get
            {
                return $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            }
        }

    }
}
