using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Payment;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        [HttpGet]
        public IActionResult Do()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Do([FromBody] PaymentModel model, CancellationToken cancellationToken)
        {
            MessageResult result = await _contestantService.Verify(model.Reference, cancellationToken);
            return Json(result);
        }
    }
}
