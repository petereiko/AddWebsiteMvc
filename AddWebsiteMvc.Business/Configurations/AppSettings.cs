using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Configurations
{
    public class AppSettings
    {
        public PayStack PayStack { get; set; } = default!;
    }

    public class PayStack
    {
        public string InitUrl { get; set; } = default!;
        public string Secret { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string VerifyUrl { get; set; } = default!;
        public string SubAccount { get; set; } = default!;
    }
}
