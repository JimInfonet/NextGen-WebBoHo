namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CustomerGroupController : Controller
{
  private const string _custMem = "CUST_MEM";
  private const string _cashBonus = "CASHBONUS";
  private const string _isUpdate = "isUpdate";
  private const string _isCustomerGroupAlreadyExists = "IsCustomerGroupAlreadyExists";
  private const string _copy = "copy";
  private const string _create = "create";
  private const string _update = "update";
  private const string _view = "view";
  private readonly IHttpService _http;
  private readonly ICustomerGroupService _customerGroupService;

  private static bool _isUpdateCustomerGroup = false;
  private static bool _isCopyCustomerGroup = false;

  public CustomerGroupController(IHttpService http, ICustomerGroupService customerGroupService)
  {
    _http = http;
    _customerGroupService = customerGroupService;
  }

  public IActionResult Index(string? actionMode, bool? showToaster)
  {
    var policies = _customerGroupService.GetPolicies();
    ViewData[_custMem] = policies.CustMem;
    return View();
  }

  public async Task<IActionResult> GetAllCustomerGroups()
  {
    var policies = _customerGroupService.GetPolicies();
    string url = $"CustomerGroup/GetAllCustomerGroupsByCashBonus?cashBonus={policies.CashBonus}";
    var customerGroups = await _http.GetAndParseAsync<IEnumerable<CustomerGroupViewModel>>(url);
    return Ok(customerGroups);
  }

  public async Task<IActionResult> CustomerGroupForm(string? groupID, string? actionMode, string? operationType,
    bool? isCopyCustomerGroup = false, bool? showToaster = false)
  {
    var policies = _customerGroupService.GetPolicies();
    ViewData[_custMem] = policies.CustMem;
    ViewData[_cashBonus] = policies.CashBonus;

    _isUpdateCustomerGroup = !string.IsNullOrEmpty(groupID) && (bool)!isCopyCustomerGroup;
    _isCopyCustomerGroup = (bool)isCopyCustomerGroup;

    ViewData[_isUpdate] = _isUpdateCustomerGroup;

    if (_isUpdateCustomerGroup || (bool)isCopyCustomerGroup)
    {
      var customerGroup = await _customerGroupService.GetCustomerGroup(groupID);

      return View(customerGroup);
    }

    return View();
  }

  public async Task<IActionResult> SaveCustomerGroup(CustomerGroupViewModel customerGroupViewModel)
  {
    var policies = _customerGroupService.GetPolicies();
    ViewData[_custMem] = policies.CustMem;
    ViewData[_cashBonus] = policies.CashBonus;
    ViewData[_isUpdate] = _isUpdateCustomerGroup;

    if (!_isUpdateCustomerGroup)
    {
      var customerGroup = await _customerGroupService.GetCustomerGroup(customerGroupViewModel.GroupID);
      if (customerGroup != null && customerGroup.GroupID == customerGroupViewModel.GroupID)
      {
        ViewData[_isCustomerGroupAlreadyExists] = true;
        return View("CustomerGroupForm", customerGroupViewModel);
      }
    }

    string createOrCopyOperation = _isCopyCustomerGroup ? _copy : _create;

    await _customerGroupService.SaveCustomerGroup(customerGroupViewModel, _isUpdateCustomerGroup);
    return RedirectToAction("CustomerGroupForm", new
    {
      groupID = customerGroupViewModel.GroupID,
      actionMode = _view,
      operationType = _isUpdateCustomerGroup ? _update : createOrCopyOperation,
      showToaster = true
    });
  }

  public async Task<IActionResult> DeleteCustomerGroup([FromBody] string groupID)
  {
    await _customerGroupService.DeleteCustomerGroup(groupID);
    return NoContent();
  }

  public async Task<IActionResult> GetCashBonusData(string groupID)
  {
    string url = $"CustomerGroup/GetCashBonusData?groupID={groupID}";
    var cashBonusData = await _http.GetAndParseAsync<IEnumerable<CashBonusChartViewModel>>(url);
    return Ok(cashBonusData);
  }

  [HttpPost]
  public async Task<IActionResult> UpdateCashBonusData([FromBody] IEnumerable<CashBonusChartViewModel> cashBonusChartViewModels)
  {
    await _customerGroupService.UpdateCashBonusData(cashBonusChartViewModels);
    return NoContent();
  }

  public async Task<IActionResult> DeleteCashBonusData(string groupID)
  {
    await _customerGroupService.DeleteCashBonusData(groupID);

    return NoContent();
  }

  public async Task<IActionResult> GetDiscountChartData(string? groupID)
  {
    var policies = _customerGroupService.GetPolicies();
    var discountChartData = await _customerGroupService.GetDiscountChartData(groupID, policies.UseARInvoices);
    return Ok(discountChartData);
  }

  public async Task<IActionResult> DeleteDiscountChartData(string groupID)
  {
    await _customerGroupService.DeleteDiscountChartData(groupID);

    return NoContent();
  }

  public async Task<IActionResult> SaveDiscountChartData([FromBody] IEnumerable<DiscountChartViewModel> discountChartViewModels)
  {
    await _customerGroupService.SaveDiscountChartData(discountChartViewModels);

    return NoContent();
  }
}
