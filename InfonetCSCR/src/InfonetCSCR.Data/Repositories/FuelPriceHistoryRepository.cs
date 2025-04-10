namespace InfonetCSCR.Data.Repositories;
public class FuelPriceHistoryRepository : BaseRepository, IFuelPriceHistoryRepository
{
  public FuelPriceHistoryRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<FuelPriceHistory>> GetFuelPriceHistoryByGradeId(IList<int?> gradeIds)
  {
    const string sql = @"SELECT DISTINCT 
                              Date_Time DateTime,
                              CashPrice,
                              GradeID
                          FROM 
                              [CSCPump].[dbo].[FuelPriceHist] 
                          WHERE 
                              GradeID IN @GrandIds 
                          ORDER BY 
                              Date_Time DESC;";

    var parameters = new { GrandIds = gradeIds };
    IList<FuelPriceHistory> fuelPriceHistories = (IList<FuelPriceHistory>)await Connection.QueryAsync<FuelPriceHistory>(sql, parameters, Transaction);
    return fuelPriceHistories;
  }

  public async Task<IList<FuelPriceHistory>> GetAllFuelPriceHistoryByStoreIdAndReportId(string storeId, int reportId)
  {
    const string sql = @"SELECT
                              Store_ID StoreID,
                              ReportID,
                              GradeID,
                              TierID,
                              LevelID,
                              CashPrice,
                              CreditPrice, 
                              Date_Time DateTime  
                          FROM 
                              [CSCPump].[dbo].[FuelPriceHist]
                          WHERE Store_ID=@StoreId AND ReportID=@ReportId;";

    var parameters = new { StoreId = storeId, ReportId = reportId };
    IList<FuelPriceHistory> fuelPriceHistories = (IList<FuelPriceHistory>)await Connection.QueryAsync<FuelPriceHistory>(sql, parameters, Transaction);
    return fuelPriceHistories;
  }

  public async Task<int> GetMaximumReportIdByStoreId(string storeId)
  {
    const string sql = @"SELECT MAX(ReportID) As ReportID FROM [CSCPump].[dbo].[FuelPriceHist] WHERE Store_ID=@StoreId;";

    var parameters = new { StoreId = storeId };
    int maxReportID = await Connection.QueryFirstOrDefaultAsync<int>(sql, parameters, Transaction);
    return maxReportID;
  }
}
