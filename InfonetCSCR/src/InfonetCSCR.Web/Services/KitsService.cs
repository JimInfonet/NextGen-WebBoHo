namespace InfonetCSCR.Web.Services;

public class KitsService : IKitsService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policy;

  public KitsService(IHttpService http, IPolicyService policy)
  {
    _http = http;
    _policy = policy;
  }

  public async Task<IEnumerable<KitsViewModel>?> GetKits()
  {
    string url = $"Kits/GetKits";
    var kits = await _http.GetAndParseAsync<IEnumerable<KitsViewModel>?>(url);
    return kits;
  }

  public KitPoliciesViewModel GetKitPolicies()
  {
    var policies = new KitPoliciesViewModel
    {
      SellInactive = _policy.GetPolicy(PolicyName.SELL_INACT),
    };

    return policies;
  }

  public async Task<IEnumerable<KitItemsViewModel>?> GetKitItemsByKitCodeWithDesc(string stockCode)
  {
    string url = $@"Kits/GetKitItemsByKitCodeWithDesc?stockCode={stockCode}";
    var kitItems = await _http.GetAndParseAsync<IEnumerable<KitItemsViewModel>?>(url);
    return kitItems;
  }

  public async Task<IEnumerable<KitItemsViewModel>?> GetKitItems(bool isSellInactive)
  {
    string url = $@"Kits/GetKitItems?isSellInactive={isSellInactive}";
    var kitItems = await _http.GetAndParseAsync<IEnumerable<KitItemsViewModel>?>(url);
    return kitItems;
  }

  public async Task<bool> AddKitItem(KitItemsViewModel kitItem)
  {
    string url = $@"Kits/AddKitItem";
    var isKitAddSuccess = await _http.PostAndParseAsync<bool>(url, kitItem);
    return isKitAddSuccess;
  }

  public async Task<bool> IsKitItem(string stockCode)
  {
    string url = $@"Kits/IsKitItem?stockCode={stockCode}";
    var isKitItem = await _http.GetAndParseAsync<bool>(url);
    return isKitItem;
  }

  public async Task<bool> IsItemAlreadyInKit(KitItemsViewModel kitItem)
  {
    string url = $@"Kits/IsItemAlreadyInKit";
    var isItemAlreadyInKit = await _http.PostAndParseAsync<bool>(url, kitItem);
    return isItemAlreadyInKit;
  }

  public async Task<bool> UpdateKitItem(KitItemsViewModel kitItem)
  {
    string url = $@"Kits/UpdateKitItem";
    var isKitItemUpdated = await _http.PostAndParseAsync<bool>(url, kitItem);
    return isKitItemUpdated;
  }

  public async Task<bool> DeleteKitItem(KitItemsViewModel kitItem)
  {
    string url = $@"Kits/DeleteKitItem";
    var isKitItemDeleted = await _http.PostAndParseAsync<bool>(url, kitItem);
    return isKitItemDeleted;
  }
}
