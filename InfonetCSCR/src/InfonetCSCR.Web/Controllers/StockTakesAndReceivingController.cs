namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class StockTakesAndReceivingController : Controller
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;

  public StockTakesAndReceivingController(
    IHttpService http,
    IApplicationConfig config,
    IPolicyService policyService)
  {
    _http = http;
    _config = config;
    _policyService = policyService;
  }

  public IActionResult Index(string stockCountType)
  {
    if (!_config.IsBackOffice) return NotFound();
    ViewData["stockCountType"] = stockCountType;
    ViewData["UserCode"] = HttpContext.Session.GetString("UserCode");
    return View();
  }

  public IActionResult GetPolicies()
  {
    var policies = new StockTakesAndReceivingPoliciesViewModel
    {
      AutoInv = _policyService.GetPolicy(PolicyName.AUTO_INV),
      SetVendor = _policyService.GetPolicy(PolicyName.SET_VEND),
      HndheldFiles = _policyService.GetPolicy(PolicyName.HNDHELD_FILES),
      ExceptedTax = _policyService.GetPolicy(PolicyName.EXPT_TAX),
      CostDec = _policyService.GetPolicy(PolicyName.COST_DEC),
      DefMargin = _policyService.GetPolicy(PolicyName.DEF_PR_CHG),
      ConfirmPc = _policyService.GetPolicy(PolicyName.CONFIRM_PC),
      UpdateNetwork = _policyService.GetPolicy(PolicyName.UPD_NETW),
      IsMarginCalcEqualsCostMkup = _policyService.GetPolicy(PolicyName.MARGINCALC).ToString().ToUpper() == "CostMkUp".ToUpper(),
      NumPrice = _policyService.GetPolicy(PolicyName.NUM_PRICE),
      CostType = _policyService.GetPolicy(PolicyName.COST_TYPE)
    };

    return Ok(policies);
  }

  public async Task<IActionResult> GetStocks([FromBody] StockTakesAndRecievingViewModel model)
  {
    bool sellInactivePolicy = _policyService.GetPolicy(PolicyName.SELL_INACT);
    model.SellInactivePolicy = (model.Purpose != (int)StockCountType.StockReceiving) && sellInactivePolicy;

    string url = "StockTakesAndReceiving/GetStocks";
    var stocks = await _http.PostAndParseAsync<IEnumerable<StockViewModel>>(url, model);
    return Ok(stocks);
  }

  public async Task<IActionResult> GetStockTakesAndReceiveData([FromBody] JqueryDatatableParam parameters, int purpose)
  {
    ReceiveDataRequestViewModel model = new()
    {
      Purpose = purpose,
      Length = parameters.Length,
      Start = parameters.Start,
      SearchValue = parameters.SearchValue,
      SearchColumnName = parameters.SearchColumnName
    };

    string url = @$"StockTakesAndReceiving/GetStockTakesAndReceiveData";
    var response = await _http.PostAndParseAsync<StocktakesAndReceivingResultViewModel>(url, model);

    return Ok(new
    {
      iTotalRecords = response.totalItems,          //Do not Rename variable name
      iTotalDisplayRecords = response.totalItems,   //Do not Rename variable name
      Data = response.receiveData                   //Do not Rename variable name
    });
  }

  public async Task<IActionResult> CheckStockCodeExistsInStockTakesAndReceiveData([FromBody] JqueryDatatableParam parameters, int purpose)
  {
    ReceiveDataRequestViewModel model = new()
    {
      Purpose = purpose,
      Length = parameters.Length,
      Start = parameters.Start,
      SearchValue = parameters.SearchValue,
      SearchColumnName = parameters.SearchColumnName
    };

    string url = @$"StockTakesAndReceiving/CheckStockCodeExistsInStockTakesAndReceiveData";
    var response = await _http.PostAndParseAsync<StocktakesAndReceivingResultViewModel>(url, model);

    return Ok(new
    {
      iTotalRecords = response.totalItems,          //Do not Rename variable name
      iTotalDisplayRecords = response.totalItems,   //Do not Rename variable name
      Data = response.receiveData                   //Do not Rename variable name
    });
  }

  public async Task<IActionResult> AddRecLine([FromBody] AddReclineViewModel recline)
  {
    string url = "StockTakesAndReceiving/AddRecLine";
    await _http.PostAsync(url, recline);
    return NoContent();
  }

  public async Task<IActionResult> DeleteRecLine(ReceiveViewModel receive)
  {
    string url = "StockTakesAndReceiving/DeleteRecLine";
    await _http.PostAsync(url, receive);
    return NoContent();
  }

  public async Task<IActionResult> DeleteAllRecLine(int purpose)
  {
    string url = $"StockTakesAndReceiving/DeleteAllRecLineAndRecHead?purpose={purpose}";
    await _http.PostAsync(url, null);
    return NoContent();
  }

  public async Task<IActionResult> DeleteAllRecLineAndRecHead(int purpose)
  {
    string url = $"StockTakesAndReceiving/DeleteAllRecLineAndRecHead?purpose={purpose}";
    await _http.PostAsync(url, null);
    return NoContent();
  }

  public async Task<IActionResult> GetCost(string stockCode)
  {
    string url = $"StockTakesAndReceiving/GetCost?stockCode={stockCode}";
    decimal cost = await _http.GetAndParseAsync<decimal>(url);
    return Ok(cost);
  }

  public async Task<IActionResult> AddAllStocksToRecLine([FromBody] StockTakesAndRecievingViewModel model)
  {
    var sellInactivePolicy = _policyService.GetPolicy(PolicyName.SELL_INACT);
    model.SellInactivePolicy = sellInactivePolicy;

    string url = "StockTakesAndReceiving/AddAllStocksToRecLine";
    bool added = await _http.PostAndParseAsync<bool>(url, model);
    return Ok(added);
  }

  public async Task<IActionResult> Suspend(int purpose)
  {
    string url = $"StockTakesAndReceiving/Suspend?purpose={purpose}";
    await _http.GetAndParseAsync<bool>(url);
    return NoContent();
  }

  public async Task<IActionResult> Unsuspend(int purpose)
  {
    string url = $"StockTakesAndReceiving/Unsuspend?purpose={purpose}";
    await _http.GetAndParseAsync<bool>(url);
    return NoContent();
  }

  public async Task<IActionResult> GetRecLineSusCount(int purpose)
  {
    string url = $"StockTakesAndReceiving/GetRecLineSusCount?purpose={purpose}";
    int count = await _http.GetAndParseAsync<int>(url);
    return Ok(count);
  }

  [HttpPost]
  public async Task<IActionResult> Process([FromBody] StockProcessParametersViewModel parameters)
  {
    string url = "StockTakesAndReceiving/Process";
    bool processed = await _http.PostAndParseAsync<bool>(url, parameters);
    return Ok(processed);
  }

  public async Task<IActionResult> GetStockReceivingExportData()
  {
    string url = "StockTakesAndReceiving/GetStockReceivingExportData";
    var exportData = await _http.GetAndParseAsync<IEnumerable<StockReceivingExportViewModel>?>(url);
    return Ok(exportData);
  }

  public async Task<IActionResult> GetStockTakesExportData()
  {
    string url = "StockTakesAndReceiving/GetStockTakesExportData";
    var exportData = await _http.GetAndParseAsync<StockTakesExportViewModel>(url);
    return Ok(exportData);
  }

  public async Task<IActionResult> ImportFromFile([FromBody] string text, string exceptedTax, int costDec, int purpose)
  {
    string url = $"StockTakesAndReceiving/ImportFromFile?exceptedTax={exceptedTax}&costDec={costDec}&purpose={purpose}";
    var badLines = await _http.PostAndParseAsync<List<string>>(url, text);
    return Ok(badLines);
  }

  public async Task<IActionResult> GetCountOfInventoryDataForMonth(int month)
  {
    string url = $"StockTakesAndReceiving/GetCountOfInventoryDataForMonth?month={month}";
    bool result = await _http.GetAndParseAsync<bool>(url);
    return Ok(result);
  }

  public async Task<IActionResult> UpdateQuantityForStockTakes(string stockCode, decimal quantity)
  {
    string costType = _policyService.GetPolicy(PolicyName.COST_TYPE);
    string url = $"StockTakesAndReceiving/UpdateQuantityForStockTakes?stockCode={stockCode}&quantity={quantity}&costType={costType}";
    await _http.GetAsync(url);
    return NoContent();
  }

  public async Task<IActionResult> UpdateTotal(string stockCode, decimal cost, decimal quantity, int sequence, bool fromText, int batchId, int purpose)
  {
    string url = $"StockTakesAndReceiving/UpdateTotal?stockCode={stockCode}&cost={cost}&quantity={quantity}&sequence={sequence}&fromText={fromText}&batchId={batchId}&purpose={purpose}";
    await _http.GetAsync(url);
    return NoContent();
  }

  public async Task<IActionResult> CalculateAllSales(int purpose, bool isUpdateInitial)
  {
    string url = $"StockTakesAndReceiving/CalculateAllSales?purpose={purpose}&isUpdateInitial={isUpdateInitial}";
    await _http.GetAsync(url);
    return NoContent();
  }

  public async Task<IActionResult> GetNotPrintedRecLinesCount(int purpose)
  {
    string url = $"StockTakesAndReceiving/GetNotPrintedRecLinesCount?purpose={purpose}";
    int count = await _http.GetAndParseAsync<int>(url);
    return Ok(count);
  }

  public async Task<IActionResult> UpdateInvoiceNumber(decimal invNum, string stockCode, int batchId, int purpose)
  {
    string url = $"StockTakesAndReceiving/UpdateInvoiceNumber?stockCode={stockCode}&invoice={invNum}&batchId={batchId}&purpose={purpose}";
    await _http.GetAsync(url);
    return NoContent();
  }
}
