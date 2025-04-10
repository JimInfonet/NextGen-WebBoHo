namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class RestrictionsController : Controller
{
  private readonly IRestrictionsService _restrictionsService;

  public RestrictionsController(IRestrictionsService restrictionsService)
  {
    _restrictionsService = restrictionsService;
  }

  public IActionResult Index()
  {
    return View();
  }

  [HttpGet]
  public async Task<IActionResult> GetRestrictions()
  {
    IList<RestrictionsViewModel>? restrictions = await _restrictionsService.GetRestrictions();
    return Ok(restrictions);
  }

  public async Task<IActionResult> RestrictionAlreadyExists(int code)
  {
    var alreadyExists = await _restrictionsService.RestrictionAlreadyExists(code);
    return Ok(alreadyExists);
  }

  public async Task<IActionResult> SaveRestriction([FromBody] RestrictionsViewModel restriction, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _restrictionsService.UpdateRestriction(restriction) : _restrictionsService.AddRestriction(restriction));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteRestriction(int code)
  {
    var isDeleted = await _restrictionsService.DeleteRestriction(code);
    return Ok(isDeleted);
  }
}
