namespace InfonetCSCR.Web.Services;

public class UserDetailsFormService : IUserDetailsFormService
{
  private readonly IPolicyService _policy;
  private readonly IHttpService _http;

  public UserDetailsFormService(IPolicyService policy, IHttpService http)
  {
    _policy = policy;
    _http = http;
  }

  private CustomerPoliciesViewModel GetPolicies()
  {
    var policies = new CustomerPoliciesViewModel
    {
      TimeSheets = _policy.GetPolicy(PolicyName.TIMESHEETS),
      HaveUserId = _policy.GetPolicy(PolicyName.HAVE_USERID)
    };

    return policies;
  }

  public bool GetTimeSheetsAndHaveUserIdPolicy()
  {
    var policies = GetPolicies();
    return policies.TimeSheets && policies.HaveUserId;
  }

  public async Task<UserDetailsViewModel?> GetUserDetails(string code)
  {
    if (string.IsNullOrEmpty(code)) return null;

    string url = $"User/GetUserDetails?code={code}";
    var user = await _http.GetAndParseAsync<UserDetailsViewModel?>(url);
    return user;
  }

  public async Task<bool> SaveUserDetails(UserDetailsViewModel userDetails, bool isUpdate)
  {
    string url = (isUpdate) ? "User/UpdateUserDetails" : $"User/AddUserDetails?timeSheetsAndHaveUserIdPolicy={GetTimeSheetsAndHaveUserIdPolicy()}";
    bool isCompleted = await _http.PostAndParseAsync<bool>(url, userDetails);
    return isCompleted;
  }

  public async Task<bool> DeleteUserDetails(string code)
  {
    var userDetails = await GetUserDetails(code);
    if (userDetails == null) return false;

    string url = "User/DeleteUserDetails";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, code);
    return isDeleted;
  }

  public async Task<int> GetInitialEmployeeId()
  {
    string url = "User/GetInitialEmployeeId";
    var initialEmployeeId = await _http.GetAndParseAsync<int>(url);
    return initialEmployeeId;
  }
}
