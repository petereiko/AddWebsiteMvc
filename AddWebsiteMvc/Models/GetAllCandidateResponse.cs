namespace AddWebsiteMvc.Models
{
   
    public class GetAllCandidateResponse:BaseResponse
    {
        public List<Candidate> data { get; set; } = new();
        public DateTime CloseDate { get; set; }
        public decimal unitVotePrice { get; set; }
        public int VotesCastToday { get; set; }
        public string ElectionTitle { get; set; } = default!;

    }
}
