using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Interfaces
{
    using AddWebsiteMvc.Business.Enums;
    using AddWebsiteMvc.Business.Models.SurveyModels.AdminDto;

    public interface ISurveyAdminService
        {
            Task<List<SurveyOverviewDto>> GetAllSurveysAsync();
            Task<SurveyAnalyticsDto> GetSurveyAnalyticsAsync(Guid surveyId);
            Task<SurveyResponsesPagedDto> GetSurveyResponsesAsync(Guid surveyId, int page, int pageSize);
            Task<ResponseDetailDto> GetResponseDetailAsync(Guid responseId);
            Task<ExportFileDto> ExportSurveyDataAsync(Guid surveyId);
            Task<SurveyChartDataDto> GetChartDataAsync(Guid surveyId);
        }
        
    
}
