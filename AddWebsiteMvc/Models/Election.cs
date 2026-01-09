namespace AddWebsiteMvc.Models
{
    public class Election
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool isActive { get; set; }
        public List<Candidate> contestants { get; set; } = new();
    }
}
