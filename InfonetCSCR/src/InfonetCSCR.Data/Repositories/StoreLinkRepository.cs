namespace InfonetCSCR.Data.Repositories;

public class StoreLinkRepository : BaseRepository, IStoreLinkRepository
{
  public StoreLinkRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<string>> GetStoreIdsByStoreIds(IEnumerable<string> storeIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select store_id as [StoreId] from Storelink where store_id in @storeIds ");

    IEnumerable<string> storeIdsData = await Connection.QueryAsync<string>(query.ToString(), new { storeIds }, Transaction);
    return storeIdsData;
  }

  public async Task<IEnumerable<string>> GetStoreIdsByVendorIds(IEnumerable<string> vendorIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"(select store_id  as [StoreId] 
                        from storelink where region_id in 
                        (select regionid from vendorlink  where VendorId in @vendorIds and (storeid is null or storeid= ''))) UNION
                        (SELECT storeid as [StoreId] From vendorlink WHERE vendorid in @vendorIds AND (regionid IS NULL or regionid = ''))");

    IEnumerable<string> storeIds = await Connection.QueryAsync<string>(query.ToString(), new { vendorIds }, Transaction);
    return storeIds;
  }

  public async Task<IEnumerable<Store>> GetStoresByPriceGroupIds(IEnumerable<string> priceGroupIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select Store_ID, Store_Desc, pricegroup_id as GroupId from Storelink where PriceGroup_id in @priceGroupIds order by Store_ID");

    IEnumerable<Store> storeIds = await Connection.QueryAsync<Store>(query.ToString(), new { priceGroupIds }, Transaction);
    return storeIds;
  }

  public async Task<IEnumerable<Store>> GetStoresByRegionIds(IEnumerable<string> regionIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select distinct Store_ID, Store_Desc from Storelink where Region_id in @regionIds order by Store_ID");

    IEnumerable<Store> storeIds = await Connection.QueryAsync<Store>(query.ToString(), new { regionIds }, Transaction);
    return storeIds;
  }

  public async Task<IEnumerable<Store>> GetAllStoresByRegionIds(IEnumerable<string> regionIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select Store_ID, Store_Desc, region_id as GroupId from Storelink where Region_id in @regionIds order by Store_ID");

    IEnumerable<Store> storeIds = await Connection.QueryAsync<Store>(query.ToString(), new { regionIds }, Transaction);
    return storeIds;
  }

  public async Task<IList<Store>?> GetStoreToLinkBySelectedRegionAsync(string regionId)
  {
    string sql = @$"SELECT DISTINCT
                        s.Store_id, 
                        s.Store_Desc 
                    FROM 
                        [CSCMaster].[dbo].[Store] s
                    LEFT JOIN 
                        [CSCMaster].[dbo].[StoreLink] sl
                    ON 
                        s.Store_id = sl.Store_id 
                        AND sl.Region_ID = '{regionId}'
                    WHERE 
                        sl.Store_id IS NULL
                    ORDER BY 
                        s.Store_id";

    IList<Store>? stores = (IList<Store>)await Connection.QueryAsync<Store>(sql, null, Transaction);
    return stores;
  }
}
