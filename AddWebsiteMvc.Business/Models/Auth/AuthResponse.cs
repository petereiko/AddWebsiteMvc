using AddWebsiteMvc.Business.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Auth
{
    public class AuthResponse
    {
        public ApplicationUser? User { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; } = default!;
    }
}
