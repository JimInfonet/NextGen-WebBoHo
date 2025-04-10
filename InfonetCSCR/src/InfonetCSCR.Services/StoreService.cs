namespace InfonetCSCR.Services;

public class StoreService : IStoreService
{
  private readonly IUnitOfWork _unitOfWork;
  public StoreService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<Store>?> GetStoreIdsByVendorIds(List<string> vendorIds)
  {
    IEnumerable<Store>? stores = await _unitOfWork.Store.GetStoreIdsByVendorIds(vendorIds);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<IEnumerable<Store>> GetStoresByStoreIds(IEnumerable<string> storeIds)
  {
    IEnumerable<Store> stores = await _unitOfWork.Store.GetStoresByStoreIdsAsync(storeIds);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<IEnumerable<Store>> GetStoresDataByStoreIds(IEnumerable<string> storeIds)
  {
    IEnumerable<string> storeIdsData = await _unitOfWork.StoreLinks.GetStoreIdsByStoreIds(storeIds);
    IEnumerable<Store> stores = await _unitOfWork.Store.GetStoresByStoreIdsAsync(storeIdsData);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<IEnumerable<Store>> GetStoresByVendorIds(IEnumerable<string> vendorIds)
  {
    IEnumerable<string> storeIds = await _unitOfWork.StoreLinks.GetStoreIdsByVendorIds(vendorIds);
    IEnumerable<Store> stores = await _unitOfWork.Store.GetStoresByStoreIdsAsync(storeIds);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<IEnumerable<Store>> GetStoresByPriceGroupIds(IEnumerable<string> priceGroupIds)
  {
    IEnumerable<Store> stores = await _unitOfWork.StoreLinks.GetStoresByPriceGroupIds(priceGroupIds);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<IEnumerable<Store>> GetStoresByRegionIds(IEnumerable<string> regionIds)
  {
    IEnumerable<Store> stores = await _unitOfWork.StoreLinks.GetStoresByRegionIds(regionIds);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<IEnumerable<StoresWithRegionAndPriceGroup>?> GetStoresWithRegionAndPriceGroup()
  {
    IEnumerable<StoresWithRegionAndPriceGroup>? storesWithRegionAndPriceGroups = await _unitOfWork.Store.GetStoresWithRegionAndPriceGroup();
    _unitOfWork.Commit();
    return storesWithRegionAndPriceGroups;
  }

  public async Task<IEnumerable<Store>> GetAllStoresListAsync()
  {
    var allStoresList = await _unitOfWork.Store.GetAllStoresAsync();
    _unitOfWork.Commit();
    return allStoresList;
  }

  public async Task<Store> GetStoreByStoreId(string storeId)
  {
    Store store = await _unitOfWork.Store.GetStoreByStoreIdAsync(storeId);
    _unitOfWork.Commit();
    return store;
  }

  public async Task<bool> AddStore(Store store, string? copyStoreId)
  {
    await _unitOfWork.Store.AddStoreAsync(store);

    string[] tables = StoreTables.Tables;
    string sourceStore = (string.IsNullOrWhiteSpace(copyStoreId) ? "ALL" : copyStoreId);
    IList<Task> tasks = new List<Task>();

    foreach (string tableName in tables)
    {
      StringBuilder sourceFields = new StringBuilder();
      StringBuilder destFields = new StringBuilder();
      IList<string> columns = await _unitOfWork.Store.GetTableColumnsAsync(tableName);

      for (int i = 0; i < columns.Count; i++)
      {
        sourceFields.Append((sourceFields.Length == 0 ? "" : ", ") + columns[i]);

        if (columns[i].ToString().ToUpper() == "STOREID")
        {
          destFields.Append((destFields.Length == 0 ? "" : ", ") + "'" + store.Store_ID + "' AS StoreID");
        }
        else
        {
          destFields.Append((destFields.Length == 0 ? "" : ", ") + columns[i]);
        }
      }

      tasks.Add(_unitOfWork.Store.InsertIntoTablesAsync(tableName, sourceFields.ToString(), destFields.ToString(), sourceStore));
    }

    await Task.WhenAll(tasks);

    bool isOk = _unitOfWork.Commit();
    return isOk;
  }

  public async Task<bool> UpdateStore(Store store)
  {
    bool isUpdated = await _unitOfWork.Store.UpdateStoreAsync(store);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteStore(string storeId)
  {
    await _unitOfWork.Store.DeleteStoreAsync(storeId, "store");
    string[] tables = StoreTables.Tables;

    foreach (string tableName in tables)
    {
      await _unitOfWork.Store.DeleteStoreAsync(storeId, tableName);
    }

    await _unitOfWork.Store.DeleteStoreAsync(storeId, "StoreLink");
    await _unitOfWork.Store.DeleteStoreAsync(storeId, "VendorLink");

    bool isDeleted = _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<IList<Store>?> GetStoreBySelectedRegion(string regionId)
  {
    IList<Store>? stores = await _unitOfWork.Store.GetStoreBySelectedRegionAsync(regionId);
    _unitOfWork.Commit();
    return stores;
  }
}
