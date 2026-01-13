using Microsoft.AspNetCore.Mvc.Rendering;

namespace AddWebsiteMvc.Models
{
    public class Candidate:BaseModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string stateName { get; set; }
        public int stateId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string electionId { get; set; }
        public Election election { get; set; }
        public string passportFileName { get; set; }
        public IFormFile? passportFile { get; set; }
        public bool IsActive { get; set; }
        public int voteCount { get; set; }
        public decimal? votePrice { get; set; }
        public int order { get; set; }

        public List<SelectListItem> States { get; set; } = new();
    }
}
