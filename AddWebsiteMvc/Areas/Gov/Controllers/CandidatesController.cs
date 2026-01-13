using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;
using AddWebsiteMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace AddWebsiteMvc.Areas.Gov.Controllers
{
    [Area("Gov")]
    public class CandidatesController : Controller
    {
        private readonly ICandidateService _candidateService;
        private readonly IConfiguration _configuration;
        private readonly IVoteService _voteService;
        public CandidatesController(ICandidateService candidateService, IConfiguration configuration, IVoteService voteService)
        {
            _candidateService = candidateService;
            _configuration = configuration;
            _voteService = voteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            MessageResult<CandidateGridViewModel> model = await _candidateService.FetchCandidatesAsync(cancellationToken);
            Parallel.ForEach(model.Data.Candidates, candidate =>
            {
                candidate.PassportFileName = $"{_configuration["BaseUrl"]}/passports/{candidate.PassportFileName}";
            });

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var result = await _candidateService.GetByIdAsync(id, cancellationToken);
            var x = result.Data;
            Candidate model = new()
            {
                firstName = x.FirstName,
                lastName = x.LastName,
                id = x.Id,
                electionId = x.ElectionId,
                IsActive = x.IsActive,
                passportFileName = $"{_configuration["BaseUrl"]}/passports/{x.PassportFileName}",
                voteCount = x.VoteCount,
                votePrice = x.votePrice,
                title = x.Title,
                stateId = x.StateId,
                stateName = x.StateName
            };
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Vote(VoteRequest model, CancellationToken cancellationToken)
        {
            InitiateVoteDto initiateVoteModel = new() { CandidateId= Guid.Parse(model.candidateId), Count = model.count, Email = model.email, FirstName = model.firstName, LastName = model.lastName, CategoryId = model.categoryId };
            var result = await _voteService.InitiateVote(initiateVoteModel, cancellationToken);

            return Json(result);

        }

        public async Task<JsonResult> LoadCandidates(CancellationToken cancellationToken)
        {
            MessageResult<IEnumerable<CandidateDto>> response = await _candidateService.GetAllAsync(cancellationToken);
            return Json(response);
        }
    }
}
