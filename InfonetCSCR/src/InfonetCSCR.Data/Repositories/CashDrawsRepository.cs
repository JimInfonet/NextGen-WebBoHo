namespace InfonetCSCR.Data.Repositories;

public class CashDrawsRepository : BaseRepository, ICashDrawsRepository
{
  public CashDrawsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<DrawTypes>> GetAllDrawTypesDataAsync()
  {
    string sql = $@"SELECT
	                    Currency_Name CurrencyName,
	                    Currency_Type CurrencyType,
	                    Button_Number ButtonNumber,
	                    Value
                    FROM [CSCMaster].[dbo].[DrawTypes];";

    IList<DrawTypes> drawTypes = (IList<DrawTypes>)await Connection.QueryAsync<DrawTypes>(sql, null, Transaction);
    return drawTypes;
  }

  public async Task<bool> InsertDrawTypeAsync(DrawTypes drawType)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[DrawTypes]
                    (Currency_Name, Currency_Type, Button_Number, Value)
                    VALUES (@CurrencyName, @CurrencyType, @ButtonNumber, @Value);";

    int rowsAffected = await Connection.ExecuteAsync(sql, drawType, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateDrawTypeAsync(DrawTypes drawType)
  {
    string sql = $@"UPDATE [CSCMaster].[dbo].[DrawTypes]
                    SET Currency_Name = @CurrencyName, Currency_Type = @CurrencyType, 
                        Button_Number = @ButtonNumber, Value = @Value
                    WHERE Currency_Type = @CurrencyType AND Button_Number = @ButtonNumber;";

    int rowsAffected = await Connection.ExecuteAsync(sql, drawType, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteDrawTypeAsync(DrawTypes drawType)
  {
    string sql = $@"DELETE FROM [CSCMaster].[dbo].[DrawTypes]
                    WHERE Currency_Type = @CurrencyType AND Button_Number = @ButtonNumber;";

    int rowsAffected = await Connection.ExecuteAsync(sql, drawType, Transaction);
    return rowsAffected > 0;
  }
}
