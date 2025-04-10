namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITokenRepository
{
  Task<bool> DeleteRefreshTokenAsync(string userCode);
  Task<bool> IsValidRefreshTokenAsync(string? refreshToken, string userCode);
  Task<bool> StoreRefreshTokenAsync(RefreshToken token);
}
