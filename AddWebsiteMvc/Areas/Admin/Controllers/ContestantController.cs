using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ContestantController : Controller
    {
        private readonly ICandidateService _contestantService;
        private readonly IElectionService _electionService;

        public ContestantController(ICandidateService contestantService, IElectionService electionService)
        {
            _contestantService = contestantService;
            _electionService = electionService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var apiResult = await _contestantService.GetAllCandidatesAsync();
            List<Candidate> model = apiResult.data;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Candidate model = new();
            var electionResponse = await _electionService.GetActiveElectionAsync();
            model.election = electionResponse.data;
            model.electionId = electionResponse.data.id;

            GetAllStateResponse stateResponse = await _electionService.GetAllStatesAsync();
            if (stateResponse.statusCode == 200)
            {
                model.States = stateResponse.data.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
            }
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(10485760)]//10MB
        public async Task<IActionResult> Create(Candidate model, CancellationToken cancellationToken)
        {
            GetAllStateResponse? stateResponse = null;
            GetElectionResponse? electionResponse = null;
            if (model == null)
            {
                model = new() { Errors = new List<string>() { "Bad Request" } };
                electionResponse = await _electionService.GetActiveElectionAsync();
                model.election = electionResponse.data;
                model.electionId = electionResponse.data.id;

                stateResponse = await _electionService.GetAllStatesAsync();
                if (stateResponse.statusCode == 200)
                {
                    model.States = stateResponse.data.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                }

                return View(model);
            }
            var result = await _contestantService.AddAsync(model, cancellationToken);
            if (result.statusCode == 200) 
            {
                TempData["SuccessMessage"] = "Contestant enrolled successfully";
                return RedirectToAction("Index", "Contestant", new { area = "Admin" });
            }
            result.data = new() { Errors = result.errors };
            electionResponse = await _electionService.GetActiveElectionAsync();
            model.election = electionResponse.data;
            model.electionId = electionResponse.data.id;
            stateResponse = await _electionService.GetAllStatesAsync();
            if (stateResponse.statusCode == 200)
            {
                model.States = stateResponse.data.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
            }
            model.Errors.AddRange(result.data.Errors);
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            BaseResponse<Candidate> response = await _contestantService.GetCandidateByIdAsync(id);
            Candidate model = new();
            if (response.statusCode == 200) 
            {
                model = response.data;

                GetAllStateResponse stateResponse = await _electionService.GetAllStatesAsync();
                if (stateResponse.statusCode == 200)
                {
                    model.States = stateResponse.data.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                }
                var electionResponse = await _electionService.GetActiveElectionAsync();
                model.election = electionResponse.data;
                model.electionId = electionResponse.data.id;

                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Candidate model, CancellationToken cancellationToken)
        {
            var result = await _contestantService.EditAsync(model, cancellationToken);
            if (result.statusCode == 200)
            {
                TempData["SuccessMessage"] = "Contestant enrolled successfully";
                return RedirectToAction(nameof(Index));
            }
            GetAllStateResponse stateResponse = await _electionService.GetAllStatesAsync();
            if (stateResponse.statusCode == 200)
            {
                model.States = stateResponse.data.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
            }
            result.data = new() { Errors = result.errors };
            model.Errors.AddRange(result.data.Errors);
            var electionResponse = await _electionService.GetActiveElectionAsync();
            model.election = electionResponse.data;
            model.electionId = electionResponse.data.id;
            return View(model);
        }


        
    }
}
