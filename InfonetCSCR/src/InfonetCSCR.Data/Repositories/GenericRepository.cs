namespace InfonetCSCR.Data.Repositories;

public class GenericRepository : BaseRepository, IGenericRepository
{
  public GenericRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<Generic>> GetAllGenericsAsync()
  {
    const string sql = @"SELECT 
	                        [GenericID] GenericID,
	                        [Description] Description
                        FROM [CSCMaster].[dbo].[Generic] 
                        ORDER BY [CSCMaster].[dbo].[Generic].GenericID";

    IList<Generic> generics = (IList<Generic>)await Connection.QueryAsync<Generic>(sql, null, Transaction);
    return generics;
  }

  public async Task<bool> AddGenericAsync(Generic generic)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[Generic]
            (
                [GenericID],
                [Description]
            )
            VALUES
            (
                @GenericID,
                @Description
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, generic, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateGenericAsync(Generic generic)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[Generic]
                      SET
                          Description = @Description
                      WHERE
                          GenericID = @GenericID";
    int rowsAffected = await Connection.ExecuteAsync(sql, generic, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteGenericAsync(string genericId)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Generic]
                        WHERE GenericID = @GenericID";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { GenericID = genericId }, Transaction);
    return rowsAffected > 0;
  }
}
