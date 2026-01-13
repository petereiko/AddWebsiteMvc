using AddWebsiteMvc.Business.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Interfaces
{
    public interface IUserManagementService
    {
        Task<AuthResponse> LoginAsync(LoginRequest model, CancellationToken cancellationToken);
        Task<AuthResponse> Register(RegisterRequest model, CancellationToken cancellationToken);
    }
}
