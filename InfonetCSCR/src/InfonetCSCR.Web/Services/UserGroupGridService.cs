namespace InfonetCSCR.Web.Services;

public class UserGroupGridService : IUserGroupGridService
{
  private readonly IHttpService _http;

  public UserGroupGridService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IEnumerable<UserGroupViewModel>?> GetAllUserGroups()
  {
    string url = $"UserGroup/GetAllUserGroups";
    IEnumerable<UserGroupViewModel>? userDetails = await _http.GetAndParseAsync<IEnumerable<UserGroupViewModel>?>(url);
    return userDetails;
  }

  public async Task<bool> RemoveUserGroup(string groupCode)
  {
    string url = "UserGroup/RemoveUserGroup";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, groupCode);
    return isDeleted;
  }

  public async Task<IEnumerable<UserDetailsViewModel>?> GetAllUsersByGroup(string groupCode)
  {
    string url = $"UserGroup/GetAllUsersByGroup?groupCode={groupCode}";
    IEnumerable<UserDetailsViewModel>? groupMembers = await _http.GetAndParseAsync<IEnumerable<UserDetailsViewModel>?>(url);

    return groupMembers;
  }
}
