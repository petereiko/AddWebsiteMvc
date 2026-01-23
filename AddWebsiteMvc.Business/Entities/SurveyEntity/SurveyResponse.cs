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
    public class SurveyResponse:BaseEntity
    {

        [Required]
        public Guid SurveyId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? UserEmail { get; set; }

        public Guid? VoteId { get; set; }

        [Required]
        public Guid ResponseToken { get; set; } = Guid.NewGuid();

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public bool IsCompleted { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(500)]
        public string UserAgent { get; set; }

        // Navigation properties
        [ForeignKey("SurveyId")]
        public virtual Survey Survey { get; set; }
        public virtual ICollection<SurveyAnswer> Answers { get; set; }
        public virtual SurveyEmailTracking EmailTracking { get; set; }
    }
}
