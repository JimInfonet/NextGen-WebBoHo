namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StockAcRepository : BaseRepository, IStockAcRepository
{
  private readonly IApplicationConfig _applicationConfig;
  public StockAcRepository(IApplicationConfig applicationConfig, IDbTransaction transaction) : base(transaction)
  {
    _applicationConfig = applicationConfig;
  }

  public async Task<bool> AddStockAcByStockCodeAsync(string stockCode, string baseStockCode)
  {
    string sql = (_applicationConfig.IsBackOffice) ?
                        @"INSERT INTO [CSCMaster].[dbo].[STOCK_AC] (
                          Stock_Code, 
                          As_Code
                          )
                          SELECT 
                            @StockCode AS Stock_Code, 
                            AS_Code 
                          FROM [CSCMaster].[dbo].[STOCK_AC] 
                          WHERE Stock_Code=@BaseStockCode" :
                        @"INSERT INTO [CSCMaster].[dbo].[STOCK_AC] (
                          Stock_Code, 
                          As_Code,
                          StoreID
                          )
                          SELECT 
                            @StockCode AS Stock_Code, 
                            AS_Code,
                            StoreID
                          FROM [CSCMaster].[dbo].[STOCK_AC] 
                          WHERE Stock_Code=@BaseStockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@BaseStockCode", SqlDbType.VarChar, 16).Value = baseStockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> AddStockAcAsync(string stockCode, string asCode, string storeId)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[STOCK_AC] (
                          Stock_Code, 
                          As_Code,
                          StoreID)
                  VALUES (
                          @StockCode, 
                          @AsCode,
                          @StoreId)";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@AsCode", SqlDbType.VarChar, 10).Value = asCode;
    cmd.Parameters.Add("@StoreId", SqlDbType.VarChar, 16).Value = storeId;

    cmd.CommandType = CommandType.Text;
    int rowAffected = await cmd.ExecuteNonQueryAsync();

    return rowAffected > 0;
  }

  public async Task<bool> DeleteStockAcByStockCodeAndStoreIdAsync(string stockCode, string storeId)
  {
    const string sql = @"DELETE FROM [CSCMaster].[dbo].[STOCK_AC] WHERE Stock_Code = @StockCode AND StoreId = @StoreId";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@StoreId", SqlDbType.VarChar, 6).Value = storeId;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteStockAcByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from [CSCMaster].[dbo].[STOCK_AC]
                        Where Stock_Ac.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> InsertStockChargesByStockChargeDataAsync(IEnumerable<StockAc> stockCharges)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[STOCK_AC] (
                          Stock_Code, 
                          As_Code
                          )
                  VALUES (
                          @StockCode, 
                          @Code
                          )";

    int affectedRows = await Connection.ExecuteAsync(sql, stockCharges, Transaction);
    return affectedRows == stockCharges.Count();
  }

  public async Task<bool> DeleteStockChargesByStockChargeDataAsync(IEnumerable<StockAc> stockCharges)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[STOCK_AC] WHERE Stock_Code = @StockCode AND As_Code = @Code";

    int affectedRows = await Connection.ExecuteAsync(sql, stockCharges, Transaction);
    return true;
  }
}
