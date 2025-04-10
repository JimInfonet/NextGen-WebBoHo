namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StockTaxRepository : BaseRepository, IStockTaxRepository
{
  private readonly IApplicationConfig _applicationConfig;
  public StockTaxRepository(IApplicationConfig applicationConfig, IDbTransaction transaction) : base(transaction)
  {
    _applicationConfig = applicationConfig;
  }

  public async Task<bool> AddStockTaxByStockCodeAsync(string stockCode, string baseStockCode)
  {
    string sql = (_applicationConfig.IsBackOffice) ?
                        @"INSERT INTO [CSCMaster].[dbo].[StockTax] (
                          Stock_Code, 
                          Tax_Name, 
                          Tax_Code
                          )
                          SELECT 
                            @StockCode AS Stock_Code, 
                            Tax_Name, 
                            Tax_Code 
                          FROM [CSCMaster].[dbo].[StockTax] 
                          WHERE Stock_Code=@BaseStockCode" :
                        @"INSERT INTO [CSCMaster].[dbo].[StockTax] (
                          Stock_Code, 
                          Tax_Name, 
                          Tax_Code,
                          StoreID
                          )
                          SELECT 
                            @StockCode AS Stock_Code, 
                            Tax_Name, 
                            Tax_Code,
                            StoreID
                          FROM [CSCMaster].[dbo].[StockTax] 
                          WHERE Stock_Code=@BaseStockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@BaseStockCode", SqlDbType.VarChar, 16).Value = baseStockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> AddStockTaxAsync(string stockCode, string taxName, string taxCode, string storeId)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[StockTax] (
                          Stock_Code, 
                          Tax_Name, 
                          Tax_Code,
                          StoreID
                          )
                  VALUES (
                          @StockCode, 
                          @TaxName, 
                          @TaxCode,
                          @StoreId
                          )";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@TaxName", SqlDbType.VarChar, 20).Value = taxName;
    cmd.Parameters.Add("@TaxCode", SqlDbType.VarChar, 20).Value = taxCode;
    cmd.Parameters.Add("@StoreId", SqlDbType.VarChar, 6).Value = storeId;

    cmd.CommandType = CommandType.Text;
    int rowAffected = await cmd.ExecuteNonQueryAsync();

    return rowAffected > 0;
  }

  public async Task<bool> DeleteStockTaxByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from StockTax
                        Where StockTax.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteStockTaxByStockCodeAndStoreIdAsync(string stockCode, string storeId)
  {
    const string sql = @"DELETE FROM [CSCMaster].[dbo].[StockTax] WHERE Stock_Code = @StockCode  AND StoreId = @StoreId";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@StoreId", SqlDbType.VarChar, 6).Value = storeId;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> InsertStockTaxByStockTaxDataAsync(IEnumerable<StockTax> stockTaxes)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[StockTax] (
                          Stock_Code, 
                          Tax_Name, 
                          Tax_Code
                          )
                  VALUES (
                          @StockCode, 
                          @TaxName, 
                          @TaxCode
                          )";

    int affectedRows = await Connection.ExecuteAsync(sql, stockTaxes, Transaction);
    return affectedRows == stockTaxes.Count();
  }

  public async Task<bool> DeleteStockTaxByStockTaxDataAsync(IEnumerable<StockTax> stockTaxes)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[StockTax] WHERE Stock_Code = @StockCode AND Tax_Name = @TaxName";

    int affectedRows = await Connection.ExecuteAsync(sql, stockTaxes, Transaction);
    return true;
  }

  public async Task<IEnumerable<StockTax>> GetStockTaxesByStockCodeAsync(string stockCode)
  {
    string sql = $@"SELECT
	                    LTRIM(RTRIM(Stock_Code)) AS StockCode,
	                    Tax_Name TaxName,
                        Tax_Code TaxCode
                    FROM [CSCMaster].[dbo].[StockTax]
                    WHERE Stock_Code = @StockCode";

    var parameters = new { StockCode = stockCode };

    IEnumerable<StockTax> stockTaxes = await Connection.QueryAsync<StockTax>(sql, parameters, Transaction);

    return stockTaxes;
  }

  public async Task<IEnumerable<StockTax>> GetStockTaxesByStockCodesAsync(IEnumerable<string> stockCodes)
  {
    string sql = $@"SELECT
	                    LTRIM(RTRIM(Stock_Code)) AS StockCode,
	                    Tax_Name TaxName,
                        Tax_Code TaxCode
                    FROM [CSCMaster].[dbo].[StockTax]
                    WHERE Stock_Code in @StockCodes";

    var parameters = new { StockCodes = stockCodes };

    IEnumerable<StockTax> stockTaxes = await Connection.QueryAsync<StockTax>(sql, parameters, Transaction);

    return stockTaxes;
  }
}
