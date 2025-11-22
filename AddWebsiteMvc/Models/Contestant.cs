namespace AddWebsiteMvc.Models
{
    public class Contestant:BaseModel
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string electionId { get; set; }
        public Election election { get; set; }
        public string dob { get; set; }
        public string passportFileName { get; set; }
        public IFormFile? passportFile { get; set; }
        public string videoFileName { get; set; }
        public bool IsActive { get; set; }
        public IFormFile? videoFile { get; set; }
        public string talent { get; set; }
        public string shortNote { get; set; }
        public int voteCount { get; set; }
        public decimal? votePrice { get; set; }
        public int order { get; set; }
    }
}
