namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockItemReportsService
{
  Task<IEnumerable<StockItemReports>> GetSalesData(string stockCode);
  Task<IEnumerable<StockItemReports>> GetPurchasesData(string stockCode);
  Task<IEnumerable<StockItemHistory>> GetStockItemHistory(string stockCode, string startDate, string endDate);
  Task<IEnumerable<SalesAnalysisByProduct>?> GetSalesAnalysisByProduct(string stockCode, string startDate, string endDate);
  Task<IEnumerable<VendorPurchasesByProduct>> GetVendorPurchasesByProduct(string stockCode, string startDate, string endDate);
  Task<IEnumerable<SaleDetails>> GetSaleDetails(string stockCode, int year, int month);
  Task<IEnumerable<PurchaseDetail>> GetPurchaseDetails(string stockCode, int year, int month);
}
