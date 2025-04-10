namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class PolicyByClassController : Controller
{
  private readonly IPolicyByClassService _policyByClassService;

  public PolicyByClassController(IPolicyByClassService policyByClassService)
  {
    _policyByClassService = policyByClassService;
  }

  public async Task<IActionResult> Index()
  {
    ViewBag.PoliciesClassList = await _policyByClassService.GetPolicyClassesAsync();
    return View();
  }

  public async Task<IActionResult> GetPoliciesByClass(string className)
  {
    var policies = await _policyByClassService.GetPoliciesByClass(className);
    return Ok(policies);
  }

  public async Task<IActionResult> GetPolicySettingsByName(string policyName)
  {
    var settings = await _policyByClassService.GetPolicySettingsByNameAsync(policyName);
    return Ok(settings);
  }

  public async Task<IActionResult> UpdateTimesheetPayPeriodOnLoad()
  {
    return Ok(await _policyByClassService.UpdateTimesheetPayPeriodOnLoadAsync());
  }
}
