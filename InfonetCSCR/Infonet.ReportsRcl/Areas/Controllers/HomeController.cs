using Microsoft.AspNetCore.Mvc;

namespace Infonet.ReportsRcl.Areas.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
