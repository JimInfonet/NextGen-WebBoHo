namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class StockTakesAndReceivingController : ControllerBase
{
  private readonly IStockTakesAndReceivingService _stockTakesAndReceivingService;

  public StockTakesAndReceivingController(
    IStockTakesAndReceivingService stockTakesAndReceivingService)
  {
    _stockTakesAndReceivingService = stockTakesAndReceivingService;
  }

  // POST api/StockTakesAndReceiving/GetStocks
  [HttpPost]
  [Route("GetStocks")]
  public async Task<IActionResult> GetStocks([FromBody] StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    IEnumerable<StockDto>? stocks = await _stockTakesAndReceivingService.GetStocks(stockTakesAndRecievingDto);
    return Ok(stocks);
  }

  // POST api/StockTakesAndReceiving/GetStockTakesAndReceiveData
  [HttpPost]
  [Route("GetStockTakesAndReceiveData")]
  public async Task<IActionResult> GetStockTakesAndReceiveData([FromBody] StockTakesAndReceiveRequestDto stockTakesAndReceiveRequest)
  {
    StockTakesAndReceivingResult stockTakesAndReceivingResult = await _stockTakesAndReceivingService.GetStockTakesAndReceiveData(stockTakesAndReceiveRequest);
    return Ok(stockTakesAndReceivingResult);
  }

  // POST api/StockTakesAndReceiving/CheckStockCodeExistsInStockTakesAndReceiveData
  [HttpPost]
  [Route("CheckStockCodeExistsInStockTakesAndReceiveData")]
  public async Task<IActionResult> CheckStockCodeExistsInStockTakesAndReceiveData([FromBody] StockTakesAndReceiveRequestDto stockTakesAndReceiveRequest)
  {
    StockTakesAndReceivingResult stockTakesAndReceivingResult = await _stockTakesAndReceivingService.GetStockTakesAndReceiveData(stockTakesAndReceiveRequest);
    return Ok(stockTakesAndReceivingResult);
  }

  // POST api/StockTakesAndReceiving/AddRecLine
  [HttpPost]
  [Route("AddRecLine")]
  public async Task<IActionResult> AddRecLine([FromBody] ReclineDto reclineDto)
  {
    bool isAdded = await _stockTakesAndReceivingService.AddRecline(reclineDto);
    return Ok(isAdded);
  }

  // Post api/StockTakesAndReceiving/DeleteRecLine
  [HttpPost]
  [Route("DeleteRecLine")]
  public async Task<IActionResult> DeleteRecLine([FromBody] StockTakesAndReceiveDto receive)
  {
    bool isDeleted = await _stockTakesAndReceivingService.DeleteRecLine(receive);
    return Ok(isDeleted);
  }

  // Post api/StockTakesAndReceiving/DeleteAllRecLineAndRecHead
  [HttpPost]
  [Route("DeleteAllRecLineAndRecHead")]
  public async Task<IActionResult> DeleteAllRecLineAndRecHead(int purpose)
  {
    bool isDeleted = await _stockTakesAndReceivingService.DeleteAllRecLineAndRecHead(purpose);
    return Ok(isDeleted);
  }

  // GET api/StockTakesAndReceiving/GetCost
  [HttpGet]
  [Route("GetCost")]
  public async Task<IActionResult> GetCost(string stockCode)
  {
    decimal cost = await _stockTakesAndReceivingService.GetCost(stockCode);
    return Ok(cost);
  }

  // POST api/StockTakesAndReceiving/AddAllStocksToRecLine
  [HttpPost]
  [Route("AddAllStocksToRecLine")]
  public async Task<IActionResult> AddAllStocksToRecLine(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    bool isAdded = await _stockTakesAndReceivingService.AddAllStocksToRecline(stockTakesAndRecievingDto);
    return Ok(isAdded);
  }

  // Get api/StockTakesAndReceiving/Suspend
  [HttpGet]
  [Route("Suspend")]
  public async Task<IActionResult> Suspend(int purpose)
  {
    bool isSuspended = await _stockTakesAndReceivingService.Suspend(purpose);
    return Ok(isSuspended);
  }

  // Get api/StockTakesAndReceiving/Unsuspend
  [HttpGet]
  [Route("Unsuspend")]
  public async Task<IActionResult> Unsuspend(int purpose)
  {
    bool isUnsuspended = await _stockTakesAndReceivingService.Unsuspend(purpose);
    return Ok(isUnsuspended);
  }

  // Get api/StockTakesAndReceiving/GetRecLineSusCount
  [HttpGet]
  [Route("GetRecLineSusCount")]
  public async Task<IActionResult> GetRecLineSusCount(int purpose)
  {
    int count = await _stockTakesAndReceivingService.GetRecLineSusCount(purpose);
    return Ok(count);
  }

  // Get api/StockTakesAndReceiving/GetStockTakesAndReceivingReportData
  [HttpGet]
  [Route("GetStockTakesAndReceivingReportData")]
  public async Task<IActionResult> GetStockTakesAndReceivingReportData(int purpose)
  {
    IEnumerable<StockTakesAndReceivingReportDto> reportData = await _stockTakesAndReceivingService.GetStockTakesAndReceivingReportData(purpose);
    return Ok(reportData);
  }

  // POST api/StockTakesAndReceiving/Process
  [HttpPost]
  [Route("Process")]
  public async Task<IActionResult> Process([FromBody] StockProcessParametersDto stockProcessParameters)
  {
    bool isProcessed = await _stockTakesAndReceivingService.Process(stockProcessParameters);
    return Ok(isProcessed);
  }

  // Get api/StockTakesAndReceiving/GetStockReceivingExportData
  [HttpGet]
  [Route("GetStockReceivingExportData")]
  public async Task<IActionResult> GetStockReceivingExportData()
  {
    IEnumerable<StockReceivingExportDto>? stockReceivingExportData = await _stockTakesAndReceivingService.GetStockReceivingExportData();
    return Ok(stockReceivingExportData);
  }

  // Get api/StockTakesAndReceiving/GetStockTakesExportData
  [HttpGet]
  [Route("GetStockTakesExportData")]
  public async Task<IActionResult> GetStockTakesExportData()
  {
    StockTakesExportDto stockTakesExportData = await _stockTakesAndReceivingService.GetStockTakesExportData();
    return Ok(stockTakesExportData);
  }

  // Post api/StockTakesAndReceiving/ImportFromFile
  [HttpPost]
  [Route("ImportFromFile")]
  public async Task<IActionResult> ImportFromFile([FromBody] string text, string exceptedTax, int costDec, int purpose)
  {
    List<string> badLines = await _stockTakesAndReceivingService.ImportFromFile(text, exceptedTax, costDec, purpose);
    return Ok(badLines);
  }

  // Get api/StockTakesAndReceiving/GetCountOfInventoryDataForMonth
  [HttpGet]
  [Route("GetCountOfInventoryDataForMonth")]
  public async Task<IActionResult> GetCountOfInventoryDataForMonth(int month)
  {
    bool result = await _stockTakesAndReceivingService.GetCountOfInventoryDataForMonth(month);
    return Ok(result);
  }

  // Get api/StockTakesAndReceiving/UpdateQuantityForStockTakes
  [HttpGet]
  [Route("UpdateQuantityForStockTakes")]
  public async Task<IActionResult> UpdateQuantityForStockTakes(string stockCode, decimal quantity, string costType)
  {
    bool result = await _stockTakesAndReceivingService.UpdateQuantityForStockTakes(stockCode, quantity, costType);
    return Ok(result);
  }

  // Get api/StockTakesAndReceiving/UpdateTotal
  [HttpGet]
  [Route("UpdateTotal")]
  public async Task<IActionResult> UpdateTotal([FromQuery] ReclineDto recLine)
  {
    bool result = await _stockTakesAndReceivingService.UpdateTotal(recLine);
    return Ok(result);
  }

  // Get api/StockTakesAndReceiving/CalculateAllSales
  [HttpGet]
  [Route("CalculateAllSales")]
  public async Task<IActionResult> CalculateAllSales(int purpose, bool isUpdateInitial)
  {
    bool result = await _stockTakesAndReceivingService.CalculateAllSales(purpose, isUpdateInitial);
    return Ok(result);
  }

  // Get api/StockTakesAndReceiving/GetNotPrintedRecLinesCount
  [HttpGet]
  [Route("GetNotPrintedRecLinesCount")]
  public async Task<IActionResult> GetNotPrintedRecLinesCount(int purpose)
  {
    int result = await _stockTakesAndReceivingService.GetNotPrintedRecLinesCount(purpose);
    return Ok(result);
  }

  // Get api/StockTakesAndReceiving/UpdateInvoiceNumber
  [HttpGet]
  [Route("UpdateInvoiceNumber")]
  public async Task<IActionResult> UpdateInvoiceNumber([FromQuery] ReclineDto reclineDto)
  {
    bool result = await _stockTakesAndReceivingService.UpdateInvoiceNumber(reclineDto);
    return Ok(result);
  }
}
