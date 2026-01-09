using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace AddWebsiteMvc.Areas.Gov.Controllers
{
    [Area("Gov")]
    public class CandidatesController : Controller
    {
        private readonly ICandidateService _candidateService;
        private readonly IConfiguration _configuration;
        public CandidatesController(ICandidateService candidateService, IConfiguration configuration)
        {
            _candidateService = candidateService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            GetAllCandidateResponse candidatesResult = await _candidateService.GetAllCandidatesAsync();
            candidatesResult.data = candidatesResult.data.Where(x => x.IsActive).Select(x => new Models.Candidate
            {
                firstName = x.firstName,
                lastName = x.lastName,
                id = x.id,
                electionId = x.electionId,
                IsActive = x.IsActive,
                passportFileName = $"/passports/{x.passportFileName}",
                voteCount = x.voteCount,
                title = x.title,
                stateId = x.stateId,
                stateName = x.stateName
            })/*.OrderBy(x=>x.shortNote).OrderByDescending(x=>x.voteCount)*/.ToList();
            return View(candidatesResult);
        }


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var result = await _candidateService.GetCandidateByIdAsync(id);
            var x = result.data;
            Candidate model = new()
            {
                firstName = x.firstName,
                lastName = x.lastName,
                id = x.id,
                electionId = x.electionId,
                IsActive = x.IsActive,
                passportFileName = $"{_configuration["BaseUrl"]}/passports/{x.passportFileName}",
                voteCount = x.voteCount,
                votePrice = x.votePrice,
                title = x.title,
                stateId = x.stateId,
                stateName = x.stateName
            };
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Vote(VoteRequest model, CancellationToken cancellationToken)
        {
            var result = await _candidateService.VoteAsync(model, cancellationToken);

            return Json(result);

        }

        public async Task<JsonResult> LoadCandidates()
        {
            GetAllCandidateResponse response = await _candidateService.GetAllCandidatesAsync();
            return Json(response);
        }
    }
}
