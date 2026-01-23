using AddWebsiteMvc.Business.Entities.SurveyEntity;
using AddWebsiteMvc.Business.Models.SurveyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Interfaces
{
    public interface ISurveyService
    {
        Task<SurveyDto> GetSurveyAsync(Guid surveyId);
        Task<SurveyDto> GetSurveyByTokenAsync(string token);
        Task<bool> HasUserRespondedAsync(Guid surveyId, string userEmail);
        Task<bool> SubmitSurveyAsync(SubmitSurveyDto submitDto);
        Task<SurveyStatisticsDto> GetSurveyStatisticsAsync(Guid surveyId);
        Task<List<SurveyAnswer>> GetResponseAnswersAsync(Guid responseId);
        Task<SurveyDto> GetSurveyByVoteIdAsync(Guid voteId);
        Task TrackEmailOpenAsync(string token);
        Task TrackEmailClickAsync(string token, string ipAddress, string userAgent);
    }
}
