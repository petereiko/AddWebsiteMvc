using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ContestantController : Controller
    {
        private readonly IContestantService _contestantService;
        private readonly IElectionService _electionService;

        public ContestantController(IContestantService contestantService, IElectionService electionService)
        {
            _contestantService = contestantService;
            _electionService = electionService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var apiResult = await _contestantService.GetAllContestantsAsync();
            List<Contestant> model = apiResult.data;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Contestant model = new();
            var electionResponse = await _electionService.GetActiveElectionAsync();
            model.election = electionResponse.data;
            model.electionId = electionResponse.data.id;
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(10485760)]//10MB
        public async Task<IActionResult> Create(Contestant model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                model = new() { Errors = new List<string>() { "Bad Request" } };
                var electionResponse = await _electionService.GetActiveElectionAsync();
                model.election = electionResponse.data;
                model.electionId = electionResponse.data.id;
                return View(model);
            }
            var result = await _contestantService.AddAsync(model, cancellationToken);
            if (result.statusCode == 200) 
            {
                TempData["SuccessMessage"] = "Contestant enrolled successfully";
                return RedirectToAction(nameof(Index));
            }
            result.data = new() { Errors = result.errors };
            model.Errors.AddRange(result.data.Errors);
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            BaseResponse<Contestant> response = await _contestantService.GetContestantByIdAsync(id);
            Contestant model = new();
            if (response.statusCode == 200) 
            {
                model = response.data;
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Contestant model, CancellationToken cancellationToken)
        {
            var result = await _contestantService.EditAsync(model, cancellationToken);
            if (result.statusCode == 200)
            {
                TempData["SuccessMessage"] = "Contestant enrolled successfully";
                return RedirectToAction(nameof(Index));
            }
            result.data = new() { Errors = result.errors };
            model.Errors.AddRange(result.data.Errors);
            return View(model);
        }
    }
}
