namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TextReportsController : ControllerBase
{
  private readonly ITextReportsService _textReportsService;
  private readonly IStockItemService _stockItemService;

  public TextReportsController(ITextReportsService textReportsService, IStockItemService stockItemService)
  {
    _textReportsService = textReportsService;
    _stockItemService = stockItemService;
  }

  // GET api/TextReports/GetAllTextReportsList
  [HttpGet]
  [Route("GetAllTextReportsList")]
  public async Task<IActionResult> GetAllTextReportsList()
  {
    var textReportsList = await _textReportsService.GetAllTextReportsListAsync();
    return Ok(textReportsList);
  }

  // POST api/TextReports/GetCardDetailsReportData
  [HttpPost]
  [Route("GetCardDetailsReportData")]
  public async Task<IActionResult> GetCardDetailsReportData(TextReportParams textReportParams)
  {
    var cardDetails = await _textReportsService.GetCardDetailsReportDataAsync(textReportParams);
    return Ok(cardDetails);
  }

  // POST api/TextReports/GetFleetCardFuelReportData
  [HttpPost]
  [Route("GetFleetCardFuelReportData")]
  public async Task<IActionResult> GetFleetCardFuelReportData(TextReportParams textReportParams)
  {
    var fleetCards = await _textReportsService.GetFleetCardFuelReportData(textReportParams);
    return Ok(fleetCards);
  }

  // GET api/TextReports/GetCardDiscountFromCardName
  [HttpGet]
  [Route("GetCardDiscountFromCardName")]
  public async Task<IActionResult> GetCardDiscountFromCardName(string cardName)
  {
    DiscountDto? discountProperties = await _textReportsService.GetCardDiscountFromCardName(cardName);
    return Ok(discountProperties);
  }

  // POST api/TextReports/GetTotalizerReadingReport
  [HttpPost]
  [Route("GetTotalizerReadingReport")]
  public async Task<IActionResult> GetTotalizerReadingReport(TextReportParams textReportParams)
  {
    var totalizerReadings = await _textReportsService.GetTotalizerReadingReportData(textReportParams);
    return Ok(totalizerReadings);
  }
  
  // POST api/TextReports/GetCreditCardSummaryReport
  [HttpPost]
  [Route("GetCreditCardSummaryReport")]
  public async Task<IActionResult> GetCreditCardSummaryReport(TextReportParams textReportParams)
  {
    var creditCardSummary = await _textReportsService.GetCreditCardSummaryReportData(textReportParams);
    return Ok(creditCardSummary);
  }
  
  // POST api/TextReports/GetARCustomerSalesReport
  [HttpPost]
  [Route("GetARCustomerSalesReport")]
  public async Task<IActionResult> GetARCustomerSalesReport(TextReportParams textReportParams)
  {
    var creditCardSummary = await _textReportsService.GetARCustomerSalesReportData(textReportParams);
    return Ok(creditCardSummary);
  }
  
  // POST api/TextReports/GetDailyFuelDiscountReport
  [HttpPost]
  [Route("GetDailyFuelDiscountReport")]
  public async Task<IActionResult> GetDailyFuelDiscountReport(TextReportParams textReportParams)
  {
    var creditCardSummary = await _textReportsService.GetDailyFuelDiscountReportData(textReportParams);
    return Ok(creditCardSummary);
  }
  
  // POST api/TextReports/GetSaleVarianceReport
  [HttpPost]
  [Route("GetSaleVarianceReport")]
  public async Task<IActionResult> GetSaleVarianceReport(TextReportParams textReportParams)
  {
    var creditCardSummary = await _textReportsService.GetSaleVarianceReportData(textReportParams);
    return Ok(creditCardSummary);
  }
  
  // POST api/TextReports/GetShiftAnalysisReportData
  [HttpPost]
  [Route("GetShiftAnalysisReportData")]
  public async Task<IActionResult> GetShiftAnalysisReportData(TextReportParams textReportParams)
  {
    var shiftAnalysisData = await _textReportsService.GetShiftAnalysisReportData(textReportParams);
    return Ok(shiftAnalysisData);
  }

  // POST api/TextReports/GetFleetCardSaleReportData
  [HttpPost]
  [Route("GetFleetCardSaleReportData")]
  public async Task<IActionResult> GetFleetCardSaleReportData(TextReportParams textReportParams)
  {
    var fleetCardSaleReportData = await _textReportsService.GetFleetCardSaleReportData(textReportParams);
    return Ok(fleetCardSaleReportData);
  }

  // POST api/TextReports/GetAffdaSaleDetailReportData
  [HttpPost]
  [Route("GetAffdaSaleDetailReportData")]
  public async Task<IActionResult> GetAffdaSaleDetailReportData(TextReportParams textReportParams)
  {
    var affdaSaleDetailReportData = await _textReportsService.GetAffdaSaleDetailReportData(textReportParams);
    return Ok(affdaSaleDetailReportData);
  }

  // POST api/TextReports/GetAffdaTaxSaleSummaryReportData
  [HttpPost]
  [Route("GetAffdaTaxSaleSummaryReportData")]
  public async Task<IActionResult> GetAffdaTaxSaleSummaryReportData(TextReportParams textReportParams)
  {
    var affdaSaleSummaryReportData = await _textReportsService.GetAffdaTaxSaleSummaryReportData(textReportParams);
    return Ok(affdaSaleSummaryReportData);
  }

  [HttpPost]
  [Route("GetTankDipReportData")]
  public async Task<IActionResult> GetTankDipReportData(TextReportParams textReportParams)
  {
    var tankDipData = await _textReportsService.GetTankDipReportData(textReportParams);
    return Ok(tankDipData);
  }

  // POST api/TextReports/GetAdagioReportData
  [HttpPost]
  [Route("GetAdagioReportData")]
  public async Task<IActionResult> GetAdagioReportData(TextReportParams textReportParams)
  {
    return Ok(await _textReportsService.GetAdagioReportData(textReportParams));
  }

  [HttpPost]
  [Route("GetIncompleteTransactionsReportData")]
  public async Task<IActionResult> GetIncompleteTransactionsReportData(TextReportParams textReportParams)
  {
    var incompleteTransactionsReportData = await _textReportsService.GetIncompleteTransactionsReportData(textReportParams);
    return Ok(incompleteTransactionsReportData);
  }

  [HttpPost]
  [Route("GetFinancialReportSalesDeptData")]
  public async Task<IActionResult> GetFinancialReportSalesDeptData(TextReportParams textReportParams)
  {
    var financialReportSalesDeptData = await _textReportsService.GetFinancialReportSalesDeptData(textReportParams);
    return Ok(financialReportSalesDeptData);
  }

  [HttpPost]
  [Route("GetFinancialReportSalesData")]
  public async Task<IActionResult> GetFinancialReportSalesData(TextReportParams textReportParams)
  {
    var financialReportSalesData = await _textReportsService.GetFinancialReportSalesData(textReportParams);
    return Ok(financialReportSalesData);
  }

  [HttpPost]
  [Route("GetFinancialReportSaleTaxesData")]
  public async Task<IActionResult> GetFinancialReportSaleTaxesData(TextReportParams textReportParams)
  {
    var financialReportSaleTaxesData = await _textReportsService.GetFinancialReportSaleTaxesData(textReportParams);
    return Ok(financialReportSaleTaxesData);
  }

  [HttpPost]
  [Route("GetFinancialReportTaxesData")]
  public async Task<IActionResult> GetFinancialReportTaxesData(TextReportParams textReportParams)
  {
    var financialReportTaxesData = await _textReportsService.GetFinancialReportTaxesData(textReportParams);
    return Ok(financialReportTaxesData);
  }

  [HttpPost]
  [Route("GetFinancialReportSaleChargesData")]
  public async Task<IActionResult> GetFinancialReportSaleChargesData(TextReportParams textReportParams)
  {
    var financialReportSaleChargesData = await _textReportsService.GetFinancialReportSaleChargesData(textReportParams);
    return Ok(financialReportSaleChargesData);
  }

  [HttpPost]
  [Route("GetFinancialReportChargesData")]
  public async Task<IActionResult> GetFinancialReportChargesData(TextReportParams textReportParams)
  {
    var financialReportChargesData = await _textReportsService.GetFinancialReportChargesData(textReportParams);
    return Ok(financialReportChargesData);
  }

  [HttpPost]
  [Route("GetFinancialReportSaleTendersData")]
  public async Task<IActionResult> GetFinancialReportSaleTendersData(TextReportParams textReportParams)
  {
     var financialReportSaleTendersData = await _textReportsService.GetFinancialReportSaleTendersData(textReportParams);
     return Ok(financialReportSaleTendersData);
  }

  [HttpPost]
  [Route("GetFinancialReportTendersData")]
  public async Task<IActionResult> GetFinancialReportTendersData(TextReportParams textReportParams)
  {
      var financialReportTendersData = await _textReportsService.GetFinancialReportTendersData(textReportParams);
      return Ok(financialReportTendersData);
  }

  [HttpPost]
  [Route("GetFinancialReportARTendersData")]
  public async Task<IActionResult> GetFinancialReportARTendersData(TextReportParams textReportParams)
  {
    var financialReportARTendersData = await _textReportsService.GetFinancialReportARTendersData(textReportParams);
    return Ok(financialReportARTendersData);
  }

  [HttpPost]
  [Route("GetFinancialReportARPayData")]
  public async Task<IActionResult> GetFinancialReportARPayData(TextReportParams textReportParams)
  {
    var financialReportARPayData = await _textReportsService.GetFinancialReportARPayData(textReportParams);
    return Ok(financialReportARPayData);
  }

  [HttpPost]
  [Route("GetFinancialReportPaymntTendersData")]
  public async Task<IActionResult> GetFinancialReportPaymntTendersData(TextReportParams textReportParams)
  {
    var financialReportPaymntTendersData = await _textReportsService.GetFinancialReportPaymntTendersData(textReportParams);
    return Ok(financialReportPaymntTendersData);
  }

  [HttpPost]
  [Route("GetFinancialReportPaymntData")]
  public async Task<IActionResult> GetFinancialReportPaymntData(TextReportParams textReportParams)
  {
    var financialReportPaymntData = await _textReportsService.GetFinancialReportPaymntData(textReportParams);
    return Ok(financialReportPaymntData);
  }

  [HttpPost]
  [Route("GetFinancialReportPayoutData")]
  public async Task<IActionResult> GetFinancialReportPayoutData(TextReportParams textReportParams)
  {
    var financialReportPayoutData = await _textReportsService.GetFinancialReportPayoutData(textReportParams);
    return Ok(financialReportPayoutData);
  }

  [HttpPost]
  [Route("GetFinancialReportOverPayData")]
  public async Task<IActionResult> GetFinancialReportOverPayData(TextReportParams textReportParams)
  {
    var financialReportOverPayData = await _textReportsService.GetFinancialReportOverPayData(textReportParams);
    return Ok(financialReportOverPayData);
  }

  [HttpPost]
  [Route("GetFinancialReportRefundData")]
  public async Task<IActionResult> GetFinancialReportRefundData(TextReportParams textReportParams)
  {
    var financialReportRefundData = await _textReportsService.GetFinancialReportRefundData(textReportParams);
    return Ok(financialReportRefundData);
  }

  [HttpPost]
  [Route("GetFinancialReportDelVoidData")]
  public async Task<IActionResult> GetFinancialReportDelVoidData(TextReportParams textReportParams)
  {
    var financialReportDelVoidData = await _textReportsService.GetFinancialReportDelVoidData(textReportParams);
    return Ok(financialReportDelVoidData);
  }

  [HttpPost]
  [Route("GetFinancialReportCustomerCntData")]
  public async Task<IActionResult> GetFinancialReportCustomerCntData(TextReportParams textReportParams)
  {
    var financialReportCustomerCntData = await _textReportsService.GetFinancialReportCustomerCntData(textReportParams);
    return Ok(financialReportCustomerCntData);
  }

  // POST api/TextReports/GetStockMasterInfo
  [HttpPost]
  [Route("GetStockMasterInfo")]
  public async Task<IActionResult> GetStockMasterInfo(TextReportParams textReportParams)
  {
    var stockInfo = await _stockItemService.GetStockMasterInfo(textReportParams.StartDepartment, textReportParams.EndDepartment);
    return Ok(stockInfo);
  }

  // POST api/TextReports/GetStockCostInfoBatch
  [HttpPost]
  [Route("GetStockCostInfoBatch")]
  public async Task<IActionResult> GetStockCostInfo([FromBody] StockCostInfoBatchParameterDto costInfo)
  {
    var stockCostInfo = await _stockItemService.GetStockCostInfoBatchForBackOffice(costInfo);
    return Ok(stockCostInfo);
  }

  // POST api/TextReports/GetStockPriceInfoBatch
  [HttpPost]
  [Route("GetStockPriceInfoBatch")]
  public async Task<IActionResult> GetStockPriceInfo([FromBody] IEnumerable<StockCostInfo> stockCostInfo)
  {
    var stockPriceInfo = await _stockItemService.GetStockPriceInfoBatch(stockCostInfo);
    return Ok(stockPriceInfo);
  }

  // POST api/TextReports/GetStockTaxInfoBatch
  [HttpPost]
  [Route("GetStockTaxInfoBatch")]
  public async Task<IActionResult> GetStockTaxInfoBatch([FromBody] List<string> stockCodes, [FromQuery] string? storeID)
  {
    var stockTaxInfoBatch = await _stockItemService.GetStockTaxInfoBatchForBackOffice(stockCodes, storeID);
    return Ok(stockTaxInfoBatch);
  }

  // POST api/TextReports/GetStockChargeInfoBatch
  [HttpPost]
  [Route("GetStockChargeInfoBatch")]
  public async Task<IActionResult> GetStockChargeInfoBatch([FromBody] List<string> stockCodes, [FromQuery] string? storeID)
  {
    var stockChargeInfoBatch = await _stockItemService.GetStockChargeInfoBatchForBackOffice(stockCodes, storeID);
    return Ok(stockChargeInfoBatch);
  }

  [HttpPost]
  [Route("GetEmployeeCommissionReportData")]
  public async Task<IActionResult> GetEmployeeCommissionReportData(TextReportParams textReportParams)
  {
    var employeeCommissionReportData = await _textReportsService.GetEmployeeCommissionReportData(textReportParams);
    return Ok(employeeCommissionReportData);
  }

  [HttpPost]
  [Route("GetEmployeeCommissionTableData")]
  public async Task<IActionResult> GetEmployeeCommissionTableData(TextReportParams textReportParams)
  {
    var employeeCommissionTableData = await _textReportsService.GetEmployeeCommissionTableData(textReportParams);
    return Ok(employeeCommissionTableData);
  }

  [HttpPost]
  [Route("GetCustomerSalesDetailReportData")]
  public async Task<IActionResult> GetCustomerSalesDetailReportData(TextReportParams textReportParams)
  {
    return Ok(await _textReportsService.GetCustomerSalesDetailReportDataAsync(textReportParams));
  }
}
