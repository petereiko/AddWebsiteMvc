namespace AddWebsiteMvc.Models
{
    public class CandidateCategoryViewModel
    {
        public Guid CandidateId { get; set; }
        public string State { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;   
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<CheckboxListItem> Categories { get; set; } = new();
    }
}
