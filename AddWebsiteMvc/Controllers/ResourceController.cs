using Microsoft.AspNetCore.Mvc;

namespace AddWebsiteMvc.Controllers
{
    public class ResourceController : Controller
    {
        public IActionResult FaQ()
        {
            return View();
        }

        public IActionResult Careers()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult AboutUs() 
        {
            return View();
        }
    }
}
