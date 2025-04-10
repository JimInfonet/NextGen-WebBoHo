namespace InfonetCSCR.Services;

public class StockItemReportsService : IStockItemReportsService
{
  private readonly IUnitOfWork _unitOfWork;

  public StockItemReportsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransConnectionString);
  }

  public async Task<IEnumerable<SalesAnalysisByProduct>?> GetSalesAnalysisByProduct(string stockCode, string startDate, string endDate)
  {
    IEnumerable<SalesAnalysisByProduct>? salesAnalysisByProducts = await _unitOfWork.StockItemReports.GetSalesAnalysisByProductAsync(stockCode, startDate, endDate);
    _unitOfWork.Commit();
    return salesAnalysisByProducts;
  }

  public async Task<IEnumerable<StockItemHistory>> GetStockItemHistory(string stockCode, string startDate, string endDate)
  {
    IEnumerable<StockItemHistory> stockItemHistories = await _unitOfWork.StockItemReports.GetStockItemHistoryAsync(stockCode, startDate, endDate);
    _unitOfWork.Commit();
    return stockItemHistories;
  }

  public async Task<IEnumerable<VendorPurchasesByProduct>> GetVendorPurchasesByProduct(string stockCode, string startDate, string endDate)
  {
    IEnumerable<VendorPurchasesByProduct> vendorPurchasesByProducts = await _unitOfWork.StockItemReports.GetVendorPurchasesByProductAsync(stockCode, startDate, endDate);
    _unitOfWork.Commit();
    return vendorPurchasesByProducts;
  }

  public async Task<IEnumerable<StockItemReports>> GetPurchasesData(string stockCode)
  {
    IEnumerable<StockItemReports> stockItemReports = await _unitOfWork.StockItemReports.GetPurchasesDataAsync(stockCode);
    _unitOfWork.Commit();
    return stockItemReports;
  }

  public async Task<IEnumerable<StockItemReports>> GetSalesData(string stockCode)
  {
    IEnumerable<StockItemReports> stockItemReports = await _unitOfWork.StockItemReports.GetSalesDataAsync(stockCode);
    _unitOfWork.Commit();
    return stockItemReports;
  }

  public async Task<IEnumerable<SaleDetails>> GetSaleDetails(string stockCode, int year, int month)
  {
    IEnumerable<SaleDetails> saleDetails = await _unitOfWork.StockItemReports.GetSaleDetailsAsync(stockCode, year, month);
    _unitOfWork.Commit();
    return saleDetails;
  }

  public async Task<IEnumerable<PurchaseDetail>> GetPurchaseDetails(string stockCode, int year, int month)
  {
    IEnumerable<PurchaseDetail> purchaseDetails = await _unitOfWork.StockItemReports.GetPurchaseDetailsAsync(stockCode, year, month);
    _unitOfWork.Commit();
    return purchaseDetails;
  }
}
