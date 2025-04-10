namespace InfonetCSCR.Services;

public class AuthService : IAuthService
{
  private readonly IUserService _userService;
  private readonly ITokenService _tokenService;
  private readonly IEncryptionService _encryptionService;

  public AuthService(IUserService userService, IEncryptionService encryptionService, ITokenService tokenService)
  {
    _encryptionService = encryptionService;
    _userService = userService;
    _tokenService = tokenService;
  }

  public async Task<bool> DeleteRefreshToken(string userCode)
  {
    return await _tokenService.DeleteRefreshToken(userCode);
  }

  public async Task<string> GetUserNameByCode(string code)
  {
    string userName = await _userService.GetUserNameByCode(code);
    return userName;
  }

  public async Task<bool> StoreRefreshToken(RefreshToken token)
  {
    return await _tokenService.StoreRefreshToken(token);
  }

  public async Task<bool> ValidateUser(User loginUser)
  {
    var user = await _userService.GetUserByCode(loginUser.Code);

    if (user == null)
      return false;

    return loginUser.Password == _encryptionService.DecryptText(user.Password);
  }
}
