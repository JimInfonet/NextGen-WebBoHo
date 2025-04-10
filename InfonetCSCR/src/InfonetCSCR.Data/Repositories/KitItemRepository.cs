namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class KitItemRepository : BaseRepository, IKitItemRepository
{
  public KitItemRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckKitItemExistsByStockCodeAsync(string stockCode)
  {
    const string sql = @"Select KIT_CODE from Kit_Item Where Kit_Item.Stock_Code=@StockCode";

    var parameters = new { StockCode = stockCode };
    string kitCode = await Connection.QueryFirstOrDefaultAsync<string>(sql, parameters, Transaction);
    return (kitCode != null);
  }

  public async Task<bool> DeleteKitItemByKitCodeAsync(string kitCode)
  {
    const string sql = @"Delete from Kit_Item
                        Where Kit_Item.Kit_Code = @KitCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@KitCode", SqlDbType.VarChar, 16).Value = kitCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteKitItemByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from Kit_Item
                        Where Kit_Item.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<IEnumerable<KitItem>> GetKitItemByStockCodesAsync(IList<string> stockCodes)
  {
    string sql = $@"SELECT
	                    KIT_CODE KitCode,
	                    LTRIM(RTRIM(STOCK_CODE)) AS StockCode,
	                    QUANTITY Quantity
                    FROM [CSCMaster].[dbo].[KIT_ITEM]
                    WHERE stock_code in @StockCodes";

    var parameters = new { StockCodes = stockCodes };

    IEnumerable<KitItem> kitItems = await Connection.QueryAsync<KitItem>(sql, parameters, Transaction);

    return kitItems;
  }
}
