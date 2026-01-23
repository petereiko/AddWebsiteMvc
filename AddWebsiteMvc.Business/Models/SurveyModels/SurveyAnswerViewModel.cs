using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class SurveyAnswerViewModel
    {
        public Guid QuestionId { get; set; }
        public string? AnswerText { get; set; }
        public int? AnswerNumeric { get; set; }
    }
}
