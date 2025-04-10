namespace InfonetCSCR.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]

public class DataTransferReportsController : ControllerBase
{
  private readonly IDataTransferReportsService _dataTransferReportsService;

  public DataTransferReportsController(IDataTransferReportsService dataTransferReportsService)
  {
    _dataTransferReportsService = dataTransferReportsService;
  }

  // Get api/DataTransferReports/GetPriceChangeReportData
  [HttpGet]
  [Route("GetPriceChangeReportData")]
  public async Task<IActionResult> GetPriceChangeReportData()
  {
    var data = await _dataTransferReportsService.GetPriceChangeReportData();
    return Ok(data);
  }

  // Get api/DataTransferReports/GetHotButtonsReportData
  [HttpGet]
  [Route("GetHotButtonsReportData")]
  public async Task<IActionResult> GetHotButtonsReportData()
  {
    var data = await _dataTransferReportsService.GetHotButtonsReportData();
    return Ok(data);
  }

  // Get api/DataTransferReports/GetCostchangeReportData
  [HttpGet]
  [Route("GetCostchangeReportData")]
  public async Task<IActionResult> GetCostchangeReportData()
  {
    var data = await _dataTransferReportsService.GetCostchangeReportData();
    return Ok(data);
  }

  // Get api/DataTransferReports/GetStockTransferReportData
  [HttpGet]
  [Route("GetStockTransferReportData")]
  public async Task<IActionResult> GetStockTransferReportData()
  {
    var data = await _dataTransferReportsService.GetStockTransferReportData();
    return Ok(data);
  }

  // Get api/DataTransferReports/GetCommodityTransferReportData
  [HttpGet]
  [Route("GetCommodityTransferReportData")]
  public async Task<IActionResult> GetCommodityTransferReportData()
  {
    var data = await _dataTransferReportsService.GetCommodityTransferReportData();
    return Ok(data);
  }

  // Get api/DataTransferReports/GetPromotionalDiscountTransferReportData
  [HttpGet]
  [Route("GetPromotionalDiscountTransferReportData")]
  public async Task<IActionResult> GetPromotionalDiscountTransferReportData()
  {
    var data = await _dataTransferReportsService.GetPromotionalDiscountTransferReportData();
    return Ok(data);
  }
}
