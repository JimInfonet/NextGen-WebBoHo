namespace InfonetCSCR.Data.Repositories;

//CSCPump Connection
public class FuelRepository : BaseRepository, IFuelRepository
{
  public FuelRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckStockCodeExistsInGradeAsync(string stockCode)
  {
    const string sql = @"select LTRIM(RTRIM(Stock_Code)) AS StockCode from Grade where stock_code=@StockCode";

    var parameters = new { StockCode = stockCode };
    string? resultStockCode = await Connection.QuerySingleOrDefaultAsync<string?>(sql, parameters, Transaction);
    return (resultStockCode != null);
  }

  public async Task<double> GetFuelPriceByStockCodeAsync(string stockCode)
  {
    const string sql = @"select FuelPrice.CashPrice as Price 
                          from Grade INNER JOIN FuelPrice ON Grade.ID=FuelPrice.GradeID 
                          where Stock_Code=@StockCode 
                          AND FuelPrice.TierID=1 AND FuelPrice.LevelID=1";

    var parameters = new { StockCode = stockCode };
    double? resultPrice = await Connection.QuerySingleOrDefaultAsync<double?>(sql, parameters, Transaction);

    if (resultPrice == null)
    {
      return (double)(resultPrice = 0.0);
    }

    return (double)resultPrice;
  }

  public async Task<bool> CheckDepartmentExistsInFuelDeptAsync(string departmentId)
  {
    const string sql = @"SELECT [Dept] FROM [CSCMaster].[dbo].[FuelDept] WHERE DEPT = @DepartmentId";

    var parameters = new { DepartmentId = departmentId };
    string department = await Connection.QueryFirstOrDefaultAsync<string>(sql, parameters, Transaction);
    return (department != null);
  }

  public async Task<string?> GetFuelDepartmentAsync()
  {
    const string sql = @"SELECT [Dept] FROM [CSCMaster].[dbo].[FuelDept]";

    string? department = await Connection.QueryFirstOrDefaultAsync<string>(sql, null, Transaction);
    return department;
  }
}
