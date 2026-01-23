using AddWebsiteMvc.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels.AdminDto
{
    public class SurveyResponsesPagedDto
    {
        public Guid SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public List<ResponseSummaryDto> Responses { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalResponses { get; set; }
        public int TotalPages { get; set; }
    }

}
