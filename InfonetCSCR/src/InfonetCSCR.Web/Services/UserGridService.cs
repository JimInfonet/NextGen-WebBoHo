namespace InfonetCSCR.Web.Services;

public class UserGridService : IUserGridService
{
  private readonly IPolicyService _policy;
  private readonly IHttpService _http;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public UserGridService(IPolicyService policy, IHttpService http,
    IHttpContextAccessor httpContextAccessor)
  {
    _policy = policy;
    _http = http;
    _httpContextAccessor = httpContextAccessor;
  }

  private CustomerPoliciesViewModel GetPolicies()
  {
    var policies = new CustomerPoliciesViewModel
    {
      TimeSheets = _policy.GetPolicy(PolicyName.TIMESHEETS),
      HaveUserId = _policy.GetPolicy(PolicyName.HAVE_USERID),
      UCHGPass = _policy.GetPolicy(PolicyName.U_CHGPASS)
    };

    return policies;
  }

  public bool GetTimeSheetsAndHaveUserIdPolicy()
  {
    var policies = GetPolicies();
    return policies.TimeSheets && policies.HaveUserId;
  }

  public async Task<IEnumerable<UserDetailsViewModel>> GetAllUserDetails()
  {
    bool timeSheetsAndHaveUserIdPolicy = GetTimeSheetsAndHaveUserIdPolicy();
    string url = $"User/GetAllUserDetails?timeSheetsAndHaveUserIdPolicy={timeSheetsAndHaveUserIdPolicy}";
    IEnumerable<UserDetailsViewModel> userDetails = await _http.GetAndParseAsync<IEnumerable<UserDetailsViewModel>>(url);
    return userDetails;
  }

  public async Task<IEnumerable<GroupInfoViewModel>?> GetGroupInfo(string userCode)
  {
    string url = $"User/GetGroupInfo?userCode={userCode}";
    IEnumerable<GroupInfoViewModel>? groupInfo = await _http.GetAndParseAsync<IEnumerable<GroupInfoViewModel>>(url);
    return groupInfo;
  }

  public async Task<IEnumerable<string>?> GetGroupCodes()
  {
    string url = $"User/GetGroupCodesBySecurityLevel?securityLevel={GetSecurityLevelForLoggedInUser()}";
    IEnumerable<string>? groupCodes = await _http.GetAndParseAsync<IEnumerable<string>>(url);
    return groupCodes;
  }

  public async Task UpdateGroupInfo(string user, string? group)
  {
    UigViewModel uig = new UigViewModel()
    {
      User = user,
      Group = group
    };
    string url = $"User/AddUIGRecord";
    await _http.PostAsync(url, uig);
  }
  
  public async Task<bool> DeleteUIGRecord(string user, string? group)
  {
    UigViewModel uig = new UigViewModel()
    {
      User = user,
      Group = group
    };
    string url = $"User/DeleteUIGRecord?securityLevelForLoggedInUser={GetSecurityLevelForLoggedInUser()}";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, uig);
    return isDeleted;
  }
  
  public int GetSecurityLevelForLoggedInUser()
  {
    UserDetailsViewModel user = GetLoggedInUser();
    return user.UserGroup?.SecurityLevel ?? 0;
  }
  
  public UserDetailsViewModel GetLoggedInUser()
  {
    string userJson = _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.UserDetails)!;
    UserDetailsViewModel user = JsonSerializer.Deserialize<UserDetailsViewModel>(userJson)!;
    return user;
  }

  public bool GetUCHGPassPolicy()
  {
    var policies = GetPolicies();
    return policies.UCHGPass;
  }

  public async Task<IEnumerable<string>?> GetUserCodes()
  {
    string url = $"User/GetUserCodes";
    IEnumerable<string>? userCodes = await _http.PostAndParseAsync<IEnumerable<string>?>(url, GetLoggedInUser());
    return userCodes;
  }
  
  public async Task ChangeUserPassword(UserDetailsViewModel userDetailsViewModel)
  {
    string url = $"User/ChangeUserPassword";
    await _http.PostAsync(url, userDetailsViewModel);
  }

  public async Task<int> GetMaxUserIDAsync()
  {
    const string url = "User/GetMaxUserID";
    return await _http.GetAndParseAsync<int>(url);
  }
}
