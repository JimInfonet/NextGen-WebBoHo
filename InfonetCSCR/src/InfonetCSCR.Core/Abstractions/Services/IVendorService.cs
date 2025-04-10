namespace InfonetCSCR.Core.Abstractions.Services;

public interface IVendorService
{
  Task<IEnumerable<Vendor>> GetAllVendorsAsync();
  Task<IEnumerable<Vendor>> GetVendorByCodeAsync(string code);
  Task<IEnumerable<Vendor>> GetVendorsByVendorIds(IEnumerable<string> VendorIds);
  Task<IEnumerable<Vendor>> GetActiveVendorsOrderByColumnNumber(int columnNumber);
  Task<Vendor?> GetVendorByPurchaseNo(long purchaseNo);
  Task<Vendor> GetVendorByVendorCode(string code);
  Task<IEnumerable<VendorSortSetup>> GetVendorsSortSetup();
  Task<IEnumerable<VendorSort>> GetVendorsSort(string vendorId);
  Task<bool> SaveVendor(VendorAddRecord vendorAdd, bool isAdd);
  Task<bool> DeleteVendor(string vendorId);
  Task<VendorDeletionCaseRecord> CheckDeleteValidation(string vendorId);
  Task<IEnumerable<Vendor>> GetAllVendorsNameAsync();
  Task<IEnumerable<VendorLink>> GetVendorLinksAsync(string vendorId);
  Task<StoreRegionRecord> GetAllStoresAndRegions();
  Task<bool> DeleteVendorLink(VendorLink vendorLink);
  Task<bool> AddVendorLink(VendorLink vendorLink);
  Task<IEnumerable<Vendor>> GetVendorsOrderByColumnNumber(int columnNumber);
}
