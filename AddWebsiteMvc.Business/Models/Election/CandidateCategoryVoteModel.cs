using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Election
{
    public class CandidateCategoryVoteModel
    {
        public Guid CandidateId { get; set; }
        public int CategoryId { get; set; }
        public int VoteCount { get; set; }
    }
}
