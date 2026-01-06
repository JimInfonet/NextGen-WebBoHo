using Microsoft.AspNetCore.Mvc;

namespace Infonet.ReportRcl.Areas.Reports.Controllers
{
    public class HomeController : Controller
    {
        [Area("Reports")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
