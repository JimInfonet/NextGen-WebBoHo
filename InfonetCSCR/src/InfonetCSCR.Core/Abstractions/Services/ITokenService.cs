namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITokenService
{
  Task<bool> DeleteRefreshToken(string userCode);
  Task<bool> IsValidRefreshToken(string? refreshToken, string userCode);
  Task<bool> StoreRefreshToken(RefreshToken token);
  Task<TokenModel> RefreshToken(TokenModel tokenModel, string userCode, string issuer, string audience, byte[] key, int jwtExpireTimeInMinute, int refreshTokenLifetimeInDays);
}
