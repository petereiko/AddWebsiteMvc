using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Payment
{
    public class InitiateVoteResponse
    {
        public string AuthorizationUrl { get; set; } = default!;
        public string Reference { get; set; } = default!;
    }
}
