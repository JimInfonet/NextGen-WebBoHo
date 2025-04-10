namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPurchaseDetailService
{
  Task<double> CalculatePOSubTotal(string purchaseNo, string vendorId, bool useCurrentVendor = false);
  Task<bool> AddPurchaseDetails(
    long purchanseNo,
    string vendorId,
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPoliciesDto,
    Vendor vendor);
  Task<POFileDto?> GetPOFileData(long purchaseNo);
  Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNumber(long purchaseNo);
  Task<bool> UpdatePurchaseDetail(
    PurchaseDetail purchaseDetail,
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPoliciesDto,
    string vendorId);
  Task<bool> DeletePurchaseDetail(
    string purchaseNo,
    string partNumber,
    string lineNumber);
  Task<VendorUnitCostPercentageRecord> GetVendorUnitCostPercentage(
    PurchaseDetail purchaseDetail,
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPoliciesDto,
    string vendorId);
  Task<bool> UpdatePurchaseDetailGSTByPurchaseNO(IEnumerable<PurchaseDetail> purchaseDetails);
  Task<bool> AddTaxToPurchaseDetail(AddTaxToPurchaseDetailParameterDto addTaxToPurchaseDetail);
}
