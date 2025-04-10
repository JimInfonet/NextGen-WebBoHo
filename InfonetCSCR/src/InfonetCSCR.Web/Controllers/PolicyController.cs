namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class PolicyController : Controller
{
  private readonly IPolicyService _policyService;

  public PolicyController(IPolicyService policyService)
  {
    _policyService = policyService;
  }

  public async Task<IActionResult> GetAddEditPolicySettingModal()
  {
    return PartialView("_AddEditPolicySettingModal");
  }

  public async Task<IActionResult> CheckIfPolicySettingExists(PolicyCompanyViewModel policySetting)
  {
    return Ok(await _policyService.CheckIfPolicySettingExistsAsync(policySetting));
  }

  public async Task<IActionResult> AddPolicySetting(PolicyCompanyViewModel policySetting)
  {
    return Ok(await _policyService.AddPolicySettingAsync(policySetting));
  }

  public async Task<IActionResult> UpdatePolicySetting(PolicyCompanyViewModel policySetting)
  {
    return Ok(await _policyService.UpdatePolicySettingAsync(policySetting));
  }

  public async Task<IActionResult> GetSetVendPolicyLog(PolicyCompanyViewModel policySetting)
  {
    return Ok(await _policyService.GetSetVendPolicyLogAsync(policySetting));
  }

  public async Task<IActionResult> DeletePolicySetting(PolicyCompanyViewModel policySetting)
  {
    return Ok(await _policyService.DeletePolicySettingAsync(policySetting));
  }

  public async Task<IActionResult> GetSubDepartmentsList()
  {
    return Ok(await _policyService.GetSubDepartmentsListAsync());
  }

  public async Task<IActionResult> GetSubDetailsList()
  {
    return Ok(await _policyService.GetSubDetailsListAsync());
  }

  public async Task<IActionResult> GetPriceGroupsList()
  {
    return Ok(await _policyService.GetPriceGroupsListAsync());
  }

  public async Task<IActionResult> GetClCategoryList()
  {
    return Ok(await _policyService.GetClCategoryListAsync());
  }

  public async Task<IActionResult> GetDiscCodesList()
  {
    return Ok(await _policyService.GetDiscCodesListAsync());
  }

  public async Task<IActionResult> GetBranchesList()
  {
    return Ok(await _policyService.GetBranchesListAsync());
  }

  public async Task<IActionResult> GetUomList()
  {
    return Ok(await _policyService.GetUomListAsync());
  }
}
