namespace InfonetCSCR.Web.Services;

public class TillsService : ITillsService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policy;

  public TillsService(IHttpService http, IPolicyService policy)
  {
    _http = http;
    _policy = policy;
  }

  public TillsPoliciesViewModel GetPolicies()
  {
    var policies = new TillsPoliciesViewModel
    {
      CashBonus = _policy.GetPolicy(PolicyName.CASHBONUS), 
      UseShifts = _policy.GetPolicy(PolicyName.USE_SHIFTS)
    };

    return policies;
  }

  public async Task<String> GetPumpFeatures()
  {
    string url = $"Tills/GetPumpFeatures";
    string pumpFeatures = await _http.GetAndParseAsync<string>(url);
    return pumpFeatures;
  }
  
  public async Task<int> GetMaxTill(int tillNo)
  {
    string url = $"Tills/GetMaxTill?tillNo={tillNo}";
    int mTill = await _http.GetAndParseAsync<int>(url);
    return mTill;
  }

  public async Task<IList<TillViewModel>?> GetTills(int maxTill)
  {
    bool isCashBonusPolicy = _policy.GetPolicy(PolicyName.CASHBONUS);
    string url = $"Tills/GetTills?maxTill={maxTill}&cashBonusPolicy={isCashBonusPolicy}";
    IList<TillViewModel>? tills = await _http.GetAndParseAsync<IList<TillViewModel>>(url);
    return tills;
  }

  public async Task<bool> AddTill(TillViewModel till)
  {
    string url = $"Tills/AddTill";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, till);
    return isAdded;
  }

  public async Task<int> GetTillsCount(int tillNo)
  {
    string url = $"Tills/GetTillsCount?tillNo={tillNo}";
    int tillCount = await _http.GetAndParseAsync<int>(url);
    return tillCount;
  }

  public async Task<bool> DeleteTill(int tillNo)
  {
    string url = $"Tills/DeleteTill";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, tillNo);
    return isDeleted;
  }

  public async Task<bool> UpdateTillByFlipActiveOrFlipProcess(TillViewModel till)
  {
    string url = $"Tills/UpdateTillByFlipActiveOrFlipProcess";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, till);
    return isUpdated;
  }
}
