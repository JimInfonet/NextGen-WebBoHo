namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IUserGroupRepository
{
  Task<IEnumerable<string>> GetGroupCodesBySecurityLevelAsync(int securityLevel);
  Task<int> GetSecurityLevelForDeleteUIGAsync(string group);
  Task<IEnumerable<UserGroup>?> GetAllUserGroups();
  Task<UserGroup?> GetUserGroupAsync(string groupCode);
  Task<bool> AddUserGroupAsync(UserGroup userGroup);
  Task<bool> RemoveUserGroupAsync(string groupCode);
}
