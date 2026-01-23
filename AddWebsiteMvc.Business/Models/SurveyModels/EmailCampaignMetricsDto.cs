using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class EmailCampaignMetricsDto
    {
        public Guid SurveyId { get; set; }
        public string? Title { get; set; }
        public int EmailsSent { get; set; }
        public int EmailsOpened { get; set; }
        public int EmailsClicked { get; set; }
        public int SurveysCompleted { get; set; }
        public decimal OpenRate { get; set; }
        public decimal ClickRate { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
