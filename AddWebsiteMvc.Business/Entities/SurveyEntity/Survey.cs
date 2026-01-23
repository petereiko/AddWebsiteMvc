using AddWebsiteMvc.Business.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Entities.SurveyEntity
{
    public class Survey:BaseEntity
    {

        [Required]
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        public string? ThankYouMessage { get; set; }

        public bool AllowMultipleResponses { get; set; }

        // Navigation properties
        public virtual ICollection<SurveyQuestion> Questions { get; set; }
        public virtual ICollection<SurveyResponse> Responses { get; set; }
    }
}
