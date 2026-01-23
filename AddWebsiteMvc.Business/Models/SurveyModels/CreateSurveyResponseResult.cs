using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class CreateSurveyResponseResult
    {
        public Guid ResponseId { get; set; }
        public Guid ResponseToken { get; set; }
        public string? SurveyUrl { get; set; }
    }
}
