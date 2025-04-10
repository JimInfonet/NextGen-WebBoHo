namespace InfonetCSCR.Data.Repositories;

public class SaleRepository : BaseRepository, ISaleRepository
{
  public SaleRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> SaleExistsForStockCodeAsync(string stockCode)
  {
    var saleNumber = await Connection.QuerySingleOrDefaultAsync<User>
        (sql: "Sproc_CheckSaleData", param: new { StockCode = stockCode }, Transaction, commandType: CommandType.StoredProcedure);
    return (saleNumber != null);
  }

  public async Task<bool> ClearSaleDataAsync()
  {
    var rowAffected = await Connection.ExecuteAsync
        (sql: "Sproc_ClearSaleData", null, Transaction, commandType: CommandType.StoredProcedure);
    return (rowAffected > 0);
  }

  public async Task<bool> CreateFuelNonStatusDataAsync(ExcelReportGenerator excelReportGenerator)
  {
    var rowAffected = await Connection.ExecuteAsync
        (sql: "SProc_FuelNonStatusData",
        param: new
        {
          StartDate = excelReportGenerator.StartDate,
          Enddate = excelReportGenerator.EndDate,
          Dept = excelReportGenerator.Department
        },
        Transaction, commandType: CommandType.StoredProcedure);
    return (rowAffected > 0);
  }

  public async Task<bool> CreateFuelStatusDataAsync(ExcelReportGenerator excelReportGenerator)
  {
    bool isMember = false; // Non member

    if (excelReportGenerator.TEType == "BITE")
    {
      isMember = true; // member
    }

    var rowAffected = await Connection.ExecuteAsync
        (sql: "SProc_FuelStatusData",
        param: new
        {
          StartDate = excelReportGenerator.StartDate,
          Enddate = excelReportGenerator.EndDate,
          Dept = excelReportGenerator.Department,
          TEType = excelReportGenerator.TEType,
          Ismember = isMember,
          MemberNo = excelReportGenerator.MemberNo
        },
        Transaction, commandType: CommandType.StoredProcedure);
    return (rowAffected > 0);
  }

  public async Task<bool> IsSaleDataExistsAsync()
  {
    string sql = @$"SELECT 
                      COUNT(*) 
                    FROM 
                      [CSCTrans].[dbo].[SaleData]";

    int rows = await Connection.QuerySingleAsync<int>(sql, null, Transaction);
    return (rows > 0);
  }

  public async Task<IList<SaleData>?> GetAllSaleDataAsync()
  {
    string sql = @$"SELECT 
                      * 
                    FROM 
                      [CSCTrans].[dbo].[SaleData]";

    IList<SaleData>? saleData = (IList<SaleData>)await Connection.QueryAsync<SaleData>(sql, null, Transaction);
    return saleData;
  }

  public async Task<bool> CreateSaleNonStatusDataAsync(ExcelReportGenerator excelReportGenerator)
  {
    var rowAffected = await Connection.ExecuteAsync
        (sql: "SProc_SaleNonStatusData",
        param: new
        {
          StartDate = excelReportGenerator.StartDate,
          Enddate = excelReportGenerator.EndDate,
          Dept = excelReportGenerator.Department
        },
        Transaction, commandType: CommandType.StoredProcedure);
    return (rowAffected > 0);
  }

  public async Task<bool> CreateSaleStatusDataAsync(ExcelReportGenerator excelReportGenerator)
  {
    bool isMember = false; // Non member

    if (excelReportGenerator.TEType == "BITE")
    {
      isMember = true; // member
    }

    var rowAffected = await Connection.ExecuteAsync
        (sql: "SProc_SaleStatusData",
        param: new
        {
          StartDate = excelReportGenerator.StartDate,
          Enddate = excelReportGenerator.EndDate,
          Dept = excelReportGenerator.Department,
          TEType = excelReportGenerator.TEType,
          Ismember = isMember,
          MemberNo = excelReportGenerator.MemberNo
        },
        Transaction, commandType: CommandType.StoredProcedure);
    return (rowAffected > 0);
  }
  
  public async Task<bool> CreateSaleNonStatusDataByFamilyGroupAsync(ExcelReportGenerator excelReportGenerator)
  {
    var rowAffected = await Connection.ExecuteAsync
        (sql: (excelReportGenerator.ReportNo == 4 ? "SProc_NonStatusSales_FamilySummary" : "SProc_NonStatusSales_FamilyDetails"),
        param: new
        {
          StartDate = excelReportGenerator.StartDate,
          Enddate = excelReportGenerator.EndDate,
          Group = excelReportGenerator.GroupId 
        },
        Transaction, commandType: CommandType.StoredProcedure);
    return (rowAffected > 0);
  }

  public async Task<bool> CreateSaleStatusDataByFamilyGroupAsync(ExcelReportGenerator excelReportGenerator)
  {
    bool isMember = false; // Non member

    if (excelReportGenerator.TEType == "BITE")
    {
      isMember = true; // member
    }

    var rowAffected = await Connection.ExecuteAsync
        (sql: (excelReportGenerator.ReportNo == 4 ? "SProc_StatusSales_FamilySummary" : "SProc_StatusSales_FamilyDetails"),
        param: new
        {
          StartDate = excelReportGenerator.StartDate,
          Enddate = excelReportGenerator.EndDate,
          Group = excelReportGenerator.GroupId,
          TEType = excelReportGenerator.TEType,
          Ismember = isMember,
          MemberNo = excelReportGenerator.MemberNo
        },
        Transaction, commandType: CommandType.StoredProcedure);
    return (rowAffected > 0);
  }
}
