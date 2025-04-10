namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class KitMasterRepository : BaseRepository, IKitMasterRepository
{
  private readonly IApplicationConfig _config;

  public KitMasterRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  public async Task<bool> MakeStockItemKitAsync(KitMast kitMast)
  {
    const string sql = @"INSERT INTO [CSCMaster].[dbo].[KIT_MAST] (
                            KIT_CODE, 
                            KIT_DESC)
                          VALUES (
                            @KitCode,
                            @KitDescription
                            )";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@KitCode", SqlDbType.VarChar, 16).Value = kitMast.StockCode;
    cmd.Parameters.Add("@KitDescription", SqlDbType.VarChar, 16).Value = kitMast.StockDescription;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteKitMasterByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from Kit_Mast
                        Where Kit_Mast.Kit_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<IEnumerable<KitItem>> GetAllKitItemsAsync()
  {
    string sql = @$"SELECT Kit_Code AS KitCode,
                           LTRIM(RTRIM(Stock_Code)) AS StockCode, 
                           Quantity
                    FROM Kit_Item";

    IEnumerable<KitItem> kitItem = await Connection.QueryAsync<KitItem>(sql, null, Transaction);
    return kitItem;
  }

  public async Task<IEnumerable<KitMast>> GetAllKitMastsAsync()
  {
    const string sql = @"SELECT Kit_Code AS StockCode, 
                                KIT_DESC AS StockDescription
                         FROM [CSCMaster].[dbo].[Kit_Mast]";

    IEnumerable<KitMast> kitMasts = await Connection.QueryAsync<KitMast>(sql, null, Transaction);
    return kitMasts;
  }

  public async Task<Tuple<string, bool>> GetKitCodeByStockCodeAsync(string stockCode)
  {
    string query = $@"SELECT Kit_Code
                        FROM [CSCMaster].[dbo].[Kit_Mast]
                        WHERE Kit_Code = @StockCode";

    var parameters = new { StockCode = stockCode };

    IEnumerable<string> kitMasts = await Connection.QueryAsync<string>(query, parameters, Transaction);
    return Tuple.Create(stockCode, kitMasts.ToList().Count() > 0);
  }

  public async Task<List<KitItem>> GetKitItemsByKitCodeAsync(string stockCode)
  {
    string query = $@"SELECT Kit_Code AS KitCode, LTRIM(RTRIM(Stock_Code)) AS StockCode, Quantity
                        FROM [CSCMaster].[dbo].[Kit_Item]
                        WHERE Kit_Code = @StockCode";

    var parameters = new { StockCode = stockCode };

    IEnumerable<KitItem> result = await Connection.QueryAsync<KitItem>(query, parameters, Transaction);
    return result.ToList();
  }

  public async Task<List<KitItemsDto>> GetKitItemsByKitCodeWithDescAsync(string stockCode)
  {
    string query = $@"SELECT LTRIM(RTRIM(Kit_Item.Stock_Code)) AS [StockCode],
                        StockMst.Descript AS [Description],
                        Kit_Item.Quantity
                    FROM [CSCMaster].[dbo].[Kit_Item]
                        INNER JOIN [CSCMaster].[dbo].StockMst ON Kit_Item.Stock_Code = StockMst.Stock_Code 
                        WHERE Kit_Code = @StockCode 
                        ORDER BY Kit_Item.Stock_Code;";

    var parameters = new { StockCode = stockCode };
    var result = await Connection.QueryAsync<KitItemsDto>(query, parameters, Transaction);
    return result.ToList();
  }

  public async Task<IEnumerable<KitsDto>> GetKitsAsync()
  {
    string sql = $@"SELECT
                          LTRIM(RTRIM(StockMst.Stock_Code)) AS [StockCode],
                          Descript AS [Description],
                          Dept,
                          Pr_Type AS [PriceType]
                      FROM
                          StockMst
                      INNER JOIN
                          Kit_Mast ON StockMst.Stock_Code = Kit_Mast.Kit_Code
                      ORDER BY
                          StockCode;";

    IEnumerable<KitsDto> KitsDto = await Connection.QueryAsync<KitsDto>(sql, null, Transaction);
    return KitsDto;
  }

  public async Task<IEnumerable<KitItemsDto>> GetKitItemsAsync(bool isSellInactive)
  {
    StringBuilder query = new();

    if (_config.IsBackOffice)
    {
      query.Append(@$"SELECT
                          LTRIM(RTRIM(CSCMaster.dbo.StockMst.Stock_Code)) AS [StockCode],
                          CSCMaster.dbo.StockMst.Descript AS [Description] ");

      if (isSellInactive)
      {
        query.Append(@$"FROM CSCMaster.dbo.StockMst
                        LEFT JOIN CSCPump.dbo.Grade
                        ON LTRIM(RTRIM(CSCMaster.dbo.StockMst.Stock_Code)) = LTRIM(RTRIM(CSCPump.dbo.Grade.Stock_Code))
                        WHERE (LTRIM(RTRIM(CSCPump.dbo.Grade.Stock_Code)) IS NULL) OR (LTRIM(RTRIM(CSCPump.dbo.Grade.Stock_Code)) = '')");
      }
      else
      {
        query.Append(@$"FROM (CSCMaster.dbo.StockMst
                        INNER JOIN Stock_BR ON LTRIM(RTRIM(CSCMaster.dbo.StockMst.Stock_Code)) = LTRIM(RTRIM(Stock_BR.Stock_Code)))
                        LEFT JOIN CSCPump.dbo.Grade ON LTRIM(RTRIM(CSCMaster.dbo.StockMst.Stock_Code)) = LTRIM(RTRIM(CSCPump.dbo.Grade.Stock_Code))
                        WHERE (Stock_BR.ActiveStock IS NULL OR Stock_BR.ActiveStock = 1)
                        AND(LTRIM(RTRIM(CSCPump.dbo.Grade.Stock_Code)) IS NULL OR LTRIM(RTRIM(CSCPump.dbo.Grade.Stock_Code)) = '')");
      }

      query.Append(@" ORDER BY LTRIM(RTRIM(CSCMaster.dbo.StockMst.Stock_Code));");
    }
    else
    {
      query.Append(@$"SELECT
                            LTRIM(RTRIM(stockmst.Stock_Code)) AS [StockCode],
                            Descript AS [Description]
                      FROM
                            StockMst ");

      if (isSellInactive)
      {
        query.Append(@$"INNER JOIN
                            Stock_br ON LTRIM(RTRIM(Stockmst.stock_code)) = LTRIM(RTRIM(stock_br.stock_code))
                        WHERE
                            (stock_br.ActiveStock IS NULL OR stock_br.ActiveStock = 1)");
      }

      query.Append(@" ORDER BY StockCode ASC;");
    }

    IEnumerable<KitItemsDto> kitItemsDto = await Connection.QueryAsync<KitItemsDto>(query.ToString(), null, Transaction);

    return kitItemsDto;
  }

  public async Task<bool> AddKitItemAsync(KitItemsDto kitItem)
  {
    string query = $@"INSERT INTO [CSCMaster].[dbo].[Kit_Item] (
                          Stock_Code,
                          Kit_Code,
                          Quantity
                          )
                  VALUES (
                          @StockCode,
                          @KitCode,
                          1
                          )";

    int rowsAffected = await Connection.ExecuteAsync(query, kitItem, Transaction);

    return rowsAffected > 0;
  }

  public async Task<bool> IsKitItemAsync(string stockCode)
  {
    string query = $@"SELECT COUNT(*)
                      FROM Kit_Mast
                      WHERE kit_code = @StockCode;";

    var parameters = new { StockCode = stockCode };

    var rowCount = await Connection.ExecuteScalarAsync<int>(query, parameters, Transaction);

    return rowCount > 0;
  }

  public async Task<bool> IsItemAlreadyInKitAsync(KitItemsDto kitItem)
  {
    string query = $@"SELECT COUNT(*)
                      FROM Kit_Item
                      WHERE Stock_code = @StockCode 
                      AND kit_code = @KitCode;";

    var rowCount = await Connection.ExecuteScalarAsync<int>(query, kitItem, Transaction);

    return rowCount > 0;
  }

  public async Task<bool> UpdateKitItemAsync(KitItemsDto kitItem)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[Kit_Item]
                      SET
                        Quantity = @Quantity
                        WHERE Kit_Code = @KitCode
                        AND Stock_Code = @StockCode;";

    int rowsAffected = await Connection.ExecuteAsync(query, kitItem, Transaction);

    return rowsAffected > 0;
  }

  public async Task<bool> DeleteKitItemAsync(KitItemsDto kitItem)
  {
    string query = $@"DELETE FROM Kit_Item
                      WHERE Kit_Code = @KitCode
                      AND Stock_Code = @StockCode;";

    int rowsAffected = await Connection.ExecuteAsync(query, kitItem, Transaction);

    return rowsAffected > 0;
  }

  public async Task<List<(string, string, double)>> GetStockCodeAndQuantityByKitCodeAsync(string stockCode)
  {
    string query = $@"SELECT Kit_Code, Stock_Code, Quantity
                        FROM [CSCMaster].[dbo].[Kit_Item]
                        WHERE Kit_Code = @StockCode";
    var parameters = new { StockCode = stockCode };
    IEnumerable<(string, string, double)> result = await Connection.QueryAsync<(string, string, double)>(query, parameters, Transaction);
    return result.ToList();
  }
}
