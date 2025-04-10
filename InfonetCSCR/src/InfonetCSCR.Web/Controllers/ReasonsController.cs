namespace InfonetCSCR.Web.Controllers;
[Authorize]
public class ReasonsController : Controller
{
  private readonly IReasonsService _reasonsService;

  public ReasonsController(IReasonsService reasonsService)
  {
    _reasonsService = reasonsService;
  }
  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetPolicies()
  {
    return Json(_reasonsService.GetPolicies());
  }

  [HttpGet]
  public async Task<IActionResult> GetReasons(string reasonType)
  {
    IList<ReasonViewModel>? reasons = await _reasonsService.GetReasons(reasonType);
    return Ok(reasons);
  }

  public async Task<IActionResult> ReasonAlreadyExists(string code, char type)
  {
    var alreadyExists = await _reasonsService.ReasonAlreadyExists(code, type);
    return Ok(alreadyExists);
  }

  public async Task<IActionResult> SaveReason([FromBody] ReasonViewModel reason, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _reasonsService.UpdateReason(reason) : _reasonsService.AddReason(reason));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteReason(string code, char type)
  {
    var isDeleted = await _reasonsService.DeleteReason(code, type);
    return Ok(isDeleted);
  }
}
