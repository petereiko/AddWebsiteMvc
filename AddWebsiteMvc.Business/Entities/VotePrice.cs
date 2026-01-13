using AddWebsiteMvc.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities
{
    public class VotePrice:BaseEntity
    {
        public decimal Price { get; set; }
        public Guid ElectionId { get; set; }
        public virtual Election Election { get; set; } = default!;
    }
}
