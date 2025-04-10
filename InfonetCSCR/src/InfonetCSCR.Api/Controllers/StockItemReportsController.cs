namespace InfonetCSCR.Api.Controllers;

[Route("api/StockItemReports")]
[ApiController]
[Authorize]

public class StockItemReportsController : ControllerBase
{
  private readonly IStockItemReportsService _stockItemReportsService;

  public StockItemReportsController(IStockItemReportsService stockItemReportsService)
  {
    _stockItemReportsService = stockItemReportsService;
  }

  [HttpGet]
  [Route("GetStockItemHistory")]
  public async Task<IActionResult> GetStockItemHistory(string stockCode, string startDate, string endDate)
  {
    IEnumerable<StockItemHistory> stockItems;
    stockItems = await _stockItemReportsService.GetStockItemHistory(stockCode, startDate, endDate);
    return Ok(stockItems);
  }

  [HttpGet]
  [Route("GetSalesAnalysisByProduct")]
  public async Task<IActionResult> GetSalesAnalysisByProduct(string stockCode, string startDate, string endDate)
  {
    IEnumerable<SalesAnalysisByProduct>? salesAnalysisByProducts = null;
    salesAnalysisByProducts = await _stockItemReportsService.GetSalesAnalysisByProduct(stockCode, startDate, endDate);
    return Ok(salesAnalysisByProducts);
  }

  [HttpGet]
  [Route("GetVendorPurchasesByProduct")]
  public async Task<IActionResult> GetVendorPurchasesByProduct(string stockCode, string startDate, string endDate)
  {
    IEnumerable<VendorPurchasesByProduct> vendorPurchasesByProduct;
    vendorPurchasesByProduct = await _stockItemReportsService.GetVendorPurchasesByProduct(stockCode, startDate, endDate);
    return Ok(vendorPurchasesByProduct);
  }

  [HttpGet]
  [Route("sales")]
  public async Task<IActionResult> GetSalesData(string stockCode)
  {
    IEnumerable<StockItemReports> salesData;
    salesData = await _stockItemReportsService.GetSalesData(stockCode);
    return Ok(salesData);
  }

  [HttpGet]
  [Route("saleDetails")]
  public async Task<IActionResult> GetSaleDetails(string stockCode, int year, int month)
  {
    IEnumerable<SaleDetails> salesDetails = await _stockItemReportsService.GetSaleDetails(stockCode, year, month);
    return Ok(salesDetails);
  }

  [HttpGet]
  [Route("purchaseDetails")]
  public async Task<IActionResult> GetPurchaseDetails(string stockCode, int year, int month)
  {
    IEnumerable<PurchaseDetail> purchaseDetails = await _stockItemReportsService.GetPurchaseDetails(stockCode, year, month);
    return Ok(purchaseDetails);
  }

  [HttpGet]
  [Route("purchases")]
  public async Task<IActionResult> GetPurchasesData(string stockCode)
  {
    IEnumerable<StockItemReports> purchasesData;
    purchasesData = await _stockItemReportsService.GetPurchasesData(stockCode);
    return Ok(purchasesData);
  }
}
