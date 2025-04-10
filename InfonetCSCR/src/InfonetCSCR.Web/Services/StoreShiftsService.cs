namespace InfonetCSCR.Web.Services;

public class StoreShiftsService : IStoreShiftsService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policy;

  public StoreShiftsService(IHttpService http, IPolicyService policy)
  {
    _http = http;
    _policy = policy;
  }

  public string GetPolicies()
  {
    return (_policy.GetPolicy(PolicyName.SHIFT_DAY));
  }

  public async Task<IList<StoreShiftViewModel>?> GetStoreShifts()
  {
    string url = "StoreShifts/GetStoreShifts";
    IList<StoreShiftViewModel>? storeShifts = await _http.GetAndParseAsync<IList<StoreShiftViewModel>>(url);
    return storeShifts;
  }

  public async Task<bool> AddStoreShift(StoreShiftViewModel storeShift)
  {
    string url = $"StoreShifts/AddStoreShift";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, storeShift);
    return isAdded;
  }

  public async Task<bool> DeleteStoreShift(int shiftNumber)
  {
    string url = $"StoreShifts/DeleteStoreShift";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, shiftNumber);
    return isDeleted;
  }
  
  public async Task<bool> UpdateStoreShifts(IList<StoreShiftViewModel> storeShifts, bool isUpdateAll)
  {
    string url = $"StoreShifts/UpdateStoreShifts?isUpdateAll={isUpdateAll}";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, storeShifts);
    return isUpdated;
  }
}
