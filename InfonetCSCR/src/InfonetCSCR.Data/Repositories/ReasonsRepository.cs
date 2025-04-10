namespace InfonetCSCR.Data.Repositories;

public class ReasonsRepository : BaseRepository, IReasonsRepository
{
  public ReasonsRepository(IDbTransaction transaction) : base(transaction) { }
  public async Task<IList<Reason>> GetReasonsAsync(string reasonType)
  {
    string sql = @$"SELECT 
                            Code,
                            Reason AS Description,
                            [Type]
                        FROM 
                            [CSCMaster].[dbo].[Reasons]
                    WHERE [Type] = '{reasonType}' ";

    IList<Reason> reasons = (IList<Reason>)await Connection.QueryAsync<Reason>(sql, null, Transaction);
    return reasons;
  }

  public async Task<bool> ReasonAlreadyExistsAsync(Reason reason)
  {
    string sql = @$"SELECT COUNT(1)
                        FROM [CSCMaster].[dbo].[Reasons]
                        WHERE Code = @Code AND [Type] = @Type ";

    int count = await Connection.ExecuteScalarAsync<int>(sql, reason, Transaction);
    return count > 0;
  }

  public async Task<bool> AddReasonAsync(Reason reason)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[Reasons]
            (
                [Code],
                [Reason],
                [Type]
            )
            VALUES
            (
                @Code,
                @Description,
                @Type
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, reason, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateReasonAsync(Reason reason)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[Reasons]
                      SET
                          Reason = @Description,
                          Code = @Code
                      WHERE
                          Code = @Code AND [Type] = @Type";

    int rowsAffected = await Connection.ExecuteAsync(sql, reason, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteReasonAsync(Reason reason)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Reasons]
                  WHERE Code = @Code AND [Type] = @Type";

    int rowsAffected = await Connection.ExecuteAsync(sql, reason, Transaction);
    return rowsAffected > 0;
  }
}
