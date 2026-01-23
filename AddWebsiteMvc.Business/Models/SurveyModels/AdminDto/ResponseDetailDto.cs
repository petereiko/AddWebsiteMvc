using AddWebsiteMvc.Business.Enums;
using AddWebsiteMvc.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels.AdminDto
{
    public class ResponseDetailDto
    {
        public Guid ResponseId { get; set; }
        public Guid SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public string UserEmail { get; set; }
        public Guid? VoteId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? CompletionTime { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        // Email tracking
        public DateTime? EmailSentAt { get; set; }
        public DateTime? EmailOpenedAt { get; set; }
        public DateTime? EmailClickedAt { get; set; }
        public EmailStatus EmailStatus { get; set; }

        // Answers
        public List<AnswerDetailDto> Answers { get; set; }
    }

}
