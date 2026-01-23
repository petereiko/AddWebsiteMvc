using AddWebsiteMvc.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels.AdminDto
{
    public class QuestionAnalyticsDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int TotalAnswers { get; set; }

        // For multiple choice / yes-no
        public Dictionary<string, int> OptionCounts { get; set; }

        // For scale / rating
        public double? AverageScore { get; set; }
        public Dictionary<int, int> ScoreDistribution { get; set; }

        // For text responses
        public List<string> TextResponses { get; set; }
    }

}
