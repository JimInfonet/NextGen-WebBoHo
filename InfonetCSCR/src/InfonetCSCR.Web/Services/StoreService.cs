namespace InfonetCSCR.Web.Services;

public class StoreService : IStoreService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policy;

  public StoreService(IHttpService http, IPolicyService policy)
  {
    _http = http;
    _policy = policy;
  }

  public bool GetPolicies()
  {
    return (_policy.GetPolicy(PolicyName.SUPPORT_WAREHOUSE));
  }

  public async Task<IList<StoreViewModel>?> GetAllStoresList()
  {
    string url = "Store/GetAllStoresList";
    IEnumerable<StoreViewModel>? stores = await _http.GetAndParseAsync<IList<StoreViewModel>>(url);
    return (IList<StoreViewModel>?)stores;
  }
  
  public async Task<StoreViewModel> GetStoreByStoreId(string storeId)
  {
    string url = $"Store/GetStoreByStoreId?storeId={storeId}";
    StoreViewModel store = await _http.GetAndParseAsync<StoreViewModel>(url);
    return store;
  }

  public async Task<bool> AddStore(StoreViewModel store)
  {
    string url = $"Store/AddStore?copyStoreId={store.CopyStoreId}";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, store);
    return isAdded; 
  }
  
  public async Task<bool> UpdateStore(StoreViewModel store)
  {
    string url = $"Store/UpdateStore";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, store);
    return isUpdated; 
  }
  
  public async Task<bool> DeleteStore(string storeId)
  {
    string url = $"Store/DeleteStore";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, storeId);
    return isDeleted; 
  }

  public async Task<IList<StoreViewModel>?> GetStoreBySelectedRegion(string regionId)
  {
    string url = $"Store/GetStoreBySelectedRegion?regionId={regionId}";
    IList<StoreViewModel>? stores = await _http.GetAndParseAsync<IList<StoreViewModel>?>(url);
    return stores;
  }
}
