namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class PolicyByEntityController : Controller
{
  private readonly IPolicyByEntityService _policyByEntityService;

  public PolicyByEntityController(IPolicyByEntityService policyByEntityService)
  {
    _policyByEntityService = policyByEntityService;
  }

  public async Task<IActionResult> Index()
  {
    ViewBag.EntityList = await _policyByEntityService.GetEntityListAsync();
    return View();
  }

  public async Task<IActionResult> GetPolicySettingsByEntity(EntityViewModel entity)
  {
    return Ok(await _policyByEntityService.GetPolicySettingsByEntityAsync(entity));
  }
}
