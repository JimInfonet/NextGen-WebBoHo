namespace InfonetCSCR.Core.Abstractions.Services;

public interface IUserService
{
  Task<UserDetails> GetUserDetails(string userCode, bool shouldIncludeUserGroup = true);
  Task<int> GetUserId(string userCode);
  Task<User?> GetUserByCode(string code);
  Task<string> GetUserNameByCode(string code);
  Task<bool> HasAccessRight(string permission, User user);
  Task<IEnumerable<UserDetails>> GetAllUserDetails(bool timeSheetsAndHaveUserIdPolicy);
  Task<bool> AddUserDetails(UserDetails userDetails, bool timeSheetsAndHaveUserIdPolicy);
  Task<bool> UpdateUserDetails(UserDetails userDetails);
  Task<bool> DeleteUserDetails(string groupID);
  Task<int> GetInitialEmployeeId();
  Task<IEnumerable<GroupInfoDto>?> GetGroupInfo(string userCode);
  Task<IEnumerable<string>> GetGroupCodesBySecurityLevel(int securityLevel);
  Task<bool> AddUIGRecord(Uig uig);
  Task<bool> DeleteUIGRecord(Uig uig, int securityLevelForLoggedInUser);
  Task<IEnumerable<string>?> GetUserCodes(UserDetails userDetails);
  Task ChangeUserPassword(UserDetails userDetails);
  Task<int> GetMaxUserIDAsync();
}
