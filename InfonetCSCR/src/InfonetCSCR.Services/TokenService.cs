namespace InfonetCSCR.Services;

public class TokenService : ITokenService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _applicationConfig;

  public TokenService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCAdminConnectionString);
    _applicationConfig = applicationConfig;
  }

  public async Task<bool> DeleteRefreshToken(string userCode)
  {
    bool result = await _unitOfWork.Token.DeleteRefreshTokenAsync(userCode);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> IsValidRefreshToken(string? refreshToken, string userCode)
  {
    bool result = false;
    using (var uow = new UnitOfWork(_applicationConfig, _applicationConfig.CSCAdminConnectionString))
    {
      result = await uow.Token.IsValidRefreshTokenAsync(refreshToken, userCode);
      uow.Commit();
    }
    return result;
  }

  public async Task<bool> StoreRefreshToken(RefreshToken token)
  {
    bool result = false;
    using (var uow = new UnitOfWork(_applicationConfig, _applicationConfig.CSCAdminConnectionString))
    {
      result = await uow.Token.StoreRefreshTokenAsync(token);
      uow.Commit();
    }
    return result;
  }

  public async Task<TokenModel> RefreshToken(TokenModel tokenModel, string userCode, string issuer, string audience, byte[] key, int jwtExpireTimeInMinute, int refreshTokenLifetimeInDays)
  {
    try
    {
      string? jwtToken = tokenModel.JwtToken;
      string? refreshToken = tokenModel.RefreshToken;

      var principal = AuthenticationHelper.GetPrincipalFromExpiredToken(jwtToken, issuer, audience, key);

      bool isValidRefreshToken = await IsValidRefreshToken(refreshToken, userCode);
      if (!isValidRefreshToken) throw new Exception("Invalid access token or refresh token");

      var newJwtToken = AuthenticationHelper.CreateToken(userCode, issuer, audience, key, jwtExpireTimeInMinute);
      var newRefreshToken = AuthenticationHelper.GenerateRefreshToken();

      RefreshToken token = new RefreshToken()
      {
        Token = newRefreshToken,
        ExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenLifetimeInDays)),
        UserCode = userCode,
      };

      bool isTokenStored = await StoreRefreshToken(token);
      if (!isTokenStored) throw new Exception("Failed to store refresh token");

      return (new TokenModel
      {
        JwtToken = newJwtToken,
        RefreshToken = newRefreshToken
      });
    }
    catch
    {
      throw new Exception("Failed to refresh token.");
    }
  }
}
