using AddWebsiteMvc.Business.Models.Vote;
using System.Drawing;

namespace AddWebsiteMvc.Models
{
    public class VoteRequest
    {
        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string CandidateId { get; set; } = default!;

        public List<VoteCategoryItem> CategoryItems { get; set; } = new();
    }


    
}
