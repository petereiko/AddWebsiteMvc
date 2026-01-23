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
    public class SurveyEmailTracking:BaseEntity
    {

        

        [Required]
        [MaxLength(255)]
        public string? UserEmail { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? OpenedAt { get; set; }

        public DateTime? ClickedAt { get; set; }
        [MaxLength(500)]
        public string? Token { get; set; }

        
        public EmailStatus EmailStatus { get; set; } = EmailStatus.Sent; // Sent, Opened, Clicked, Completed, Failed

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        // Navigation properties
        [ForeignKey("ResponseId")]
        [Required]
        public Guid ResponseId { get; set; }
        public virtual SurveyResponse Response { get; set; }
    }
}
