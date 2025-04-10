namespace InfonetCSCR.Web.Abstractions.Services;

public interface IUserGridService
{
  bool GetTimeSheetsAndHaveUserIdPolicy();
  Task<IEnumerable<UserDetailsViewModel>> GetAllUserDetails();
  Task<IEnumerable<GroupInfoViewModel>?> GetGroupInfo(string userCode);
  Task<IEnumerable<string>?> GetGroupCodes();
  Task UpdateGroupInfo(string user, string? group);
  Task<bool> DeleteUIGRecord(string user, string? group);
  int GetSecurityLevelForLoggedInUser();
  bool GetUCHGPassPolicy();
  UserDetailsViewModel GetLoggedInUser();
  Task<IEnumerable<string>?> GetUserCodes();
  Task ChangeUserPassword(UserDetailsViewModel userDetailsViewModel);
  Task<int> GetMaxUserIDAsync();
}
