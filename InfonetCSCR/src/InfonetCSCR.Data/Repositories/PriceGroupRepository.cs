namespace InfonetCSCR.Data.Repositories;

public class PriceGroupRepository : BaseRepository, IPriceGroupRepository
{
  public PriceGroupRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<PriceGroup>> GetPriceGroupsByPriceGroupIdsAsync(IEnumerable<string> priceGroupIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select PriceGroupID ID, PriceGroupName Name from  PriceGroups where PriceGroupID in @priceGroupIds");

    IEnumerable<PriceGroup> priceGroups = await Connection.QueryAsync<PriceGroup>(query.ToString(), new { priceGroupIds }, Transaction);
    return priceGroups;
  }

  public async Task<IEnumerable<PriceGroup>> GetPriceGroupsListAsync()
  {
    const string query = @"
      SELECT 
        PriceGroupID ID,
        PriceGroupName Name 
      FROM PriceGroups 
      ORDER BY PriceGroupID";

    return await Connection.QueryAsync<PriceGroup>(query.ToString(), null, Transaction);
  }

  public async Task<bool> AddPriceGroupAsync(PriceGroup priceGroup)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[PriceGroups]
            (
                [pricegroupid],
                [pricegroupname]
            )
            VALUES
            (
                @ID,
                @Name
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, priceGroup, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeletePriceGroupAsync(string priceGroupId, string table)
  {
    string column;

    if (table == "Storelink")
    {
      column = "PriceGroup_id";
    }
    else
    {
      column = "pricegroupid";
    }

    string sql = @$"DELETE FROM [CSCMaster].[dbo].[{table}]
                        WHERE {column} = '{priceGroupId}'";

    int rowsAffected = await Connection.ExecuteAsync(sql, null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<Store>?> GetStoreBySelectedPriceGroupAsync(string priceGroupId)
  {
    string sql = @$"SELECT 
                      A.Store_ID, 
                      B.Store_Desc 
                    FROM 
                      [CSCMaster].[dbo].[StoreLink] AS A 
                    INNER JOIN 
                      [CSCMaster].[dbo].[Store] AS B 
                    ON 
                      B.store_id = A.store_id 
                    WHERE 
                      A.priceGroup_id = '{priceGroupId}' 
                    ORDER BY 
                      Store_ID";

    return await Connection.QueryAsync<Store>(sql, null, Transaction);
  }

  public async Task<IEnumerable<Store>?> GetStoreToLinkBySelectedPriceGroupAsync(string priceGroupId)
  {
    string sql = @$"SELECT DISTINCT
                      Store_id, 
                      Store_Desc 
                    FROM 
                      [CSCMaster].[dbo].[Store] 
                    WHERE 
                      Store_ID NOT IN 
                      (
                        SELECT 
                          Store_id 
                        FROM 
                          [CSCMaster].[dbo].[StoreLink] 
                        WHERE 
                          PriceGroup_ID = '{priceGroupId}' 
                      )
                    ORDER BY 
                      [CSCMaster].[dbo].[Store].Store_ID";

    return await Connection.QueryAsync<Store>(sql, null, Transaction);
  }

  public async Task<StoreLink?> GetStoreFromStoreLinkByStoreIdAsync(string storeId)
  {
    string sql = @$"SELECT 
                      * 
                    FROM 
                      [CSCMaster].[dbo].[StoreLink] 
                    WHERE 
                      store_id = '{storeId}'";

    StoreLink? store = await Connection.QueryFirstOrDefaultAsync<StoreLink>(sql, null, Transaction);
    return store;
  }

  public async Task<bool> AddStoreToStoreLinkAsync(StoreLink store)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[StoreLink] 
            (
                [PriceGroup_Id], 
                [Store_id], 
                [Store_desc] 
            )
            VALUES 
            (
                @PriceGroup_Id, 
                @Store_ID, 
                @Store_Desc 
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, store, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateStoreToStoreLinkAsync(StoreLink currentStore, string? newPriceGroupId)
  {
    string sql = @$"
      UPDATE [CSCMaster].[dbo].[StoreLink]
      SET [PriceGroup_Id] = '{newPriceGroupId ?? string.Empty}'
      WHERE
        ISNULL([Region_Id], '') = ISNULL(@Region_Id, '')
        AND [Store_id] = @Store_ID
        AND ISNULL([Store_desc], '') = ISNULL(@Store_Desc, '')
        AND ISNULL([PriceGroup_Id], '') = ISNULL(@PriceGroup_Id, '')";

    int rowsAffected = await Connection.ExecuteAsync(sql, currentStore, Transaction);
    return rowsAffected > 0;
  }

  public async Task<StoreLink?> GetStoreFromStoreLinkByStoreIdAndPriceGroupIdAsync(StoreLink store)
  {
    string sql = @$"SELECT 
                      * 
                    FROM 
                      [CSCMaster].[dbo].[StoreLink] 
                    WHERE 
                      Store_id = @Store_ID 
                    AND 
                      PriceGroup_Id = @PriceGroup_Id";

    StoreLink? storeLink = await Connection.QueryFirstOrDefaultAsync<StoreLink>(sql, store, Transaction);
    return storeLink;
  }

  public async Task<bool> DeleteStoreAsync(StoreLink store)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[StoreLink] 
                    WHERE 
                      PriceGroup_Id = @PriceGroup_Id 
                    AND 
                      Store_id = @Store_ID";

    int rowAffected = await Connection.ExecuteAsync(sql, store, Transaction);
    return rowAffected > 0;
  }
}
