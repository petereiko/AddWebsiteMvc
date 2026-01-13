using AddWebsiteMvc.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PaymentController : Controller
    {
        private readonly IVoteService _contestantService;

        public PaymentController(IVoteService contestantService)
        {
            _contestantService = contestantService;
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(string reference, CancellationToken cancellationToken)
        {
            var result = await _contestantService.Verify(reference, cancellationToken);
            if (result.Success) 
            {
                TempData["SuccessMessage"] = "You vote has been counted successfully";
                return RedirectToAction("Index", "Candidates", new { area = "Gov" });
            }
            return View();
        }
    }
}
