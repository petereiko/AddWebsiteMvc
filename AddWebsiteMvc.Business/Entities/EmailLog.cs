using AddWebsiteMvc.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities
{
    public class EmailLog:BaseEntity
    {
        public string? Email { get; set; }
        public string? Message { get; set; }
        public string? Subject { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentDate { get; set; }
    }
}
