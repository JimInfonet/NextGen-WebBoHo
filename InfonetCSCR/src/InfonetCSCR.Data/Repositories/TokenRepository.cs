namespace InfonetCSCR.Data.Repositories;

public class TokenRepository : BaseRepository, ITokenRepository
{
  public TokenRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> DeleteRefreshTokenAsync(string userCode)
  {
    const string sql = @"DELETE FROM RefreshToken Where UserCode=@userCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@userCode", SqlDbType.VarChar, 16).Value = userCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> IsValidRefreshTokenAsync(string? refreshToken, string userCode)
  {
    const string sql = @"SELECT * FROM RefreshToken WHERE UserCode = @UserCode and Token=@RefreshToken and ExpiryTime >= @CurrentTime";

    var parameters = new { UserCode = userCode, RefreshToken = refreshToken, CurrentTime = DateTime.UtcNow };
    IEnumerable<RefreshToken> token = await Connection.QueryAsync<RefreshToken>(sql, parameters, Transaction);

    return (token.Count() > 0);
  }

  public async Task<bool> StoreRefreshTokenAsync(RefreshToken token)
  {
    const string sql = @"IF EXISTS (SELECT * FROM RefreshToken WHERE UserCode = @userCode)
                           BEGIN
                             UPDATE RefreshToken 
                             SET Token = @token,
	                           ExpiryTime = @expiryTime
                             WHERE UserCode = @userCode;
                           END
                         ELSE
                           BEGIN
                             INSERT INTO RefreshToken
	                           VALUES(
	                           @token,
	                           @expiryTime,
	                           @userCode)
                           END";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@token", SqlDbType.VarChar, 256).Value = token.Token;
    cmd.Parameters.Add("@expiryTime", SqlDbType.DateTime).Value = token.ExpiryTime;
    cmd.Parameters.Add("@userCode", SqlDbType.VarChar, 16).Value = token.UserCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }
}
