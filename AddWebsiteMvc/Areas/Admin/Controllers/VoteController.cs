using AddWebsiteMvc.Business;
using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;
using AddWebsiteMvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VoteController : Controller
    {
        private readonly ICandidateService _candidateService;
        private readonly IVoteService _voteService;
        private readonly IGenericRepository<CandidateCategory> _candidateCategoryRepository;

        public VoteController(ICandidateService candidateService, IVoteService voteService, IGenericRepository<CandidateCategory> candidateCategoryRepository)
        {
            _candidateService = candidateService;
            _voteService = voteService;
            _candidateCategoryRepository = candidateCategoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string id, CancellationToken cancellationToken)
        {
            CandidateGridViewModel model = new();
            Guid candidateId = Guid.Parse(id);
            MessageResult<CandidateDto> candidate = await _candidateService.GetByIdAsync(candidateId, cancellationToken);
            model.Categories = (await _candidateCategoryRepository.FilterAsync(x=>x.CandidateId==candidateId, false, x => x.Category))
                .Select(x => new CategoryDto
                {
                    Id = x.CategoryId,
                    Name = x.Category.Name,
                }).ToList();
            model.Id = candidateId;
            return View(model);
        }


        [HttpPost]
        public async Task<JsonResult> Vote([FromBody]VoteRequest model, CancellationToken cancellationToken)
        {
            InitiateVoteDto initiateVoteModel = new() { CandidateId = Guid.Parse(model.CandidateId), Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, CategoryItems = model.CategoryItems };
            var result = await _voteService.AdminVote(initiateVoteModel, cancellationToken);

            return Json(result);

        }
    }
}
