namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IVendorRepository
{
  Task<IEnumerable<Vendor>> GetAllVendorsAsync();
  Task<Vendor> GetVendorByCodeAsync(string code);
  Task<IEnumerable<Vendor>> GetVendorsByVendorIdsAsync(IEnumerable<string> vendorIds);
  Task<IEnumerable<Vendor>> GetActiveVendorsOrderByColumnNumberAsync(int columnNumber);
  Task<double> GetPoDiscountAsync(string vendorId);
  Task<bool> GetIsTaxIncludeInPOAsync(string vendorId);
  Task<string> GetCustAccountAsync(string vendorId);
  Task<Vendor?> GetVendorByPurchaseNoAsync(long purchaseNo);
  Task<Vendor> GetVendorByVendorCodeAsync(string code, bool isBackOffice);
  Task<bool> AddVendorAsync(Vendor vendor, bool isBackOffice);
  Task<bool> UpdateVendorAsync(Vendor vendor, bool isBackOffice);
  Task<int> GetVendorCountStockMstAsync(string vendorId);
  Task<int> GetVendorCountStockCostsAsync(string vendorId);
  Task<int> GetVendorCountStockPricesAsync(string vendorId);
  Task VendorDeleteFromVendorSortAsync(string vendorId);
  Task VendorDeleteFromVendorAsync(string vendorId);
  Task VendorDeleteFromStockCostsAsync(string vendorId);
  Task VendorDeleteFromStockPricesAsync(string vendorId);
  Task SetTEVendorNullProductTaxExemptAsync(string vendorId);
  Task<IEnumerable<Vendor>> GetAllVendorNameAsync();
  Task<IEnumerable<VendorLink>> GetVendorLinksAsync(string vendorId);
  Task<bool> DeleteVendorStoreAsync(string vendorId, string storeId);
  Task<bool> DeleteVendorRegionAsync(string vendorId, string regionId);
  Task<bool> AddVendorLinkAsync(VendorLink vendorLink);
  Task<IEnumerable<Vendor>> GetVendorsOrderByColumnNumberAsync(int columnNumber);
}
