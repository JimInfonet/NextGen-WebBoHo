namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class RegionRepository : BaseRepository, IRegionRepository
{
  public RegionRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<Region>?> GetRegion()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM([Region_Id])) AS ID, [Region_Desc] AS Name, [Region_Code] AS RegionCode 
                      FROM    [CSCMaster].[dbo].[REGION] ORDER BY [Region_Id]");

    return await Connection.QueryAsync<Region>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<Region>> GetRegionsByRegionIdsAsync(IEnumerable<string> regionIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select Region_id ID, Region_Desc Name from  REGION where Region_id in @regionIds");

    IEnumerable<Region> regions = await Connection.QueryAsync<Region>(query.ToString(), new { regionIds }, Transaction);
    return regions;
  }

  public async Task<bool> AddRegionAsync(Region region)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[Region]
            (
                [Region_Id],
                [Region_Desc]
            )
            VALUES
            (
                @ID,
                @Name
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, region, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteRegionAsync(string regionId, string table)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[{table}]
                        WHERE Region_Id = '{regionId}'";

    int rowsAffected = await Connection.ExecuteAsync(sql, null, Transaction);
    return rowsAffected > 0;
  }


  public async Task<StoreLink?> GetStoreFromStoreLinkByStoreIdAsync(string storeId)
  {
    string sql = @$"SELECT 
                      Region_Id AS Region_Id, 
                      Store_id AS Store_ID, 
                      Store_desc AS Store_Desc, 
                      PriceGroup_Id AS PriceGroup_Id 
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
                [Region_Id], 
                [Store_id], 
                [Store_desc] 
            )
            VALUES 
            (
                @Region_Id, 
                @Store_ID, 
                @Store_Desc 
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, store, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateStoreToStoreLinkAsync(string regionId, StoreLink currentStore)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[StoreLink]
                     SET
                         Region_Id = '{regionId ?? string.Empty}'
                    WHERE
                      ISNULL([Region_Id], '') = ISNULL(@Region_Id, '')
                      AND [Store_id] = @Store_ID
                      AND ISNULL([Store_desc], '') = ISNULL(@Store_Desc, '')
                      AND ISNULL([PriceGroup_Id], '') = ISNULL(@PriceGroup_Id, '')";

    int rowsAffected = await Connection.ExecuteAsync(sql, currentStore, Transaction);
    return rowsAffected > 0;
  }

  public async Task<StoreLink?> GetStoreFromStoreLinkByStoreIdAndRegionIdAsync(StoreLink store)
  {
    string sql = @$"SELECT 
                      Region_Id AS Region_Id, 
                      Store_id AS Store_ID, 
                      Store_desc AS Store_Desc, 
                      PriceGroup_Id AS PriceGroup_Id 
                    FROM 
                      [CSCMaster].[dbo].[StoreLink] 
                    WHERE 
                      Store_id = @Store_ID 
                    AND 
                      Region_Id = @Region_Id";

    StoreLink? storeLink = await Connection.QueryFirstOrDefaultAsync<StoreLink>(sql, store, Transaction);
    return storeLink;
  }

  public async Task<bool> DeleteStoreAsync(StoreLink store)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[StoreLink] 
                    WHERE 
                      Region_Id = @Region_Id 
                    AND 
                      Store_id = @Store_ID";

    int rowAffected = await Connection.ExecuteAsync(sql, store, Transaction);
    return rowAffected > 0;
  }
}
