using AddWebsiteMvc.Business.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities.SurveyEntity
{
    public class SurveyAnswer:BaseEntity
    {

        [Required]
        public Guid ResponseId { get; set; }

        [Required]
        public Guid QuestionId { get; set; }

        public string? AnswerText { get; set; }

        public int? AnswerNumeric { get; set; }

        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ResponseId")]
        public virtual SurveyResponse Response { get; set; }

        [ForeignKey("QuestionId")]
        public virtual SurveyQuestion Question { get; set; }
    }
}
