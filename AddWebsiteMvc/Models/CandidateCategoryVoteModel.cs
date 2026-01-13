namespace AddWebsiteMvc.Models
{
    public class CandidateCategoryVoteModel
    {
        public string CandidateId { get; set; } = default!;
        public int CategoryId { get; set; }
        public int VoteCount { get; set; }
    }
}
