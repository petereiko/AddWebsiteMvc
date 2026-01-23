using AddWebsiteMvc.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels.AdminDto
{
    public class SurveyChartDataDto
    {
        public List<ChartDataPoint> ResponsesOverTime { get; set; }
        public List<ChartDataPoint> CompletionFunnel { get; set; }
        public Dictionary<string, List<ChartDataPoint>> QuestionCharts { get; set; }
    }

}
