using AddWebsiteMvc.Business.Common;
using AddWebsiteMvc.Business.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities.SurveyEntity
{
    public class SurveyQuestion:BaseEntity
    {

        [Required]
        public Guid SurveyId { get; set; }

        [Required]
        [MaxLength(500)]
        public string? QuestionText { get; set; }

        [Required]
        [MaxLength(50)]
        public QuestionType QuestionType { get; set; } // Text, MultipleChoice, Rating, YesNo, Scale

        public bool IsRequired { get; set; } = true;

        public int DisplayOrder { get; set; }

        public string? Options { get; set; } // JSON for multiple choice

        public int? MinValue { get; set; }

        public int? MaxValue { get; set; }

        // Navigation properties
        [ForeignKey("SurveyId")]
        public virtual Survey Survey { get; set; }
        public virtual ICollection<SurveyAnswer> Answers { get; set; }
    }
}
