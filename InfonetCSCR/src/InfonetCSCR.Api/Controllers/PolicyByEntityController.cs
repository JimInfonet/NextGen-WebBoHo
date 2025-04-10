namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class PolicyByEntityController : ControllerBase
{
  private readonly IPolicyByEntityService _policyByEntityService;

  public PolicyByEntityController(IPolicyByEntityService policyByEntityService)
  {
    _policyByEntityService = policyByEntityService;
  }

  // GET: api/PolicyByEntity/GetEntityList
  [HttpGet]
  public async Task<IActionResult> GetEntityList()
  {
    return Ok(await _policyByEntityService.GetEntityListAsync());
  }

  // POST: api/PolicyByEntity/GetPolicySettingsByEntity
  [HttpPost]
  public async Task<IActionResult> GetPolicySettingsByEntity(Entity entity)
  {
    return Ok(await _policyByEntityService.GetPolicySettingsByEntityAsync(entity));
  }
}
