using AddWebsiteMvc.Business;
using AddWebsiteMvc.Business.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AddWebsiteMvc
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public CustomClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor
            )
        : base(userManager, roleManager, optionsAccessor)
        {
            _userManager = userManager;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            string commaSeparatedRoles = string.Join(",", userRoles);
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.Surname, $"{user.LastName} {user.FirstName}"));
            identity.AddClaim(new Claim("Role", commaSeparatedRoles));
            return identity;
        }
    }
}
