namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class FuelPriceChangeController : Controller
{
  private readonly IPolicyService _policyService;
  private readonly IFuelPriceChangeService _fuelPriceChangeService;
  private readonly IHttpContextAccessor _contextAccessor;

  #region Private Properties

  private FuelPriceChangePolicyRecordViewModel FuelPriceChangePolicy
  {
    get
    {
      return new FuelPriceChangePolicyRecordViewModel
      {
        IsFuelGP = _policyService.GetPolicy(PolicyName.FUEL_GP),
        IsGrade = !_policyService.GetPolicy(PolicyName.FUEL_GP),
        IsFuelCP = _policyService.GetPolicy(PolicyName.FUEL_CP),
      };
    }
  }

  #endregion

  public FuelPriceChangeController(
    IPolicyService policyService, 
    IFuelPriceChangeService fuelPriceChangeService,
    IHttpContextAccessor contextAccessor)
  {
    _policyService = policyService;
    _fuelPriceChangeService = fuelPriceChangeService;
    _contextAccessor = contextAccessor;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetFuelPriceChangePolicies()
  {
    return Ok(FuelPriceChangePolicy);
  }

  public async Task<IActionResult> CheckStoreDataInHeadOfficeDatabase([FromBody] FuelPriceChangeStoreCheckRecordViewModel fuelPriceChangeStoreCheckRecordViewModel)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    StoreDataInHoDbRecordViewModel? storeDataInHoDbRecordViewModel = 
      await _fuelPriceChangeService.CheckStoreDataInHeadOfficeDatabase(fuelPriceChangeStoreCheckRecordViewModel);
    return Ok(storeDataInHoDbRecordViewModel);
  }

  public async Task<IActionResult> GetDataFromGradeTierAndServiceLevelTable([FromBody] GradeGridRecordViewModel gradeGridRecord)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    IList<GradeTierServiceLevelTableDataViewModel> gradeTierServiceLevelTableData = 
      await _fuelPriceChangeService.GetDataFromGradeTierAndServiceLevelTable(gradeGridRecord);
    return Ok(gradeTierServiceLevelTableData);
  }
  
  public async Task<IActionResult> TransferFuelPriceChange([FromBody] IList<FuelPriceChangeTransferRecordViewModel> storesData)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    bool isTransfered = await _fuelPriceChangeService.TransferFuelPriceChange(storesData);
    return Ok(isTransfered);
  }

  public IActionResult GetEmployeeId()
  {
    string user = _contextAccessor.HttpContext?.Session.GetString(SessionKeys.UserCode)!;
    return Json(user.ToLower());
  }

  [HttpGet]
  public ActionResult ShowReport()
  {
    return PartialView("_Report");
  }
}
