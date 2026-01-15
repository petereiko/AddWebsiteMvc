using AddWebsiteMvc.Business.Models.Vote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Election
{
    public class InitiateVoteDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid CandidateId { get; set; } = Guid.Empty;
        public string? Reference { get; set; }

        public List<VoteCategoryItem> CategoryItems { get; set; } = new();
    }
}
