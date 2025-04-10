namespace InfonetCSCR.Api.Controllers;

[Route("api/CustomerGroup")]
[ApiController]
[Authorize]

public class CustomerGroupController : ControllerBase
{
  public readonly ICustomerGroupService _customerGroupService;

  public CustomerGroupController(ICustomerGroupService customerGroupService)
  {
    _customerGroupService = customerGroupService;
  }

  // GET api/CustomerGroup/GetAllCustomerGroupsByCashBonus
  [HttpGet]
  [Route("GetAllCustomerGroupsByCashBonus")]
  public async Task<IActionResult> GetAllCustomerGroupsByCashBonus(bool cashBonus)
  {
    IEnumerable<CustomerGroup> customerGroups = await _customerGroupService.GetAllCustomerGroupsByCashBonus(cashBonus);
    return Ok(customerGroups);
  }

  // POST api/CustomerGroup/AddCustomerGroup
  [HttpPost]
  [Route("AddCustomerGroup")]
  public async Task<IActionResult> AddCustomerGroup([FromBody] CustomerGroup customerGroup)
  {
    bool isCustomerGroupAdded = await _customerGroupService.AddCustomerGroup(customerGroup);
    return Ok(isCustomerGroupAdded);
  }

  // POST api/CustomerGroup/UpdateCustomerGroup
  [HttpPost]
  [Route("UpdateCustomerGroup")]
  public async Task<IActionResult> UpdateCustomerGroup([FromBody] CustomerGroup customerGroup)
  {
    bool isCustomerGroupUpdated = await _customerGroupService.UpdateCustomerGroup(customerGroup);
    return Ok(isCustomerGroupUpdated);
  }

  // POST api/CustomerGroup/DeleteCustomerGroup
  [HttpPost]
  [Route("DeleteCustomerGroup")]
  public async Task<IActionResult> DeleteCustomerGroup([FromBody] string groupID)
  {
    bool isCustomerGroupDeleted = await _customerGroupService.DeleteCustomerGroup(groupID);
    return Ok(isCustomerGroupDeleted);
  }

  // GET api/CustomerGroup/GetCustomerGroup
  [HttpGet]
  [Route("GetCustomerGroup")]
  public async Task<IActionResult> GetCustomerGroup(string? groupID)
  {
    if (string.IsNullOrEmpty(groupID))
      return Ok(null);

    var customerGroup = await _customerGroupService.GetCustomerGroup(groupID);
    return Ok(customerGroup);
  }

  // GET api/CustomerGroup/GetCashBonusData
  [HttpGet]
  [Route("GetCashBonusData")]
  public async Task<IActionResult> GetCashBonusData(string groupID)
  {
    IEnumerable<CashBonusChart> cashBonusData = await _customerGroupService.GetCashBonusData(groupID);
    return Ok(cashBonusData);
  }

  // POST api/CustomerGroup/UpdateCashBonusData
  [HttpPost]
  [Route("UpdateCashBonusData")]
  public async Task<IActionResult> UpdateCashBonusData([FromBody] IEnumerable<CashBonusChart> cashBonusCharts)
  {
    bool isUpdated = await _customerGroupService.UpdateCashBonusData(cashBonusCharts);
    return Ok(isUpdated);
  }

  // POST api/CustomerGroup/DeleteCashBonusData
  [HttpPost]
  [Route("DeleteCashBonusData")]
  public async Task<IActionResult> DeleteCashBonusData(string? groupID)
  {
    if (string.IsNullOrEmpty(groupID))
      return Ok(null);

    await _customerGroupService.DeleteCashBonusData(groupID);
    return Ok();
  }

  // GET api/CustomerGroup/GetDiscountChartData
  [HttpGet]
  [Route("GetDiscountChartData")]
  public async Task<IActionResult> GetDiscountChartData(string? groupID, bool isUseARInvoice)
  {
    var discountCharts = await _customerGroupService.GetDiscountChartData(groupID, isUseARInvoice);
    return Ok(discountCharts);
  }

  // POST api/CustomerGroup/DeleteDiscountChartData
  [HttpPost]
  [Route("DeleteDiscountChartData")]
  public async Task<IActionResult> DeleteDiscountChartData(string groupID)
  {
    bool isUpdated = await _customerGroupService.DeleteDiscountChart(groupID);
    return Ok(isUpdated);
  }

  // POST api/CustomerGroup/SaveDiscountChartData
  [HttpPost]
  [Route("SaveDiscountChartData")]
  public async Task<IActionResult> SaveDiscountChartData(IEnumerable<DiscountChart> discountCharts)
  {
    await _customerGroupService.SaveDiscountChartData(discountCharts);
    return Ok(null);
  }
}
