namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStoreRepository
{
  Task<IEnumerable<Store>?> GetStoreIdsByVendorIds(List<string> vendorIds);
  Task<IEnumerable<Store>> GetStoresByStoreIdsAsync(IEnumerable<string> storeIds);
  Task<IEnumerable<StoresWithRegionAndPriceGroup>?> GetStoresWithRegionAndPriceGroup();
  Task<IEnumerable<Store>> GetAllStoresAsync();
  Task<IEnumerable<StoreOptionDto>> GetStoreDropdownValuesAsync();
  Task<Store> GetStoreByStoreIdAsync(string storeId);
  Task<bool> AddStoreAsync(Store store);
  Task<bool> UpdateStoreAsync(Store store);
  Task<bool> DeleteStoreAsync(string storeId, string table);
  Task<IList<string>> GetTableColumnsAsync (string tableName);
  Task<bool> InsertIntoTablesAsync(string table, string sourceFields, string destFields, string sourceStore);
  Task<IList<Store>?> GetStoreBySelectedRegionAsync(string regionId);
}
