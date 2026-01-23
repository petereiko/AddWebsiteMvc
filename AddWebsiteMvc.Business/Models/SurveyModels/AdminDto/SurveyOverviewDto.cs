using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels.AdminDto
{
    public class SurveyOverviewDto
    {
        public Guid SurveyId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalResponses { get; set; }
        public int CompletedResponses { get; set; }
        public int PendingResponses { get; set; }
        public decimal CompletionRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
