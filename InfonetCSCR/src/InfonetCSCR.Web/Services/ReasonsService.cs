namespace InfonetCSCR.Web.Services;

public class ReasonsService : IReasonsService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policy;

  public ReasonsService(IHttpService http, IPolicyService policy)
  {
    _http = http;
    _policy = policy;
  }

  public async Task<IList<ReasonViewModel>?> GetReasons(string reasonType)
  {
    string url = $"Reasons/GetReasons?reasonType={reasonType}";
    IList<ReasonViewModel>? reasons = await _http.GetAndParseAsync<IList<ReasonViewModel>>(url);
    return reasons;
  }

  public ReasonsPoliciesViewModel GetPolicies()
  {
    var policies = new ReasonsPoliciesViewModel
    {
      POReason = _policy.GetPolicy(PolicyName.PO_REASON),
      VOIDReason = _policy.GetPolicy(PolicyName.VOID_REASON),
      RETReason = _policy.GetPolicy(PolicyName.RET_REASON),
      DISCReason = _policy.GetPolicy(PolicyName.DISC_REASON),
      PRReason = _policy.GetPolicy(PolicyName.PR_REASON),
      OCDReason = _policy.GetPolicy(PolicyName.OCD_REASON),
      DRAWReason = _policy.GetPolicy(PolicyName.DRAW_REASON)
    };

    return policies;
  }

  public async Task<bool> ReasonAlreadyExists(string code, char type)
  {
    string url = $"Reasons/ReasonAlreadyExists";
    ReasonViewModel reason = new ReasonViewModel
    {
      Code = code,
      Type = type
    };
    var isExist = await _http.PostAndParseAsync<bool>(url, reason);
    return isExist;
  }

  public async Task<bool> AddReason(ReasonViewModel reason)
  {
    string url = $"Reasons/AddReason";
    var isReasonAddSuccess = await _http.PostAndParseAsync<bool>(url, reason);
    return isReasonAddSuccess;
  }

  public async Task<bool> UpdateReason(ReasonViewModel reason)
  {
    string url = $"Reasons/UpdateReason";
    var reasons = await _http.PostAndParseAsync<bool>(url, reason);
    return reasons;
  }

  public async Task<bool> DeleteReason(string code, char type)
  {
    string url = $"Reasons/DeleteReason";
    ReasonViewModel reason = new ReasonViewModel
    {
      Code = code,
      Type = type
    };
    var isDeleted = await _http.PostAndParseAsync<bool>(url, reason);
    return isDeleted;
  }
}
