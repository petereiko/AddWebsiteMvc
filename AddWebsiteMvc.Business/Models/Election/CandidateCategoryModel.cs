using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Election
{
    public class CandidateCategoryModel
    {
        public Guid CandidateId { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; } = default!;
    }
}
