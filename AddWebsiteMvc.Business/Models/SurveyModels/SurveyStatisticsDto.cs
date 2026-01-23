using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class SurveyStatisticsDto
    {
        public Guid SurveyId { get; set; }
        public string? Title { get; set; }
        public int TotalResponses { get; set; }
        public int CompletedResponses { get; set; }
        public int IncompleteResponses { get; set; }
        public decimal CompletionRate { get; set; }
        public int AvgCompletionTimeSeconds { get; set; }
    }
}
