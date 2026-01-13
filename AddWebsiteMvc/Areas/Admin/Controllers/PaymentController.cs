using AddWebsiteMvc.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PaymentController : Controller
    {
        private readonly ICandidateService _contestantService;

        public PaymentController(ICandidateService contestantService)
        {
            _contestantService = contestantService;
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(string reference)
        {
            var result = await _contestantService.ConfirmPayment(reference);
            if (result.statusCode == 200) 
            {
                TempData["SuccessMessage"] = "You vote has been counted successfully";
                return RedirectToAction("Index", "Candidates", new { area = "Gov" });
            }
            return View();
        }
    }
}
