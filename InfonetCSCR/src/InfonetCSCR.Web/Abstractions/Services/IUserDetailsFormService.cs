namespace InfonetCSCR.Web.Abstractions.Services;

public interface IUserDetailsFormService
{
  bool GetTimeSheetsAndHaveUserIdPolicy();
  Task<UserDetailsViewModel?> GetUserDetails(string code);
  Task<bool> SaveUserDetails(UserDetailsViewModel userDetails, bool isUpdate);
  Task<bool> DeleteUserDetails(string code);
  Task<int> GetInitialEmployeeId();
}
