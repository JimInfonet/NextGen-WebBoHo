namespace InfonetCSCR.Services;

public class UserGroupService : IUserGroupService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ISecurityLevelService _securityLevelService;

  public UserGroupService(IApplicationConfig applicationConfig, 
    ISecurityLevelService securityLevelService)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCAdminConnectionString);
    _securityLevelService = securityLevelService;
  }

  public async Task<IEnumerable<string>> GetGroupCodesBySecurityLevel(int securityLevel)
  {
    IEnumerable<string> groupInfo = await _unitOfWork.UserGroup.GetGroupCodesBySecurityLevelAsync(securityLevel);
    _unitOfWork.Commit();

    return groupInfo;
  }
  
  public async Task<int> GetSecurityLevelForDeleteUIG(string group)
  {
    int securityLevel = await _unitOfWork.UserGroup.GetSecurityLevelForDeleteUIGAsync(group);
    _unitOfWork.Commit();

    return securityLevel;
  }

  public async Task<IEnumerable<UserGroup>?> GetAllUserGroups()
  {
    IEnumerable<UserGroup>? userGroups = await _unitOfWork.UserGroup.GetAllUserGroups();
    _unitOfWork.Commit();

    return userGroups;
  }

  public async Task<UserGroup?> GetUserGroup(string groupCode)
  {
    UserGroup? userGroup = await _unitOfWork.UserGroup.GetUserGroupAsync(groupCode);
    _unitOfWork.Commit();

    return userGroup;
  }

  public async Task<bool> AddUserGroup(UserGroup userGroup)
  {
    bool isAdded = await _unitOfWork.UserGroup.AddUserGroupAsync(userGroup);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> RemoveUserGroup(string groupCode)
  {
    bool isRemoved = await _unitOfWork.UserGroup.RemoveUserGroupAsync(groupCode);
    _unitOfWork.Commit();

    return isRemoved;
  }

  public async Task<IEnumerable<SecurityLevel>?> GetSecurityLevels(UserDetails userDetails)
  {
    int loggedInUserSecurityLevel = userDetails.UserGroup != null ? 
      await _unitOfWork.UserGroup.GetSecurityLevelForDeleteUIGAsync(userDetails.UserGroup.Code) : 0;
    IEnumerable<SecurityLevel>? securityLevels = await _securityLevelService.GetSecurityLevels(loggedInUserSecurityLevel);
    _unitOfWork.Commit();

    return securityLevels;
  }

  public async Task<IEnumerable<UserDetails>?> GetAllUsersByGroup(string groupCode)
  {
    IEnumerable<UserDetails>? groupMembers = await _unitOfWork.Users.GetAllUsersByGroupAsync(groupCode);
    _unitOfWork.Commit();

    return groupMembers;
  }
}
