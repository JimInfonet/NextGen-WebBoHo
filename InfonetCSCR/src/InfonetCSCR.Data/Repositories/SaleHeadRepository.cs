namespace InfonetCSCR.Data.Repositories;

public class SaleHeadRepository : BaseRepository, ISaleHeadRepository
{
  public SaleHeadRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<SaleHeadAndSaleLine>?> GetSaleData(DateTime shiftDate, string? fuelDept)
  {
    string sql = $@"SELECT SALEHEAD.ShiftDate
                    FROM [CSCTrans].[dbo].[SALELINE]
                    INNER JOIN [CSCTrans].[dbo].[SALEHEAD] ON SALEHEAD.SALE_NO = SALELINE.SALE_NO
                    WHERE SALELINE.DEPT = @FuelDept AND SALEHEAD.ShiftDate = @ShiftDate;";

    var parameters = new { ShiftDate = shiftDate, FuelDept = fuelDept };
    IList<SaleHeadAndSaleLine>? saleHeadAndSaleLines = (IList<SaleHeadAndSaleLine>?)await Connection.QueryAsync<SaleHeadAndSaleLine>(sql, parameters, Transaction);
    return saleHeadAndSaleLines;
  }

  public async Task<IList<SaleHeadAndSaleLine>?> GetSaleHeadAndSaleLineDataByStockCodesAsync(IList<string> stockCodes, DateTime shiftDate, string? fuelDept)
  {
    string sql = $@"SELECT 
                        SALEHEAD.SALE_DATE SaleDate, 
                        SALEHEAD.SALE_TIME SaleTime, 
                        SALELINE.STOCK_CODE StockCode, 
                        SALEHEAD.SD SD
                    FROM  
                        [CSCTrans].[dbo].[SALEHEAD]  
                        INNER JOIN [CSCTrans].[dbo].[SALELINE] ON SALEHEAD.SALE_NO = SALELINE.SALE_NO 
                    WHERE 
                        SALELINE.DEPT = @FuelDept
                        AND SALELINE.STOCK_CODE IN @StockCodes 
                        AND SALEHEAD.ShiftDate = @ShiftDate  
                        AND SALEHEAD.T_Type IN ('SALE', 'REFUND', 'PATP_APP')  
                    ORDER BY 
                        SALEHEAD.SD, 
                        SALEHEAD.SALE_TIME;";

    var parameters = new { StockCodes = stockCodes, ShiftDate = shiftDate, FuelDept = fuelDept };
    IList<SaleHeadAndSaleLine>? saleHeadAndSaleLines = (IList<SaleHeadAndSaleLine>?)await Connection.QueryAsync<SaleHeadAndSaleLine>(sql, parameters, Transaction);
    return saleHeadAndSaleLines;
  }

  public async Task<IList<SaleHeadAndSaleLine>> GetSaleHeadAndSaleLineDataByGradeIdsAsync(IList<int?> gradeIds, DateTime shiftDate, string? fuelDept)
  {
    string sql = $@"SELECT 
                        ISNULL(SALELINE.QUANTITY, 0) AS Quantity, 
                        ISNULL(SALELINE.AMOUNT, 0) AS Amount, 
                        SALELINE.PRICE Price,
                        SALELINE.GradeID GradeID
                    FROM 
                        [CSCTrans].[dbo].[SALELINE] 
                        INNER JOIN [CSCTrans].[dbo].[SALEHEAD] ON SALEHEAD.SALE_NO = SALELINE.SALE_NO 
                    WHERE 
                        SALELINE.DEPT = @FuelDept 
                        AND SALELINE.GradeID IN @GradeIds
                        AND SALEHEAD.ShiftDate = @ShiftDate  
                        AND SALEHEAD.T_Type IN ('SALE', 'REFUND', 'PATP_APP');";

    var parameters = new { GradeIds = gradeIds, ShiftDate = shiftDate, FuelDept = fuelDept };
    IList<SaleHeadAndSaleLine> saleHeadAndSaleLines = (IList<SaleHeadAndSaleLine>)await Connection.QueryAsync<SaleHeadAndSaleLine>(sql, parameters, Transaction);
    return saleHeadAndSaleLines;
  }

  public async Task<decimal> GetTotalDiscountByShiftDateAsync(DateTime shiftDate, string? fuelDept)
  {
    string sql = $@"SELECT 
                        SUM(SALELINE.[DISCOUNT]) AS Total_Discount 
                    FROM 
                        [CSCTrans].[dbo].[SALELINE] 
                        INNER JOIN [CSCTrans].[dbo].[SALEHEAD] 
                        ON SALEHEAD.SALE_NO = SALELINE.SALE_NO 
                    WHERE 
                        SALELINE.DEPT = @FuelDept
                        AND SALEHEAD.T_Type IN ('SALE', 'REFUND', 'PATP_APP')  
                        AND SALEHEAD.ShiftDate = @ShiftDate;";

    var parameters = new { ShiftDate = shiftDate, FuelDept = fuelDept };
    decimal totalDiscount = await Connection.QueryFirstOrDefaultAsync<decimal>(sql, parameters, Transaction);
    return totalDiscount;
  }

  public async Task<IList<SaleHeadDto>?> GetDistinctDatesFromSaleHeadAsync(ExcelReportGenerator excelReportGenerator)
  {
    string sql = $@"SELECT DISTINCT 
                        MONTH(SALE_DATE) AS Month, 
                        YEAR(SALE_DATE) AS Year
                    FROM 
                        [CSCTrans].[dbo].[SALEHEAD] 
                    WHERE 
                        SALE_DATE >= @StartDate 
                    AND SALE_DATE <= @EndDate
                    ORDER BY 
                        MONTH(SALE_DATE)";

    IList<SaleHeadDto>? dates = (IList<SaleHeadDto>?)await Connection.QueryAsync<SaleHeadDto>(sql, excelReportGenerator, Transaction);
    return dates;
  }
  
  public async Task<IList<SaleHead>?> GetSalesByHourReportAsync(ExcelReportGenerator excelReportGenerator)
  {
    string conditionColumn = (excelReportGenerator.Version == "MVCC" ? "SALE_DATE" : "SHIFTDATE");
    string hoQuery = (string.IsNullOrWhiteSpace(excelReportGenerator.StoreId) ? "" : $"STORE='{excelReportGenerator.StoreId}' AND");
    string sql = $@"SELECT 
                        SD, 
                        SALE_DATE AS SaleDate, 
                        SALE_AMT AS SaleAmount, 
                        T_TYPE AS TType, 
                        ShiftDate, 
                        SALE_TIME AS SaleTime 
                    FROM 
                        [CSCTrans].[dbo].[SALEHEAD] 
                    WHERE 
                        {hoQuery} 
                        {conditionColumn} BETWEEN @StartDate AND @EndDate";

    IList<SaleHead>? dates = (IList<SaleHead>?)await Connection.QueryAsync<SaleHead>(sql, excelReportGenerator, Transaction);
    return dates;
  }
}
