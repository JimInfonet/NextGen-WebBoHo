namespace InfonetCSCR.Web.Services;

public class GroupService : IGroupService
{
  private readonly IHttpService _http;

  public GroupService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<FamilyGroupJoinFamilyViewModel>?> GetAllGroups()
  {
    string url = "Group/GetAllGroups";
    IList<FamilyGroupJoinFamilyViewModel>? groups = await _http.GetAndParseAsync<IList<FamilyGroupJoinFamilyViewModel>>(url);
    return groups;
  }
  
  public async Task<bool> AddGroup(FamilyGroupRecordViewModel group)
  {
    string url = "Group/AddGroup";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, group);
    return isAdded;
  }

  public async Task<bool> DeleteGroup(FamilyGroupJoinFamilyViewModel group)
  {
    string url = $"Group/DeleteGroup";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, group);
    return isDeleted;
  }

  public async Task<IList<FamilyGroupViewModel>?> GetAllFamilyGroups()
  {
    string url = "Group/GetAllFamilyGroups";
    IList<FamilyGroupViewModel>? groups = await _http.GetAndParseAsync<IList<FamilyGroupViewModel>>(url);
    return groups;
  }
}
