namespace InfonetCSCR.Core.Abstractions.Services;

public interface IUserGroupService
{
  Task<IEnumerable<string>> GetGroupCodesBySecurityLevel(int securityLevel);
  Task<int> GetSecurityLevelForDeleteUIG(string group);
  Task<IEnumerable<UserGroup>?> GetAllUserGroups();
  Task<UserGroup?> GetUserGroup(string groupCode);
  Task<bool> AddUserGroup(UserGroup userGroup);
  Task<bool> RemoveUserGroup(string groupCode);
  Task<IEnumerable<SecurityLevel>?> GetSecurityLevels(UserDetails userDetails);
  Task<IEnumerable<UserDetails>?> GetAllUsersByGroup(string groupCode);
}
