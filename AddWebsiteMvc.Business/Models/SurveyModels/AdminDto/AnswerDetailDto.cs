using AddWebsiteMvc.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels.AdminDto
{
    public class AnswerDetailDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string AnswerText { get; set; }
        public int? AnswerNumeric { get; set; }
        public DateTime AnsweredAt { get; set; }
    }

}
