namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class StockItemReportsController : Controller
{
  private readonly IHttpService _http;

  public StockItemReportsController(IHttpService http)
  {
    _http = http;
  }

  // GET: StockItemReportsController
  public async Task<IActionResult> Index(string stockCode, string description, string vendor, string vendorPart)
  {
    IEnumerable<StockItemReportsViewModel>? salesData = await GetSalesData(stockCode);
    var (quantityViewData, amountViewData) = StockItemReportsHelper.mapReportData(salesData);

    ViewBag.SalesQuantityViewData = quantityViewData;
    ViewBag.SalesAmountViewData = amountViewData;

    IEnumerable<StockItemReportsViewModel>? purchasesData = await GetPurchasesData(stockCode);
    (quantityViewData, amountViewData) = StockItemReportsHelper.mapReportData(purchasesData);

    string url = $"StockPrice/GetVendorPart?stockCode={stockCode}&vendorId={vendor}";
    var vendorPartViewModel = await _http.GetAndParseAsync<VendorPartViewModel?>(url);
    ViewBag.VendorPart = vendorPartViewModel.PartNo;

    if (vendor != "ALL" && vendor != null)
    {
      vendor = $"{vendor} {await GetVendorNameByCode(vendor)}";
    }

    ViewBag.PurchasesQuantityViewData = quantityViewData;
    ViewBag.PurchasesAmountViewData = amountViewData;
    ViewBag.StockCode = stockCode;
    ViewBag.Description = description;
    ViewBag.Vendor = vendor;

    return View();
  }

  public async Task<IEnumerable<StockItemReportsViewModel>?> GetSalesData(string stockCode)
  {
    string url = $"StockItemReports/Sales?stockcode={stockCode}";
    var salesData = await _http.GetAndParseAsync<IEnumerable<StockItemReportsViewModel>>(url);
    return salesData;
  }

  public async Task<IActionResult> GetSaleDetails(string stockCode, int year, int month)
  {
    string url = $"stockitemreports/saleDetails?stockcode={stockCode}&year={year}&month={month}";
    var saleDetailsData = await _http.GetAndParseAsync<IEnumerable<SaleDetailsViewModel>>(url);
    return Ok(saleDetailsData);
  }

  public async Task<IActionResult> GetPurchaseDetails(string stockCode, int year, int month)
  {
    string url = $"StockItemReports/PurchaseDetails?stockcode={stockCode}&year={year}&month={month}";
    var purchaseDetails = await _http.GetAndParseAsync<IEnumerable<PurchaseDetailViewModel>>(url);
    return Ok(purchaseDetails);
  }

  public async Task<IEnumerable<StockItemReportsViewModel>?> GetPurchasesData(string stockCode)
  {
    string url = $"stockitemreports/purchases?stockcode={stockCode}";
    var purchases = await _http.GetAndParseAsync<IEnumerable<StockItemReportsViewModel>>(url);
    return purchases;
  }

  public async Task<string> GetVendorNameByCode(string code)
  {
    string url = $"vendors?code={code}";
    var vendors = await _http.GetAndParseAsync<IEnumerable<VendorViewModel>>(url);
    return vendors?.FirstOrDefault()?.Name ?? string.Empty;
  }

    //public async Task<bool> RefreshToken()
    //{
    //    return await _http.RefreshToken();
    //}
    public Task<bool> RefreshToken()
    {
        return _http.RefreshToken();
    }
}
