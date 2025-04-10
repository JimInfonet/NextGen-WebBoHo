namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPurchaseHeaderRepository
{
  Task<IEnumerable<PurchaseHeader>> GetPurchaseHeadersByPOStatusAsync(string poStatus, int columnIndex);
  Task<PurchaseHeader?> GetPurchaseHeadersByPurchaseNoAsync(long purchaseNo);
  Task<bool> UpdatePurchaseHeaderEmailedStatusAsync(PurchaseHeader purchaseheader);
  Task<bool> AddPurchaseHeaderAsync(PurchaseHeader purchaseHeader);
  Task<long> GetNextPurchaseNumberAsync();
  Task<bool> DeletePurchaseHeaderByPurchaseNoAsync(string purchaseNo);
  Task<bool> UpdatePurchaseHeaderAsync(PurchaseHeader purchaseheader);
  Task<bool> UpdatePurchaseHeaderSurChargeDiscountAsync(PurchaseHeader purchaseheader);
  Task<IEnumerable<PurchasingAndReceivingReportDto>> GetPurchasingAndReceivingReportDataAsync(int purchaseNo, string vendorSortCondition);
  Task<bool> UpdateOrInsertPurchaseCostChangeAsync(PurchaseCostChange purchaseCostChange);
  Task<bool> UpdateCurrentStockByPurchaseNoAndLineNumAsync(long purchaseNo, int lineNum, decimal currentStock);
  Task<bool> UpdateHeaderFileNameAsync(string headerFile, long purchaseNo);
  Task<IEnumerable<PurchaseHeader>> GetPurchaseHeaderByHeaderFileAsync(string headerFile);
}
