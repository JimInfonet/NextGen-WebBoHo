namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IVendorSortRepository
{
  Task<IEnumerable<VendorSort>> GetVendorSortsAsync(string vendorId);
  Task<IEnumerable<VendorSort>> GetAllVendorSortsByVendorListAsync();
  Task<VendorSortSetup> GetVendorSortSetupsAsync(string sortItem);
  Task<IEnumerable<VendorSortSetup>> GetVendorSortSetupsBySortItemsAsync(IEnumerable<string> sortItems);
  Task<IEnumerable<VendorSortSetup>> GetAllVendorSortSetupsBySortItemsAsync();
  Task DeleteVendorSortAsync(IEnumerable<string> sortItems, string vendorId);
  Task AddVendorSortAsync(IEnumerable<VendorSort> sortItems);
  Task<int> GetNextSortOrderAsync(string vendorId);
}

