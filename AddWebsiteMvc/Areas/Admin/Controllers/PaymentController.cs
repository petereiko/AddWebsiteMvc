using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Interfaces;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PaymentController : Controller
    {
        private readonly IVoteService _contestantService;
        private readonly IEmailService _emailService;

        public PaymentController(IVoteService contestantService, IEmailService emailService)
        {
            _contestantService = contestantService;
            _emailService = emailService;
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
