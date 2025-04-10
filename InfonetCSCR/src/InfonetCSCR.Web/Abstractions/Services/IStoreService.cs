namespace InfonetCSCR.Web.Abstractions.Services;

public interface IStoreService
{
  bool GetPolicies();
  Task<IList<StoreViewModel>?> GetAllStoresList();
  Task<StoreViewModel> GetStoreByStoreId(string storeId);
  Task<bool> AddStore(StoreViewModel store);
  Task<bool> UpdateStore(StoreViewModel store);
  Task<bool> DeleteStore(string storeId);
  Task<IList<StoreViewModel>?> GetStoreBySelectedRegion(string regionId);
}
