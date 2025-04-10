namespace InfonetCSCR.Data.Repositories;

public class DiscountChartRepository : BaseRepository, IDiscountChartRepository
{
  public DiscountChartRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<DiscountChart>> GetDiscountChartDataAsync(string? groupID, bool isUseARInvoice)
  {
    var discountCharts = await Connection.QueryAsync<DiscountChart>
        ("Sproc_LoadDiscountChart", new 
                                    { 
                                      GroupID = groupID, 
                                      USE_ARINVOICES = isUseARInvoice 
                                    }, 
        Transaction, commandType: CommandType.StoredProcedure);
    return discountCharts;
  }

  public async Task<bool> DeleteDiscountChartDataAsync(string groupID)
  {
    string query = $@"DELETE FROM 
                            [CSCMaster].[dbo].[FuelDiscountChart] 
                            WHERE [GroupID] = @GroupID;";

    var rowAffected = await Connection.ExecuteAsync(query, new { GroupID = groupID }, Transaction);
    return rowAffected > 0;
  }

  public async Task<bool> SaveDiscountChartDataAsync(DiscountChart discountChart)
  {
    var rowAffected = await Connection.ExecuteAsync
        ("Sproc_UpdateOrInsertDiscountChart", new 
                                              { 
                                                GroupID = discountChart.GroupID,
                                                Grade = discountChart.Grade,
                                                DiscountRate = discountChart.DiscountRate 
                                              }, 
        Transaction, commandType: CommandType.StoredProcedure);

    return rowAffected > 0;
  }
}
