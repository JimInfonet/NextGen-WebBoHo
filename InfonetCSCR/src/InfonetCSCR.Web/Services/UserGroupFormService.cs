namespace InfonetCSCR.Web.Services;

public class UserGroupFormService : IUserGroupFormService
{
  private readonly IHttpService _http;
  private readonly IUserGridService _userGridService;

  public UserGroupFormService(IHttpService http, 
    IUserGridService userGridService)
  {
    _http = http;
    _userGridService = userGridService;
  }

  public async Task<IEnumerable<SecurityLevelViewModel>?> GetSecurityLevels()
  {
    string url = $"UserGroup/GetSecurityLevels";
    IEnumerable<SecurityLevelViewModel>? securityLevels = await _http.PostAndParseAsync<IEnumerable<SecurityLevelViewModel>?>(url, _userGridService.GetLoggedInUser());
    return securityLevels;
  }

  public async Task<UserGroupViewModel?> GetUserGroup(string groupCode)
  {
    string url = $"UserGroup/GetUserGroup?groupCode={groupCode}";
    UserGroupViewModel? userGroup = await _http.GetAndParseAsync<UserGroupViewModel?>(url);
    return userGroup;
  }
  
  public async Task<bool> AddUserGroup(UserGroupViewModel userGroup)
  {
    string url = $"UserGroup/AddUserGroup";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, userGroup);
    return isAdded;
  }
}
