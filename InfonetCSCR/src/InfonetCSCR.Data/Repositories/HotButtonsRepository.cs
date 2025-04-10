namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class HotButtonsRepository : BaseRepository, IHotButtonsRepository
{
  public HotButtonsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<HotButtonsWithProductName>> GetHotButtonsWithProductNameAsync()
  {
    const string sql = @"SELECT 
	                       Button AS ButtonId,
	                       product AS Product,
	                       descript AS ProductName,
	                       Description_1 AS Caption1,
	                       Description_2 AS Caption2
                       FROM [CSCMaster].[dbo].[HotButtons], [CSCMaster].[dbo].[stockmst]
                       WHERE stockmst.Stock_code = Hotbuttons.product
                       ORDER BY [CSCMaster].[dbo].[HotButtons].product";

    IEnumerable<HotButtonsWithProductName> hotButtons = await Connection.QueryAsync<HotButtonsWithProductName>(sql, null, Transaction);
    return hotButtons;
  }

  public async Task<IList<HotButtons>> GetHotButtonsByButtonIdRangeAsync(HotButtonsIdRangeRecord hotButtonsIdRangeRecord)
  {
    string sql = @"SELECT
                    Button ButtonId,
                    Product ProductCode,
                    Description_1 Description1,
                    Description_2 Description2,
                    DefaultQty DefaultQuantity,
                    Color ColorCode,
                    BottlePrice
                  FROM
                    [CSCMaster].[dbo].[HotButtons]
                  WHERE
                    HotButtons.Button >= @StartButtonId AND
                    HotButtons.Button <= @EndButtonId
                  ORDER BY
                  [CSCMaster].[dbo].[HotButtons].Button;";

    IList<HotButtons> hotButtons = (IList<HotButtons>)await Connection.QueryAsync<HotButtons>(sql, hotButtonsIdRangeRecord, Transaction);
    return hotButtons;
  }

  public async Task<bool> InsertHotButtonsAsync(HotButtons hotButtons)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[HotButtons] (
                        Button,
                        Product,
                        Description_1,
                        Description_2,
                        DefaultQty,
                        Color,
                        BottlePrice
                    ) VALUES (
                        @ButtonId,
                        @ProductCode,
                        @Description1,
                        @Description2,
                        @DefaultQuantity,
                        @ColorCode,
                        @BottlePrice
                    );";

    int rowsAffected = await Connection.ExecuteAsync(sql, hotButtons, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateHotButtonsColorAsync(HotButtons hotButton)
  {
    string sql = @"UPDATE [CSCMaster].[dbo].[HotButtons]
                   SET Color = @ColorCode
                   WHERE Button = @ButtonId;";

    int rowsAffected = await Connection.ExecuteAsync(sql, hotButton, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteHotButtonAsync(HotButtons hotButton)
  {
    string sql = @"Delete From [CSCMaster].[dbo].[HotButtons] Where Hotbuttons.Button = @ButtonId";

    int rowsAffected = await Connection.ExecuteAsync(sql, hotButton, Transaction);
    return rowsAffected > 0;
  }

  public async Task<HotButtons> GetBottleReturnDataByProductCodeAsync(string productCode)
  {
    string sql = @"SELECT
                    Button ButtonId,
                    Product ProductCode,
                    Description_1 Description1,
                    Description_2 Description2,
                    DefaultQty DefaultQuantity,
                    Color ColorCode,
                    BottlePrice
                  FROM
                    [CSCMaster].[dbo].[HotButtons]
                  WHERE
                    HotButtons.Product = @ProdcutCode AND
                    HotButtons.Button > 1000"; /// 1000 is hard coded in VB code

    var parameters = new { ProdcutCode = productCode };
    HotButtons hotButton = await Connection.QueryFirstOrDefaultAsync<HotButtons>(sql, parameters, Transaction);
    return hotButton;
  }
}
