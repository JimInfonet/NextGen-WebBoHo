namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IUserRepository
{
  Task<UserDetails> GetUserDetailsAsync(string code);
  Task<UserGroup> GetUserGroupDetailsAsync(string code);
  Task<User?> GetByCodeAsync(string code);
  Task<int> GetUserId(string userCode);
  Task<bool> GetUserAccessRightAsync(string permission, User user);
  Task<string> GetUserNameByCodeAsync(string code);
  Task<string> GetUserTypeByCodeAsync(string userCode);
  Task<IEnumerable<UserDetails>> GetAllUserDetailsAsync(bool timeSheetsAndHaveUserIdPolicy);
  Task<bool> AddUserDetailsAsync(UserDetails userDetails);
  Task<bool> UpdateUserDetailsAsync(UserDetails userDetails);
  Task<bool> DeleteUserDetailsAsync(string code);
  Task<IEnumerable<UserDetails>?> GetAllUsersByGroupAsync(string groupCode);
  Task<IEnumerable<string>?> GetUserCodesAsync(int securityLevel);
  Task<bool> ChangeUserPasswordAsync(UserDetails userDetails);
  Task<int> GetMaxUserIDAsync();
}
