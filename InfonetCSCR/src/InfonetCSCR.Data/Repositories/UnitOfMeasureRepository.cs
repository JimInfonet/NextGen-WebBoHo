namespace InfonetCSCR.Data.Repositories;

public class UnitOfMeasureRepository : BaseRepository, IUnitOfMeasureRepository
{
  public UnitOfMeasureRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<UnitOfMeasure>> GetAllUnitOfMeasuresAsync()
  {
    const string sql = @"SELECT 
	                        [UMID] UnitOfMeasureID,
	                        [Description] Description
                        FROM [CSCMaster].[dbo].[UM] 
                        ORDER BY [CSCMaster].[dbo].[UM].UMID";

    IList<UnitOfMeasure> unitOfMeasures = (IList<UnitOfMeasure>)await Connection.QueryAsync<UnitOfMeasure>(sql, null, Transaction);
    return unitOfMeasures;
  }

  public async Task<bool> AddUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[UM]
            (
                [UMID],
                [Description]
            )
            VALUES
            (
                @UnitOfMeasureID,
                @Description
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, unitOfMeasure, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[UM]
                      SET
                          Description = @Description
                      WHERE
                          UMID = @UnitOfMeasureID";
    int rowsAffected = await Connection.ExecuteAsync(sql, unitOfMeasure, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteUnitOfMeasureAsync(string unitOfMeasureId)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[UM]
                        WHERE UMID = @UnitOfMeasureID";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { UnitOfMeasureID = unitOfMeasureId }, Transaction);
    return rowsAffected > 0;
  }
}
