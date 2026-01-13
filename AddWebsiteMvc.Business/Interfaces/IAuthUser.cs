using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Interfaces
{
    public interface IAuthUser
    {
        string UserName { get; }
        string Email { get; }
        Guid UserId { get; }
        string Roles { get; }
        string FullName { get; }
        string BaseUrl { get; }
    }
}
