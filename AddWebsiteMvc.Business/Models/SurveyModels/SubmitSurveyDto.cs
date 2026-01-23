using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class SubmitSurveyDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public List<AnswerDto> Answers { get; set; }
    }
}
