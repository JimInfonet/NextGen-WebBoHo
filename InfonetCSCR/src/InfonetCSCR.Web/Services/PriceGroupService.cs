namespace InfonetCSCR.Web.Services;

public class PriceGroupService : IPriceGroupService
{
  private readonly IHttpService _http;

  public PriceGroupService(IHttpService http)
  {
    _http= http;
  }

  public async Task<IList<PriceGroupViewModel>?> GetAllPriceGroupsAsync()
  {
    string url = $"PriceGroup/GetAllPriceGroups";
    IList<PriceGroupViewModel>? priceGroups = await _http.GetAndParseAsync<IList<PriceGroupViewModel>?>(url);
    return priceGroups;
  }

  public async Task<bool> AddPriceGroupAsync(PriceGroupViewModel priceGroup)
  {
    string url = $"PriceGroup/AddPriceGroup";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, priceGroup);
    return isAdded;
  }

  public async Task<bool> DeletePriceGroupAsync(string priceGroupId)
  {
    string url = $"PriceGroup/DeletePriceGroup";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, priceGroupId);
    return isDeleted;
  }

  public async Task<IEnumerable<StoreViewModel>?> GetStoreBySelectedPriceGroupAsync(string priceGroupId)
  {
    string url = $"PriceGroup/GetStoreBySelectedPriceGroup?priceGroupId={priceGroupId}";
    return await _http.GetAndParseAsync<IList<StoreViewModel>?>(url);
  }

  public async Task<IEnumerable<StoreViewModel>?> GetStoreToLinkBySelectedPriceGroupAsync(string priceGroupId)
  {
    string url = $"PriceGroup/GetStoreToLinkBySelectedPriceGroup?priceGroupId={priceGroupId}";
    return await _http.GetAndParseAsync<IList<StoreViewModel>?>(url);
  }

  public async Task<bool> AddStoreToThePriceGroupAsync(StoreLinkViewModel store)
  {
    string url = $"PriceGroup/AddStoreToThePriceGroup";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, store);
    return isAdded;
  }

  public async Task<bool> DeleteStoreForSelectedPriceGroupAsync(StoreLinkViewModel store)
  {
    string url = $"PriceGroup/DeleteStoreForSelectedPriceGroup";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, store);
    return isDeleted;
  }
}
