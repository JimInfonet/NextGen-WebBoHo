namespace InfonetCSCR.Web.Abstractions.Services;

public interface IUserGroupFormService
{
  Task<IEnumerable<SecurityLevelViewModel>?> GetSecurityLevels();
  Task<UserGroupViewModel?> GetUserGroup(string groupCode);
  Task<bool> AddUserGroup(UserGroupViewModel userGroup);
}
