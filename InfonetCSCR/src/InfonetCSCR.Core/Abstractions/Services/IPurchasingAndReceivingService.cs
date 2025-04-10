namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPurchasingAndReceivingService
{
  Task<bool> AddPurchasingAndReceiving(
    PurchasingAndReceivingDto purchasingAndReceivingDto,
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPolicies);
  Task<bool> DeletePurchasingAndReceivingByPurchaseNo(string purchaseNo);
  Task<IEnumerable<PurchasingAndReceivingReportDto>> GetPurchasingAndReceivingReportData(int purchaseNo);
}
