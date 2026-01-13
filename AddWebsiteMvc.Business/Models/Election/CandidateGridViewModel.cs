using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.Election
{
    public class CandidateGridViewModel
    {
        public List<CandidateDto> Candidates { get; set; } = new();
        public List<CandidateCategoryModel> CandidateCategories { get; set; } = new();
        public List<CandidateCategoryVoteModel> CandidateCategoryVotes { get; set; } = new();
        public ElectionDto Election { get; set; } = default!;
        public int VotesCastToday { get; set; }
        public VotePriceDto VotePrice { get; set; } = default!;
        public List<CategoryDto> Categories { get; set; } = new();

    }
}
