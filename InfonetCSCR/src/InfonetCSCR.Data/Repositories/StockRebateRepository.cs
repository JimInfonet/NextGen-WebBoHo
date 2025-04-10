namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StockRebateRepository : BaseRepository, IStockRebateRepository
{
  private readonly IApplicationConfig _applicationConfig;
  public StockRebateRepository(IApplicationConfig applicationConfig, IDbTransaction transaction) : base(transaction)
  {
    _applicationConfig = applicationConfig;
  }

  public async Task<bool> AddStockRebateByStockCodeAsync(string stockCode, string baseStockCode)
  {
    string sql = (_applicationConfig.IsBackOffice) ?
                        @"INSERT INTO [CSCMaster].[dbo].[StockRebates] (
                          Stock_Code, 
                          RebateNo,
                          VendorID, 
                          Rebate, 
                          StartDate, 
                          EndDate, 
                          OrderNo
                          ) 
                          SELECT 
                            @StockCode AS Stock_Code, 
                            RebateNo, 
                            VendorID, 
                            Rebate, 
                            StartDate,
                            EndDate, 
                            OrderNo 
                          FROM [CSCMaster].[dbo].[StockRebates] 
                          WHERE Stock_Code=@BaseStockCode" :
                        @"INSERT INTO [CSCMaster].[dbo].[StockRebates] (
                          Stock_Code, 
                          RebateNo,
                          StoreID,
                          VendorID, 
                          Rebate, 
                          StartDate, 
                          EndDate, 
                          OrderNo
                          ) 
                          SELECT 
                            @StockCode AS Stock_Code, 
                            RebateNo, 
                            StoreID,
                            VendorID, 
                            Rebate, 
                            StartDate,
                            EndDate, 
                            OrderNo 
                          FROM [CSCMaster].[dbo].[StockRebates] 
                          WHERE Stock_Code=@BaseStockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@BaseStockCode", SqlDbType.VarChar, 16).Value = baseStockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteStockRebateByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from StockRebates
                        Where StockRebates.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }
}
