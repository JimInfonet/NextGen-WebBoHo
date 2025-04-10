namespace InfonetCSCR.Data.Repositories;

public class GradeRepository : BaseRepository, IGradeRepository
{
  public GradeRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<Grade>> GetGradeByStoreIdsAsync(IList<string> storeIds)
  {
    string sql = $@"SELECT
                        [Store_ID] StoreId,
                        [ID] AS Id,
                        [ShortName] AS ShortName,
                        [FullName] AS FullName,
                        [Stock_Code] AS StockCode,
                        [SubDept] AS SubDept,
                        [Ratio] AS Ratio,
                        [FeedStock] AS FeedStock,
                        [Price_diff] AS Price_diff,
                        [StockGradeID] AS StockGradeID,
                        [FuelType] AS FuelType
                    FROM [CSCPump].[dbo].[Grade]
                    WHERE Store_ID IN @StoreIds;";

    var parameters = new { StoreIds = storeIds };
    IList<Grade> grades = (IList<Grade>)await Connection.QueryAsync<Grade>(sql, parameters, Transaction);
    return grades;
  }

  public async Task<IList<GradeTierServiceLevelTableData>> GetDataFromGradeTierServiceLevelTableAsync(string storeId)
  {
    string sql = $@"SELECT DISTINCT 
                        Grade.ID GradeId, 
                        Grade.Stock_Code StockCode, 
                        Grade.FullName, 
                        Grade.ShortName, 
                        Tier.ID TierId, 
                        Tier.Description TierDescription, 
                        ServiceLevel.Description ServiceLevelDescription, 
                        ServiceLevel.ID AS ServiceLevelId, 
                        Grade.FuelType 
                    FROM 
                        [CSCPump].[dbo].[Grade] 
                        CROSS JOIN [CSCPump].[dbo].[Tier] 
                        CROSS JOIN [CSCPump].[dbo].[ServiceLevel] 
                    WHERE 
                        Grade.Store_ID = @StoreId 
                        AND Grade.Store_ID = Tier.Store_ID 
                        AND Grade.Store_ID = ServiceLevel.Store_ID 
                        AND Grade.FuelType <> 'O' 
                    ORDER BY 
                        Grade.ID, 
                        Tier.ID, 
                        ServiceLevel.ID;";

    var parameters = new { StoreId = storeId };
    IList<GradeTierServiceLevelTableData> gradeTierServiceLevels = (IList<GradeTierServiceLevelTableData>)await Connection.QueryAsync<GradeTierServiceLevelTableData>(sql, parameters, Transaction);
    return gradeTierServiceLevels;
  }

  public async Task<IList<Grade>?> GetGradeByJoiningGradeSaleLineSaleHeadTables(DateTime shiftDate, string? department)
  {
    const string sql = @"SELECT 
                            GR.[ID] Id, 
                            GR.Stock_Code StockCode 
                        FROM 
                            [CSCPump].[dbo].[Grade] GR  
                            INNER JOIN CSCTrans.dbo.SALELINE SL ON SL.[GradeID] = GR.[ID]  
                            INNER JOIN CSCTrans.dbo.SALEHEAD SH ON SH.SALE_NO = SL.SALE_NO 
                        WHERE 
                            SH.ShiftDate = @ShiftDate  
                            AND SL.DEPT = @Department  
                        GROUP BY 
                            GR.[ID], 
                            GR.Stock_Code;";

    var parameters = new { ShiftDate = shiftDate, Department = department };
    IList<Grade>? grades = (IList<Grade>?)await Connection.QueryAsync<Grade?>(sql, parameters, Transaction);
    return grades;
  }

  public async Task<IList<string>?> GetUniqueStockCodesAsync()
  {
    const string sql = @$"SELECT DISTINCT 
                              stock_code as stock 
                          FROM
                              [CSCPump].[dbo].[Grade] 
                          WHERE 
                              FuelType <> '0'                     -- hard coded in VB
                          ORDER BY 
                              stock_code";

    IList<string>? stockCodes = (IList<string>?)await Connection.QueryAsync<string?>(sql, null, Transaction);
    return stockCodes;
  }
  
  public async Task<IList<Grade>?> GetGradesByFuelTypeAsync()
  {
    const string sql = @$"SELECT 
                            FullName, 
                            Stock_Code AS StockCode,
                            FuelType
                          FROM 
                            [CSCPump].[dbo].[Grade]
                          WHERE 
                            (FuelType = 'G') OR (FuelType = 'D')      -- hard coded in VB
                          ORDER BY 
                            FuelType DESC, StockGradeID";

    IList<Grade>? grades = (IList<Grade>?)await Connection.QueryAsync<Grade?>(sql, null, Transaction);
    return grades;
  }
}
