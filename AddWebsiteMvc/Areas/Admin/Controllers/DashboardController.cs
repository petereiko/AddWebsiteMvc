using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using AddWebsiteMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IAuthUser _authUser;
        private readonly ICandidateService _contestantService;
        private readonly IElectionService _electionService;

        public DashboardController(IAuthUser authUser, ICandidateService contestantService, IElectionService electionService)
        {
            _authUser = authUser;
            _contestantService = contestantService;
            _electionService = electionService;
        }

        public async Task<IActionResult> Index()
        {
            Task<GetAllCandidateResponse> getAllContestantResponseTask = _contestantService.GetAllCandidatesAsync();
            Task<GetElectionResponse> getElectionResponseTask = _electionService.GetActiveElectionAsync();

            await Task.WhenAll(getAllContestantResponseTask, getElectionResponseTask);

            DashboardViewModel model = new()
            {
                ContestantCount = getAllContestantResponseTask.Result.data.Count,
                Election = getElectionResponseTask.Result.data
            };

            return View(model);
        }

        
    }
}
