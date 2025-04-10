namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FuelPriceChangeController : ControllerBase
{
  private readonly IFuelPriceChangeService _fuelPriceChangeService;

  public FuelPriceChangeController(IFuelPriceChangeService fuelPriceChangeService)
  {
    _fuelPriceChangeService = fuelPriceChangeService;
  }

  // POST api/FuelPriceChange/CheckStoreDataInHeadOfficeDatabase
  [HttpPost]
  [Route("CheckStoreDataInHeadOfficeDatabase")]
  public async Task<IActionResult> CheckStoreDataInHeadOfficeDatabase(FuelPriceChangeStoreCheckRecord fuelPriceChangeStoreCheckRecord)
  {
    StoreDataInHoDbRecord storeDataInHoDbRecord = await _fuelPriceChangeService.CheckStoreDataInHeadOfficeDatabase(fuelPriceChangeStoreCheckRecord);
    return Ok(storeDataInHoDbRecord);
  }

  // POST api/FuelPriceChange/GetDataFromGradeTierAndServiceLevelTable
  [HttpPost]
  [Route("GetDataFromGradeTierAndServiceLevelTable")]
  public async Task<IActionResult> GetDataFromGradeTierAndServiceLevelTable(GradeGridRecord gradeGridRecord)
  {
    IList<GradeTierServiceLevelTableData>? gradeTierServiceLevelTableData = await _fuelPriceChangeService.GetDataFromGradeTierServiceLevelTable(gradeGridRecord);
    return Ok(gradeTierServiceLevelTableData);
  }

  // POST api/FuelPriceChange/TransferFuelPriceChange
  [HttpPost]
  [Route("TransferFuelPriceChange")]
  public async Task<IActionResult> TransferFuelPriceChange(IList<FuelPriceChangeTransferRecord> storesData)
  {
    bool isTransfered = await _fuelPriceChangeService.TransferFuelPriceChange(storesData);
    return Ok(isTransfered);
  }

  // GET api/FuelPriceChange/GetFuelPriceChangeReportData
  [HttpGet]
  [Route("GetFuelPriceChangeReportData")]
  public async Task<IActionResult> GetFuelPriceChangeReportData(string storeIds, bool isFuelCp)
  {
    IList<string> allStoreIds = storeIds.Split(',')
                        .Select(item => item.Trim())
                        .ToList();
    IList<GradeTierServiceLevelTableData>? fuelPriceChangeReportData = await _fuelPriceChangeService.GetFuelPriceChangeReportData(allStoreIds, isFuelCp);
    return Ok(fuelPriceChangeReportData);
  }
}
