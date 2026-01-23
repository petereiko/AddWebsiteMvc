using AddWebsiteMvc.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels.AdminDto
{
    public class SurveyAnalyticsDto
    {
        public Guid SurveyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // Overall Stats
        public int TotalInvites { get; set; }
        public int EmailsOpened { get; set; }
        public int LinksClicked { get; set; }
        public int SurveysStarted { get; set; }
        public int SurveysCompleted { get; set; }

        // Rates
        public decimal OpenRate { get; set; }
        public decimal ClickRate { get; set; }
        public decimal StartRate { get; set; }
        public decimal CompletionRate { get; set; }

        // Timing
        public double AvgCompletionTimeMinutes { get; set; }
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? LastResponseAt { get; set; }

        // Question-level analytics
        public List<QuestionAnalyticsDto> Questions { get; set; }
    }

}
