namespace InfonetCSCR.Core.Abstractions.Repositories;
public interface IStockItemReportsRepository
{
  Task<IEnumerable<StockItemReports>> GetSalesDataAsync(string stockCode);
  Task<IEnumerable<StockItemReports>> GetPurchasesDataAsync(string stockCode);
  Task<IEnumerable<StockItemHistory>> GetStockItemHistoryAsync(string stockCode, string startDate, string endDate);
  Task<IEnumerable<SalesAnalysisByProduct>?> GetSalesAnalysisByProductAsync(string stockCode, string startDate, string endDate);
  Task<IEnumerable<VendorPurchasesByProduct>> GetVendorPurchasesByProductAsync(string stockCode, string startDate, string endDate);
  Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsAsync(string stockCode, int year, int month);
  Task<IEnumerable<SaleDetails>> GetSaleDetailsAsync(string stockCode, int year, int month);
}
