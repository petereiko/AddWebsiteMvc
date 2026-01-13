using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;
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
        private readonly ICandidateService _candidateService;
        private readonly IElectionService _electionService;

        public ContestantController(ICandidateService contestantService, IElectionService electionService)
        {
            _candidateService = contestantService;
            _electionService = electionService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            MessageResult<IEnumerable<CandidateDto>> result = await _candidateService.GetAllAsync(cancellationToken);
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            CandidateDto model = new();
            var electionResponse = await _electionService.GetActiveAsync();
            model.Election = electionResponse.Data!;
            model.ElectionId = electionResponse.Data!.Id;

            var stateResponse = await _electionService.GetAllStatesAsync(cancellationToken);
            if (stateResponse.Success)
            {
                model.States = stateResponse.Data.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
            }
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(10485760)]//10MB
        public async Task<IActionResult> Create(CandidateDto model, CancellationToken cancellationToken)
        {
            MessageResult<IEnumerable<StateDto>>? stateResponse = null;
            MessageResult<ElectionDto>? electionResponse = null;
            if (model == null)
            {
                model = new() { Errors = new List<string>() { "Bad Request" } };
                electionResponse = await _electionService.GetActiveAsync();
                model.Election = electionResponse.Data!;
                model.ElectionId = electionResponse.Data!.Id;

                stateResponse = await _electionService.GetAllStatesAsync(cancellationToken);
                if (stateResponse.Success)
                {
                    model.States = stateResponse.Data!.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                }

                return View(model);
            }
            var result = await _candidateService.AddAsync(model, cancellationToken);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Contestant enrolled successfully";
                return RedirectToAction("Index", "Contestant", new { area = "Admin" });
            }
            result.Data = new() { Errors = new() { result.Message } };
            electionResponse = await _electionService.GetActiveAsync();
            model.Election = electionResponse.Data;
            model.ElectionId = electionResponse.Data.Id;
            stateResponse = await _electionService.GetAllStatesAsync(cancellationToken);
            if (stateResponse.Success)
            {
                model.States = stateResponse.Data.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
            }
            model.Errors.Add(result.Message);
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            MessageResult<CandidateDto> response = await _candidateService.GetByIdAsync(id, cancellationToken);
            CandidateDto model = new();
            if (response.Success)
            {
                model = response.Data;

                var stateResponse = await _electionService.GetAllStatesAsync(cancellationToken);
                if (stateResponse.Success)
                {
                    model.States = stateResponse.Data.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                }
                var electionResponse = await _electionService.GetActiveAsync();
                model.Election = electionResponse.Data!;
                model.ElectionId = electionResponse.Data!.Id;

                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CandidateDto model, CancellationToken cancellationToken)
        {
            var result = await _candidateService.UpdateAsync(model, cancellationToken);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Contestant enrolled successfully";
                return RedirectToAction(nameof(Index));
            }
            var stateResponse = await _electionService.GetAllStatesAsync(cancellationToken);
            if (stateResponse.Success)
            {
                model.States = stateResponse.Data.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
            }
            result.Data = new() { Errors = new() { stateResponse.Message } };
            model.Errors.AddRange(result.Data.Errors);
            var electionResponse = await _electionService.GetActiveAsync();
            model.Election = electionResponse.Data!;
            model.ElectionId = electionResponse.Data!.Id;
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            MessageResult response = await _candidateService.DeleteAsync(id, cancellationToken);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Candidate deleted successfully";

            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the candidate";
            }
            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> ManageCategories(Guid id, string title, string firstname, string lastname, string state)
        {
            MessageResult<List<CandidateCategoryViewModel>> apiResponse = await _candidateService.FetchCandidateCategoriesAsync(id);
            CandidateCategoryListViewModel model = new();
            if (apiResponse.Success)
            {
                model = new()
                {
                    CandidateId = id,
                    Categories = apiResponse.Data.Select(x => new CheckboxListItem
                    {
                        IsChecked = x.IsChecked,
                        Name = x.Name,
                        Value = x.CategoryId
                    }).ToList(),
                    FirstName = firstname,
                    LastName = lastname,
                    Title = title,
                    State = state
                };
                return View(model);
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the candidate";
            }
            return RedirectToAction(nameof(Index));

        }


        [HttpPost]
        public async Task<IActionResult> ManageCategories(CandidateCategoryListViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("ManageCategories", new { id = model.CandidateId, title = model.Title, firstname = model.FirstName, lastname = model.LastName, state = model.State });
            }

            var updateModel = new CandidateCategoryListViewModel()
            {
                CandidateId = model.CandidateId,
                Categories = model.Categories.Select(x => new CheckboxListItem
                {
                    IsChecked = x.IsChecked,
                    Name = x.Name,
                    Value = x.Value
                }).ToList()
            };

            var result = await _candidateService.UpdateCandidateCategoriesAsync(updateModel, cancellationToken);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = "An error occurred while updating categories.";
                return RedirectToAction("ManageCategories", new { id = model.CandidateId, title = model.Title, firstname = model.FirstName, lastname = model.LastName, state = model.State });
            }
            else
            {
                TempData["SuccessMessage"] = "Categories updated successfully.";

                return RedirectToAction(nameof(Index));

            }
        }
    }
}
