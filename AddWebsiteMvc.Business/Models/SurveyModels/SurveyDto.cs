using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class SurveyDto
    {
        public Guid SurveyId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ThankYouMessage { get; set; }
        public List<SurveyQuestionDto> Questions { get; set; }
    }
}
