using InfonetCSCR.Core.Models;
using Newtonsoft.Json;

namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly ILogger<ReportController> _logger;
    private readonly IApplicationConfig _config;
    private readonly IPolicyService _policy;
    private readonly DateTime today;
    static readonly bool _isUpdateCustomer = false;

    public ReportController(ILogger<ReportController> logger, IApplicationConfig config, IPolicyService policy)
    {
        _logger = logger;
        _config = config;
        _policy = policy;
        today = DateTime.Today;
    }
    public IActionResult Dashboard()
    {
        ViewBag.IsHeadOffice = !_config.IsBackOffice;

        ViewBag.ReportUrl = _config.DashboardBaseUrl;

        return View(); // Do NOT redirect
        //return Redirect(WebBaseUrl);
    }
    public IActionResult Category()
    {
        ViewBag.IsHeadOffice = !_config.IsBackOffice;

        ViewBag.ReportUrl = _config.CategoryBaseUrl;

        return View(); // Do NOT redirect
        //return Redirect(WebBaseUrl);
    }
    public IActionResult Reports()
    {
        ViewBag.IsHeadOffice = !_config.IsBackOffice;

        ViewBag.ReportUrl = _config.ReportBaseUrl;

        return View(); // Do NOT redirect
        //return Redirect(WebBaseUrl);
    }

    public ActionResult ShowReport()
    {
        return PartialView("_Report");
    }

}
