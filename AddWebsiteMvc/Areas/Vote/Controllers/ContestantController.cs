using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Areas.Vote.Controllers
{
    [Area("Vote")]
    public class ContestantController : Controller
    {
        private readonly IContestantService _contestantService;
        private readonly IConfiguration _configuration;
        public ContestantController(IContestantService contestantService, IConfiguration configuration)
        {
            _contestantService = contestantService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var contestantsResult = await _contestantService.GetAllContestantsAsync();
            contestantsResult.data = contestantsResult.data.Where(x=>x.IsActive).Select(x => new Models.Contestant
            {
                dob = (DateTime.Now.Year - Convert.ToInt32(x.dob.Split('-')[0])).ToString(),
                firstName = x.firstName,
                lastName = x.lastName,
                id = x.id,
                electionId = x.electionId,
                IsActive = x.IsActive,
                passportFileName = $"{_configuration["BaseUrl"]}/passports/{x.passportFileName}",
                shortNote = x.shortNote,
                talent = x.talent,
                videoFileName = $"{_configuration["BaseUrl"]}/videos/{x.videoFileName}",
                voteCount=x.voteCount
            }).ToList();
            return View(contestantsResult);
        }


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var result = await _contestantService.GetContestantByIdAsync(id);
            var x = result.data;
            Contestant model = new()
            {
                dob = (DateTime.Now.Year - Convert.ToInt32(x.dob.Split('-')[0])).ToString(),
                firstName = x.firstName,
                lastName = x.lastName,
                id = x.id,
                electionId = x.electionId,
                IsActive = x.IsActive,
                passportFileName = $"{_configuration["BaseUrl"]}/passports/{x.passportFileName}",
                shortNote = x.shortNote,
                talent = x.talent,
                videoFileName = $"{_configuration["BaseUrl"]}/videos/{x.videoFileName}",
                voteCount = x.voteCount,
                votePrice = x.votePrice
            };
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Vote(VoteRequest model, CancellationToken cancellationToken)
        {
            var result = await _contestantService.VoteAsync(model, cancellationToken);

            return Json(result);
            
        }
    }
}
