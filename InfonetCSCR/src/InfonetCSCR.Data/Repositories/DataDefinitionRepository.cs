namespace InfonetCSCR.Data.Repositories;

public class DataDefinitionRepository : BaseRepository, IDataDefinitionRepository
{
  public DataDefinitionRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> InsertIntoDataDefinitionBatch(List<DataDefinition> dataDefinitions)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[DataDefinition] (
                       [ProductID]
                      ,[Description]
                      ,[StoreId]
                      ,[DefType]
                      ,[DefCode]
                      ,[Transfer]
                      ,[Trans_Date]
                      ,[Trans_User]
                      ,[VendorId]
                      ,[Trans_Flag]) 
                     VALUES  
                      (@ProductID, @Description, @StoreId, @DefType, @DefCode, @Transfer, @TransferDate, @TransferUser, @VendorId, @Trans_Flag)";

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), dataDefinitions, Transaction);
    return affectedRows > 0;
  }

  public async Task<IEnumerable<DataDefinition>> GetDataDefinitionsAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT
	                      ProductID,
	                      Description,
	                      StoreId,
	                      DefType,
	                      DefCode,
	                      Transfer,
	                      Trans_Date TransferDate,
	                      Trans_User TransferUser,
	                      VendorId,
	                      Trans_Flag
                      FROM [CSCTransfer].[dbo].[DataDefinition]
                      WHERE trans_flag = 'O'  order by Storeid, Productid,deftype");

    IEnumerable<DataDefinition> dataDefinitions = await Connection.QueryAsync<DataDefinition>(query.ToString(), null, Transaction);
    return dataDefinitions;
  }

  public async Task<IEnumerable<DataDefinition>> GetDataDefinitionsByCriteriaAsync(GetDataDefinitionsByCriteriaDto model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT
	                      ProductID,
	                      Description,
	                      StoreId,
	                      DefType,
	                      DefCode,
	                      Transfer,
	                      Trans_Date TransferDate,
	                      Trans_User TransferUser,
	                      VendorId,
	                      Trans_Flag
                      FROM [CSCTransfer].[dbo].[DataDefinition]
                      WHERE ProductId = '{model.ProductId}' and StoreId = '{model.StoreId}' and DefType= '{model.DefType}'  
                      ORDER BY DefCode");

    IEnumerable<DataDefinition> dataDefinitions = await Connection.QueryAsync<DataDefinition>(query.ToString(), null, Transaction);
    return dataDefinitions;
  }

  public async Task<bool> DeleteFromDataDefinitionBatch(List<DataDefinition> dataDefinitions)
  {
    string sql = $@"DELETE FROM [CSCTransfer].[dbo].[DataDefinition] 
                      WHERE StoreId = @StoreId AND DefType = @DefType AND ProductID = @ProductID";

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), dataDefinitions, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> UpdateDataDefinitionTransFlagAsync()
  {
    string sql = $@"UPDATE [CSCTransfer].[dbo].[DataDefinition] SET Trans_Flag = 'Y' WHERE Trans_Flag ='O'";

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows > 0;
  }
}
