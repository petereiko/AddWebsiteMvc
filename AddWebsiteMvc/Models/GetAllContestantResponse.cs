namespace AddWebsiteMvc.Models
{
   
    public class GetAllContestantResponse:BaseResponse
    {
        public List<Contestant> data { get; set; } = new();
        public DateTime CloseDate { get; set; }
        
    }
}
