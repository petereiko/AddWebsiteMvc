using AddWebsiteMvc.Business.Models.SurveyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Interfaces
{
    public interface IEmailService
    {
        //Task<bool> SendSurveyInvitationAsync(SendSurveyEmailRequest request, string surveyUrl);
        Task<bool> SendSurveyInvitationAsync(string userEmail, string userName, Guid voteId);

    }
}
