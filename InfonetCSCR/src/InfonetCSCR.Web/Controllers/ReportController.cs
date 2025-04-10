namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class ReportController : Controller
{

  public ActionResult ShowReport()
  {
    return PartialView("_Report");
  }
}
