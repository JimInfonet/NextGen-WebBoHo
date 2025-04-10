namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStoreService
{
  Task<IEnumerable<Store>?> GetStoreIdsByVendorIds(List<string> vendorIds);
  Task<IEnumerable<Store>> GetStoresByStoreIds(IEnumerable<string> storeIds);
  Task<IEnumerable<Store>> GetStoresDataByStoreIds(IEnumerable<string> storeIds);
  Task<IEnumerable<Store>> GetStoresByVendorIds(IEnumerable<string> vendorIds);
  Task<IEnumerable<Store>> GetStoresByPriceGroupIds(IEnumerable<string> priceGroupIds);
  Task<IEnumerable<Store>> GetStoresByRegionIds(IEnumerable<string> regionIds);
  Task<IEnumerable<StoresWithRegionAndPriceGroup>?> GetStoresWithRegionAndPriceGroup();
  Task<IEnumerable<Store>> GetAllStoresListAsync();
  Task<Store> GetStoreByStoreId(string storeId);
  Task<bool> AddStore(Store store, string? copyStoreId);
  Task<bool> UpdateStore(Store store);
  Task<bool> DeleteStore(string storeId);
  Task<IList<Store>?> GetStoreBySelectedRegion(string regionId);
}
