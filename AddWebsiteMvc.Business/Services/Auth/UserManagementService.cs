using AddWebsiteMvc.Business;
using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Entities.Identity;
using AddWebsiteMvc.Business.Enums;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace AddWebsiteMvc.Business.Services.Auth
{
    public class UserManagementService:IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthUser> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Entities.Election> _electionRepository;
        public UserManagementService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AuthUser> logger, IHttpContextAccessor httpContextAccessor, IGenericRepository<Entities.Election> electionRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _electionRepository = electionRepository;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest model, CancellationToken cancellationToken)
        {
            AuthResponse response = new ();
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null || !user.IsActive)
                {
                    response.Message = "Invalid credentials";
                    return response;
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (!result.Succeeded)
                {
                    response.Message = "Invalid credentials";
                    return response;
                }

                await _signInManager.SignInAsync(user, true);

                response.User = user;
                response.Success = true;
                response.Message = "Login successful";
                _logger.LogInformation("User {Email} logged in successfully", model.Email);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.Message, ex);
            }
            return response;
        }

        public async Task<AuthResponse> Register(RegisterRequest model, CancellationToken cancellationToken)
        {
            AuthResponse response = new();
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    response.Message = result.Errors.Select(e => e.Description).FirstOrDefault();
                    return response;
                }

                // Add default User role
                await _userManager.AddToRoleAsync(user, EnumHelper.GetEnumDescription(model.Role));

                _logger.LogInformation("User {Email} registered successfully", model.Email);
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.Message = ex.Message;
            }
            
            return response;
        }
    }
}
