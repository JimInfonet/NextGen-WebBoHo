namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
  public IActionResult Index()
  {
    return View();
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
    ViewData["errorMessage"] = exceptionHandlerPathFeature?.Error?.Message;
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}