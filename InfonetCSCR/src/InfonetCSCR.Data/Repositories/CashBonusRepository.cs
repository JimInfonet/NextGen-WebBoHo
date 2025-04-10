namespace InfonetCSCR.Data.Repositories;

public class CashBonusRepository : BaseRepository, ICashBonusRepository
{
  public CashBonusRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<CashBonusChart>> GetCashBonusDataAsync(string groupID)
  {
    string query = $@"SELECT 
                            [IncrementalType], 
                            [IncrementalQty], 
                            [BonusRate], 
                            [GroupID] 
                        FROM 
                            [CSCMaster].[dbo].[CashBonusChart] 
                            WHERE [GroupID] = @GroupID 
                            ORDER BY [IncrementalType] DESC, [IncrementalQty] ASC;";

    var cashBonusData = await Connection.QueryAsync<CashBonusChart>(query, new { GroupID = groupID }, Transaction);
    return cashBonusData;
  }
  
  public async Task DeleteExistingCashBonusRecordsAsync(string groupID)
  {
    string query = $@"DELETE FROM 
                            [CSCMaster].[dbo].[CashBonusChart] 
                            WHERE [GroupID] = @GroupID;";

    await Connection.ExecuteAsync(query, new { GroupID = groupID }, Transaction);
  }

  public async Task<bool> AddCashBonusDataAsync(CashBonusChart cashBonusChart)
  {
    string query = $@"INSERT INTO [CSCMaster].[dbo].[CashBonusChart] (
                        [IncrementalType], 
                        [IncrementalQty], 
                        [BonusRate], 
                        [GroupID]
                    )
                    VALUES (
                        @IncrementalType,
                        @IncrementalQty,
                        @BonusRate,
                        @GroupID
                    );";

    int rowsAffected = await Connection.ExecuteAsync(query, cashBonusChart, Transaction);
    return rowsAffected > 0;
  }
}
