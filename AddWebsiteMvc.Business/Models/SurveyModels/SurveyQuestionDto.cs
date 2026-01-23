using AddWebsiteMvc.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class SurveyQuestionDto
    {
        public Guid QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public List<string> Options { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
    }
}
