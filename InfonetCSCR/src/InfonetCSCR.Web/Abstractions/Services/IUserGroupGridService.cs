namespace InfonetCSCR.Web.Abstractions.Services;

public interface IUserGroupGridService
{
  Task<IEnumerable<UserGroupViewModel>?> GetAllUserGroups();
  Task<bool> RemoveUserGroup(string groupCode);
  Task<IEnumerable<UserDetailsViewModel>?> GetAllUsersByGroup(string groupCode);
}
