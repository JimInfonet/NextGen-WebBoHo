namespace InfonetCSCR.Data.Repositories;

public class RestrictionsRepository : BaseRepository, IRestrictionsRepository
{
  public RestrictionsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<Restriction>> GetRestrictionsAsync()
  {
    string sql = @$"SELECT 
                            Code,
                            Restriction AS Description,
                            AgeLimit
                        FROM 
                            [CSCMaster].[dbo].[Restrictions]";

    IList<Restriction> restrictions = (IList<Restriction>)await Connection.QueryAsync<Restriction>(sql, null, Transaction);
    return restrictions;
  }

  public async Task<bool> RestrictionAlreadyExistsAsync(int code)
  {
    string sql = @$"SELECT COUNT(1)
                        FROM [CSCMaster].[dbo].[Restrictions]
                        WHERE Code = @code";
    int count = await Connection.ExecuteScalarAsync<int>(sql, new { Code = code }, Transaction);
    return count > 0;
  }

  public async Task<bool> AddRestrictionAsync(Restriction restriction)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[Restrictions]
            (
                [Code],
                [Restriction],
                [AgeLimit]
            )
            VALUES
            (
                @Code,
                @Description,
                @AgeLimit
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, restriction, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateRestrictionAsync(Restriction restriction)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[Restrictions]
                      SET
                          Restriction = @Description,
                          AgeLimit = @AgeLimit
                      WHERE
                          Code = @Code";

    int rowsAffected = await Connection.ExecuteAsync(sql, restriction, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteRestrictionAsync(int code)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Restrictions]
                        WHERE Code = @Code";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { Code = code }, Transaction);
    return rowsAffected > 0;
  }
}