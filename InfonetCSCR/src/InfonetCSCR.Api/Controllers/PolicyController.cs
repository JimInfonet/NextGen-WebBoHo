namespace InfonetCSCR.Api.Controllers;

[Route("api/Policy")]
[ApiController]
[Authorize]

public class PolicyController : ControllerBase
{
  private readonly IPolicyService _policyService;
  private readonly IPriceGroupService _priceGroupService;

  public PolicyController(IPolicyService policyService, IPriceGroupService priceGroupService)
  {
    _policyService = policyService;
    _priceGroupService = priceGroupService;
  }

  [HttpGet]
  [Route("PolicyAllCanbe")]
  public async Task<IActionResult> GetPolicyAllCanbe()
  {
    var data = await _policyService.GetPolicyAllCanbe();
    return Ok(data);
  }

  [HttpGet]
  [Route("PolicyAllSettings")]
  public async Task<IActionResult> GetPolicyAllSettings()
  {
    var data = await _policyService.GetPolicyAllSettings();
    return Ok(data);
  }

  [HttpGet]
  [Route("PolicyAllClass")]
  public async Task<IActionResult> GetPolicyAllClass()
  {
    var data = await _policyService.GetPolicyAllClass();
    return Ok(data);
  }

  [HttpGet]
  [Route("PolicyAllCompany")]
  public async Task<IActionResult> GetPolicyAllCompany()
  {
    var data = await _policyService.GetPolicyAllCompany();
    return Ok(data);
  }

  // POST api/Policy/CheckIfPolicySettingExists
  [HttpPost]
  [Route("CheckIfPolicySettingExists")]
  public async Task<IActionResult> CheckIfPolicySettingExists(PolicyCompany policySetting)
  {
    return Ok(await _policyService.CheckIfPolicySettingExistsAsync(policySetting));
  }

  // POST api/Policy/AddPolicySetting
  [HttpPost]
  [Route("AddPolicySetting")]
  public async Task<IActionResult> AddPolicySetting(PolicyCompany policySetting)
  {
    return Ok(await _policyService.AddPolicySettingAsync(policySetting));
  }

  // POST api/Policy/UpdatePolicySetting
  [HttpPost]
  [Route("UpdatePolicySetting")]
  public async Task<IActionResult> UpdatePolicySetting(PolicyCompany policySetting)
  {
    return Ok(await _policyService.UpdatePolicySettingAsync(policySetting));
  }

  // POST api/Policy/DeletePolicySetting
  [HttpPost]
  [Route("DeletePolicySetting")]
  public async Task<IActionResult> DeletePolicySetting(PolicyCompany policySetting)
  {
    return Ok(await _policyService.DeletePolicySettingAsync(policySetting));
  }

  // GET api/Policy/GetPriceGroupsList
  [HttpGet]
  [Route("GetPriceGroupsList")]
  public async Task<IActionResult> GetPriceGroupsList()
  {
    return Ok(await _priceGroupService.GetPriceGroupsListAsync());
  }
}
