namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class ExcelReportsController : ControllerBase
{
  private readonly IExcelReportsService _excelReportsService;

  public ExcelReportsController(IExcelReportsService excelReportsService)
  {
    _excelReportsService = excelReportsService;
  }

  // POST api/ExcelReports/CheckSaleDataExists
  [HttpPost]
  [Route("CheckSaleDataExists")]
  public async Task<IActionResult> CheckSaleDataExists([FromBody] ExcelReportGenerator excelReportGenerator)
  {
    bool isSaleDataExists = await _excelReportsService.CheckSaleDataExists(excelReportGenerator);
    return Ok(isSaleDataExists);
  }

  // POST api/ExcelReports/GetFuelDailySalesReportData
  [HttpPost]
  [Route("GetFuelDailySalesReportData")]
  public async Task<IActionResult> GetFuelDailySalesReportData(ExcelReportGenerator excelReportGenerator)
  {
    DateTime shiftDate = excelReportGenerator.SelectedDate ?? DateTime.Now;
    ExcelReportDataDto? excelReportDataDto = await _excelReportsService.GetFuelDailySalesReportData(shiftDate);
    return Ok(excelReportDataDto);
  }

  // GET api/ExcelReports/GetFuelLitresOrDollarsSummaryReportData
  [HttpGet]
  [Route("GetFuelLitresOrDollarsSummaryReportData")]
  public async Task<IActionResult> GetFuelLitresOrDollarsSummaryReportData()
  {
    FuelLitresSummaryReportDto? saleData = await _excelReportsService.GetFuelLitresOrDollarsSummaryReportData();
    return Ok(saleData);
  }

  // GET api/ExcelReports/GetUniqueStockCodes
  [HttpGet]
  [Route("GetUniqueStockCodes")]
  public async Task<IActionResult> GetUniqueStockCodes()
  {
    IList<string>? stockCodes = await _excelReportsService.GetUniqueStockCodes();
    return Ok(stockCodes);
  }

  // GET api/ExcelReports/GetAllStockMaster
  [HttpGet]
  [Route("GetAllStockMaster")]
  public async Task<IActionResult> GetAllStockMaster()
  {
    IList<StockItem>? stockMasters = await _excelReportsService.GetAllStockMaster();
    return Ok(stockMasters);
  }

  // GET api/ExcelReports/GetGradesByFuelType
  [HttpGet]
  [Route("GetGradesByFuelType")]
  public async Task<IActionResult> GetGradesByFuelType()
  {
    IList<Grade>? grades = await _excelReportsService.GetGradesByFuelType();
    return Ok(grades);
  }

  // POST api/ExcelReports/GetDistinctDatesFromSaleHead
  [HttpPost]
  [Route("GetDistinctDatesFromSaleHead")]
  public async Task<IActionResult> GetDistinctDatesFromSaleHead(ExcelReportGenerator excelReportGenerator)
  {
    IList<SaleHeadDto>? dates = await _excelReportsService.GetDistinctDatesFromSaleHead(excelReportGenerator);
    return Ok(dates);
  }

  // POST api/ExcelReports/GetSummaryFuelProductsReportData
  [HttpPost]
  [Route("GetSummaryFuelProductsReportData")]
  public async Task<IActionResult> GetSummaryFuelProductsReportData(ExcelReportGenerator excelReportGenerator)
  {
    IList<SummaryFuelProductsReportDto>? data = await _excelReportsService.GetSummaryFuelProductsReportData(excelReportGenerator);
    return Ok(data);
  }

  // POST api/ExcelReports/GetNorth60OrYTGFleetCardReportData
  [HttpPost]
  [Route("GetNorth60OrYTGFleetCardReportData")]
  public async Task<IActionResult> GetNorth60OrYTGFleetCardReportData(ExcelReportGenerator excelReportGenerator, bool isYTG)
  {
    IList<FleetCardReportDto>? data = await _excelReportsService.GetNorth60OrYTGFleetCardReportData(excelReportGenerator, isYTG);
    return Ok(data);
  }

  // GET api/ExcelReports/GetAmountFromSaleLine
  [HttpGet]
  [Route("GetAmountFromSaleLine")]
  public async Task<IActionResult> GetAmountFromSaleLine()
  {
    IList<SaleLine>? amounts = await _excelReportsService.GetAmountFromSaleLine();
    return Ok(amounts);
  }

  // POST api/ExcelReports/GetSalesByHourReport
  [HttpPost]
  [Route("GetSalesByHourReport")]
  public async Task<IActionResult> GetSalesByHourReport(ExcelReportGenerator excelReportGenerator)
  {
    IList<SaleHead>? data = await _excelReportsService.GetSalesByHourReport(excelReportGenerator);
    return Ok(data);
  }

  // GET api/ExcelReports/GetAllReportData
  [HttpGet]
  [Route("GetAllReportData")]
  public async Task<IActionResult> GetAllReportData()
  {
    IList<ReportData>? reportData = await _excelReportsService.GetAllReportData();
    return Ok(reportData);
  }
}
