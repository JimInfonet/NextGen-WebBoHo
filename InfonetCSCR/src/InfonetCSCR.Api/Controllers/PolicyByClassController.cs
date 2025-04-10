namespace InfonetCSCR.Api.Controllers;

[Route("api/PolicyByClass")]
[ApiController]
[Authorize]
public class PolicyByClassController : ControllerBase
{
  private readonly IPolicyByClassService _policyByClassService;

  public PolicyByClassController(IPolicyByClassService policyByClassService)
  {
    _policyByClassService = policyByClassService;
  }

  // GET api/PolicyByClass/GetPolicyClasses
  [HttpGet]
  [Route("GetPolicyClasses")]
  public async Task<IActionResult> GetPolicyClasses()
  {
    var items = await _policyByClassService.GetPolicyClassesAsync();
    return Ok(items);
  }

  // GET api/PolicyByClass/GetPoliciesByClass
  [HttpGet]
  [Route("GetPoliciesByClass")]
  public async Task<IActionResult> GetPoliciesByClass(string className)
  {
    var items = await _policyByClassService.GetPoliciesByClassAsync(className);
    return Ok(items);
  }

  // GET api/PolicyByClass/GetPolicySettingsByName
  [HttpGet]
  [Route("GetPolicySettingsByName")]
  public async Task<IActionResult> GetPolicySettingsByName(string policyName)
  {
    var items = await _policyByClassService.GetPolicySettingsByNameAsync(policyName);
    return Ok(items);
  }

  // POST api/PolicyByClass/UpdateTimesheetPayPeriodOnLoad
  [HttpGet]
  [Route("UpdateTimesheetPayPeriodOnLoad")]
  public async Task<IActionResult> UpdateTimesheetPayPeriodOnLoad(long initialEmpID)
  {
    return Ok(await _policyByClassService.UpdateTimesheetPayPeriodOnLoadAsync(initialEmpID));
  }
}
