namespace AddWebsiteMvc.Models
{
    public class CandidateCategoryModel
    {
        public string CandidateId { get; set; } = default!;
        public int CategoryId { get; set; }
        public string Category { get; set; } = default!;
    }
}
