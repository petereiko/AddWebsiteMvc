using Microsoft.AspNetCore.Mvc;

namespace AddWebsiteMvc.Areas.Admin.Controllers
{
    public class TransactionController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Transactions()
        {

            return View();
        }
    }
}
