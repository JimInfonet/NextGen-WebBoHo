namespace InfonetCSCR.Core.Abstractions.Services;

public interface IAuthService
{
  public Task<string> GetUserNameByCode(string code);
  public Task<bool> StoreRefreshToken(RefreshToken token);
  Task<bool> DeleteRefreshToken(string userCode);
  public Task<bool> ValidateUser(User user);
}
