namespace InfonetCSCR.Data.Repositories;

public class FuelPriceChangeRepository : BaseRepository, IFuelPriceChangeRepository
{
  public FuelPriceChangeRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> DeleteStoreAsync(IList<FuelPriceChangeTransferRecord> storeIds)
  {
    string sql = $@"DELETE FROM 
                      [CSCPump].[dbo].[FuelPrice] 
                    WHERE 
                      Store_ID = @StoreId";

    int rowsAffected = await Connection.ExecuteAsync(sql, storeIds, Transaction);
    return rowsAffected > 0;
  }

  public async Task<int> GetReportIdAsync(string tableName)
  {
    string sql = $@"SELECT ISNULL(MAX(ReportID), 0) FROM [CSCPump].[dbo].[{tableName}]";

    int maxReportId = await Connection.QueryFirstOrDefaultAsync<int>(sql, null, Transaction);
    return maxReportId;
  }

  public async Task<IEnumerable<FuelPriceChangeTransferRecord>> GetMaxReportIdsAsync(IEnumerable<string> storeIDs)
  {
    var query = $@"
      SELECT
        [Store_ID] AS [StoreId],
        ISNULL(MAX(ReportID), 0) AS [ReportId] 
      FROM [CSCPump].[dbo].[FuelPriceHist] 
      WHERE [Store_ID] IN @StoreIDs 
      GROUP BY [Store_ID]";

    return await Connection.QueryAsync<FuelPriceChangeTransferRecord>(query, new { StoreIDs = storeIDs }, Transaction);
  }

  public async Task<bool> AddNewFuelPricesAsync(IList<FuelPriceChangeTransferRecord> stores)
  {
    string sql = $@"INSERT INTO [CSCPump].[dbo].[FuelPrice] (
                      store_id,
                      GradeID,
                      TierID,
                      LevelID,
                      CashPrice,
                      CreditPrice,
                      EmplID,
                      ReportID,
                      Date_Time,
                      TaxFreePrice,
                      TaxFreeCreditPrice,
                      CategoryFK,
                      Source,
                      TransmissionID
                  ) VALUES (
                      @StoreId,
                      @GradeId,
                      @TierId,
                      @ServiceLevelId,
                      @CashPrice,
                      @CreditPrice,
                      @EmployeeId,
                      @ReportID,
                      @ReportDateTime,
                      @TaxFreePrice,
                      @TaxFreeCreditPrice,
                      0,            -- CategoryFK is hardcoded as 0 in VB Codebase
                      'H',           -- Source is hardcoded as 'H' in VB Codebase
                      {DateTime.Now:MMddyyyyHHmmss}
                  )";

    int rowsAffected = await Connection.ExecuteAsync(sql, stores, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> AddNewFuelPriceHistsAsync(IList<FuelPriceChangeTransferRecord> stores)
  {
    string sql = $@"INSERT INTO [CSCPump].[dbo].[FuelPriceHist] (
                      store_id,
                      GradeID,
                      TierID,
                      LevelID,
                      CashPrice,
                      CreditPrice,
                      EmplID,
                      ReportID,
                      Date_Time,
                      TaxFreePrice,
                      TaxFreeCreditPrice,
                      Source,
                      TransmissionID
                  ) VALUES (
                      @StoreId,
                      @GradeId,
                      @TierId,
                      @ServiceLevelId,
                      @CashPrice,
                      @CreditPrice,
                      @EmployeeId,
                      @ReportID,
                      @ReportDateTime,
                      @TaxFreePrice,
                      @TaxFreeCreditPrice,
                      'H',           -- Source is hardcoded as 'H' in VB codebase
                      {DateTime.Now:MMddyyyyHHmmss}
                  )";

    int rowsAffected = await Connection.ExecuteAsync(sql, stores, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IList<FuelPriceHistory>> GetAllFuelPriceHistoryByStoreIdAndReportIdAsync(IList<FuelPriceChangeTransferRecord> stores)
  {
    var storeIds = stores.Select(s => s.StoreId).Distinct().ToList();
    var reportIds = stores.Select(s => s.ReportId).Distinct().ToList();

    const string sql = @"SELECT
                              Store_ID AS StoreID,
                              ReportID,
                              GradeID,
                              TierID,
                              LevelID,
                              CashPrice,
                              CreditPrice, 
                              Date_Time DateTime  
                          FROM 
                              [CSCPump].[dbo].[FuelPriceHist]
                          WHERE Store_ID IN @StoreIds AND ReportID IN @ReportIds";

    var parameters = new { StoreIds = storeIds, ReportIds = reportIds };
    IList<FuelPriceHistory> fuelPriceHistories = (IList<FuelPriceHistory>)await Connection.QueryAsync<FuelPriceHistory>(sql, parameters, Transaction);
    return fuelPriceHistories;
  }

  public async Task<IList<GradeTierServiceLevelTableData>> GetFuelPriceChangeReportDataAsync(IList<string> storeIds)
  {
    string sql = $@"
                  SELECT DISTINCT 
                      Grade.Store_ID AS StoreId, 
                      Grade.ID AS GradeId, 
                      Grade.Stock_Code AS StockCode, 
                      Grade.FullName, 
                      Grade.ShortName, 
                      Tier.ID AS TierId, 
                      Tier.Description AS TierDescription, 
                      ServiceLevel.Description AS ServiceLevelDescription, 
                      ServiceLevel.ID AS ServiceLevelId, 
                      Grade.FuelType 
                  FROM 
                      [CSCPump].[dbo].[Grade] 
                      CROSS JOIN [CSCPump].[dbo].[Tier] 
                      CROSS JOIN [CSCPump].[dbo].[ServiceLevel] 
                  WHERE 
                      Grade.Store_ID IN @StoreIds 
                      AND Grade.Store_ID = Tier.Store_ID 
                      AND Grade.Store_ID = ServiceLevel.Store_ID 
                      AND Grade.FuelType <> 'O' 
                  ORDER BY 
                      Grade.ID, 
                      Tier.ID, 
                      ServiceLevel.ID";

    var parameters = new { StoreIds = storeIds };
    IList<GradeTierServiceLevelTableData> gradeTierServiceLevels = (IList<GradeTierServiceLevelTableData>)await Connection.QueryAsync<GradeTierServiceLevelTableData>(sql, parameters, Transaction);
    return gradeTierServiceLevels;
  }
}
