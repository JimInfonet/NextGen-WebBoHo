namespace InfonetCSCR.Web.Abstractions.Services;

public interface IVendorsService
{
  Task<IEnumerable<VendorViewModel>> GetVendors();
  VendorsPolicyRecordViewModel GetVendorsPolicies();
  Task<VendorAddRecordViewModel> AddVendorForm();
  Task<VendorAddRecordViewModel> EditVendorForm(string vendorId);
  Task<IEnumerable<VendorSortSetupViewModel>> GetVendorSortSetup();
  Task<bool> SaveVendor(VendorAddRecordViewModel vendorModel, bool isAdd);
  Task<string> CheckAndMakeCodeExistsMessage(string code);
  Task<bool> DeleteVendor(string vendorId);
  Task<VendorDeleteValidationRecordViewModel> CheckDeleteValidation(string vendorId);
  Task<IEnumerable<VendorViewModel>> GetAllVendorName();
  Task<IEnumerable<VendorLinkViewModel>> GetVendorLink(string vendorId);
  Task<StoreRegionRecordViewModel> GetAllStoresAndRegions();
  Task<bool> DeleteVendorLink(VendorLinkViewModel vendorLink);
  Task<bool> AddVendorLink(VendorLinkViewModel vendorLink);
}
