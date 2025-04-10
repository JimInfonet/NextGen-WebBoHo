namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPurchaseDetailRepository
{
  Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNoAsync(long purchaseNo);
  Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNoAndOrderByVendorsAsync(long purchaseNo, string vendorSort);
  Task<bool> DeletePurchaseDetailByPurchaseNoAsync(string purchaseNo);
  Task<double> CalculatPODiscount(string purchaseNo);
  Task<POSubTotalDataDto> GetPOSubTotalDataAsync(string purchaseNo);
  Task<double> CalculatePOGSTTotal(string purchaseNo);
  Task<int> GetLinesCountAsync(string purchaseNo);
  Task<int> GetMaxLineNumAsync(string purchaseNo);
  Task<bool> UpdatePurchaseHeaderLineCount(string purchaseNo);
  Task<bool> AddPurchaseDetailBatchAsync(IList<PurchaseDetail> model);
  Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNumberAndVendorSortAsync(long purchaseNo, string? vendorSort);
  Task<bool> UpdatePurchaseDetailAsync(PurchaseDetail purchaseDetail);
  Task<PurchaseDetail> GetPurchaseDetailByPurchaseNumberAsync(long purchaseNo, int lineNumber, string partNumber);
  Task<bool> DeletePurchaseDetailAsync(
    string purchaseNo,
    string partNumber,
    string lineNumber);
  Task<bool> UpdatePurchaseDetailGSTByPurchaseNOAsync(IEnumerable<PurchaseDetail> purchaseDetails);
  Task<PurchaseDetail> GetPurchaseDetailsByPurchaseNoAndStockCodeAsync(long? purchaseNo, string stockCode);
  Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsWithStockAsync(long purchaseNo);
  Task<bool> UpdatePurchaseDetailAsync(long purchaseNo);
  Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNoListOrderByLinenumAsync(List<long> purchaseNoList);
  Task<IEnumerable<POSubTotalDataDto>> GetCalculatedDiscountsAsync(List<long> purchaseNoList);
}
