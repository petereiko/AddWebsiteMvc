using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities
{
    public class CandidateCategory
    {
        public int Id { get; set; }
        public Guid CandidateId { get; set; }
        public virtual Candidate Candidate { get; set; } = default!;
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = default!;
    }
}
