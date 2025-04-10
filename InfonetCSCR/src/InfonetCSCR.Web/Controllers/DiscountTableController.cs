namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class DiscountTableController : Controller
{
  private readonly IPolicyService _policyService;
  private readonly IDiscountTableService _discountTableService;

  #region Private Properties

  private DiscountTablePolicyRecordViewModel DiscountTablePolicies
  {
    get
    {
      return new DiscountTablePolicyRecordViewModel
      {
        IsProductDiscount = _policyService.GetPolicy(PolicyName.PROD_DISC),
        IsCustomerDiscount = _policyService.GetPolicy(PolicyName.CUST_DISC),
      };
    }
  }

  #endregion

  public DiscountTableController(
    IPolicyService policyService,
    IDiscountTableService discountTableService)
  {
    _policyService = policyService;
    _discountTableService = discountTableService;
  }

  public IActionResult Index()
  {
    ViewData["IsProductDiscount"] = DiscountTablePolicies.IsProductDiscount;
    ViewData["IsCustomerDiscount"] = DiscountTablePolicies.IsCustomerDiscount;

    return View();
  }

  public IActionResult GetDiscountTablePolicies()
  {
    return Ok(DiscountTablePolicies);
  }

  public async Task<IActionResult> GetAllDiscountTableData(DiscountTablePolicyRecordViewModel discountTablePolicyRecordViewModel)
  {
    IList<DiscountTableViewModel>? discountTableData = await _discountTableService.GetAllDiscountTableData(discountTablePolicyRecordViewModel);
    return Ok(discountTableData);
  }

  public async Task<IActionResult> InsertDataOnDiscountTable([FromBody] DiscountTableViewModel discountTableViewModel)
  {
    bool isAdded = await _discountTableService.InsertDataOnDiscountTable(discountTableViewModel);
    return Ok(isAdded);
  }

  public async Task<IActionResult> UpdateDiscountPercentage([FromBody] DiscountTableViewModel discountTableViewModel)
  {
    bool isUpdated = await _discountTableService.UpdateDiscountPercentage(discountTableViewModel);
    return Ok(isUpdated);
  }

  public async Task<IActionResult> DeleteDiscount([FromBody] DiscountTableViewModel discountTableViewModel)
  {
    bool isDeleted = await _discountTableService.DeleteDiscount(discountTableViewModel);
    return Ok(isDeleted);
  }
}
