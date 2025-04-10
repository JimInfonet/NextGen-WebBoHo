namespace InfonetCSCR.Web.Services;

public class RegionService : IRegionService
{
  private readonly IHttpService _http;

  public RegionService(IHttpService http)
  {
    _http = http;
  }

  public async Task<bool> AddRegion(RegionViewModel region)
  {
    string url = $"Region/AddRegion";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, region);
    return isAdded;
  }

  public async Task<bool> DeleteRegion(string regionId)
  {
    string url = $"Region/DeleteRegion";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, regionId);
    return isDeleted;
  }
  
  public async Task<IList<StoreViewModel>?> GetStoreToLinkBySelectedRegion(string regionId)
  {
    string url = $"Region/GetStoreToLinkBySelectedRegion?regionId={regionId}";
    IList<StoreViewModel>? stores = await _http.GetAndParseAsync<IList<StoreViewModel>?>(url);
    return stores;
  }

  public async Task<bool> AddStoreToTheRegion(StoreLinkViewModel store)
  {
    string url = $"Region/AddStoreToTheRegion";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, store);
    return isAdded;
  }

  public async Task<bool> DeleteStoreForSelectedRegion(StoreLinkViewModel store)
  {
    string url = $"Region/DeleteStoreForSelectedRegion";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, store);
    return isDeleted;
  }
}
