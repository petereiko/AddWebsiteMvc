using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class SubmitSurveyViewModel
    {
        public string Token { get; set; }
        public List<SurveyAnswerViewModel> Answers { get; set; } = new();
    }
}
