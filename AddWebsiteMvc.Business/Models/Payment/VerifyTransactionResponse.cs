using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Payment
{
    public class VerifyTransactionResponse
    {
        public VerifyData? data { get; set; }
    }

    public class VerifyData
    {
        public string? status { get; set; }
        public string? message { get; set; }
    }

    

}
