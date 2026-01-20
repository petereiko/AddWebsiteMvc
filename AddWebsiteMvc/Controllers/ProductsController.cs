using Microsoft.AspNetCore.Mvc;

namespace AddWebsiteMvc.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Promotions()
        {
            return View();
        }
    }
}
