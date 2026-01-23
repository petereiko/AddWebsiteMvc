using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class SendSurveyEmailRequest
    {
        [Required]
        [EmailAddress]
        public string? UserEmail { get; set; }

        [Required]
        public Guid SurveyId { get; set; }

        public Guid VoteId { get; set; }

        public string? UserName { get; set; }
    }
}
