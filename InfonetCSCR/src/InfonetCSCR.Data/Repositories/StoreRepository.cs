namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StoreRepository : BaseRepository, IStoreRepository
{
  public StoreRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<Store>?> GetStoreIdsByVendorIds(List<string> vendorIds)
  {
    string vendorIdsString = "'" + string.Join("', '", vendorIds) + "'"; // ['001', '002'] --> "'001', '002'"

    StringBuilder query = new StringBuilder();
    query.Append($@"(SELECT store_id  AS [Store_ID] FROM storelink 
                         WHERE region_id IN 
                           (SELECT regionid FROM vendorlink  WHERE VendorId IN ({vendorIdsString})  
                             AND (storeid IS NULL OR storeid = '')))
                       UNION  
                       (SELECT storeid AS [Store_ID] FROM vendorlink 
                         WHERE vendorid IN ({vendorIdsString}) AND (regionid IS NULL OR regionid = ''))");

    IEnumerable<Store> storeIds = await Connection.QueryAsync<Store>(query.ToString(), null, Transaction);
    return storeIds;
  }

  public async Task<IEnumerable<Store>> GetStoresByStoreIdsAsync(IEnumerable<string> storeIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select store_id Store_ID, store_desc Store_Desc from  store where store_id IN @storeIds");

    IEnumerable<Store> stores = await Connection.QueryAsync<Store>(query.ToString(), new { storeIds }, Transaction);
    return stores;
  }

  public async Task<IEnumerable<StoresWithRegionAndPriceGroup>?> GetStoresWithRegionAndPriceGroup()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT * 
                      FROM
                      (
	                      SELECT	Store_id AS StoreID, Store_desc AS StoreName, '' AS RegionID, '' AS PriceGroupID
	                      FROM	[CSCMaster].[dbo].[Store] 
	                      WHERE	RTRIM(LTRIM(Store_id)) NOT IN (SELECT RTRIM(LTRIM(Store_id)) FROM [CSCMaster].[dbo].[StoreLink])
	                      UNION ALL
	                      SELECT	RTRIM(LTRIM(SL.Store_id)) AS StoreID, SL.Store_desc AS StoreName, RTRIM(LTRIM(SL.Region_Id)) AS RegionID, RTRIM(LTRIM(SL.PriceGroup_Id)) AS PriceGroupID
	                      FROM	[CSCMaster].[dbo].[StoreLink] SL
                      ) AS P
                      ORDER BY StoreID");

    return await Connection.QueryAsync<StoresWithRegionAndPriceGroup>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<Store>> GetAllStoresAsync()
  {
    string query = $@"Select 
                        store_id Store_ID, 
                        store_desc Store_Desc, 
                        addr1 Addr1, 
                        city City, 
                        province Province, 
                        post_code Post_Code, 
                        phone Phone, 
                        fax Fax, 
                        contact Contact, 
                        isWarehouse IsWarehouse, 
                        data_path Data_Path 
                      From 
                        [CSCMaster].[dbo].[Store] 
                      Order By Store_ID";

    return await Connection.QueryAsync<Store>(query, null, Transaction);
  }

  public async Task<IEnumerable<StoreOptionDto>> GetStoreDropdownValuesAsync()
  {
    string sql = $@"SELECT
                      Store_ID AS StoreID,
                      Store_Desc AS StoreDesc,
                      Data_Path AS DataPath,
                      IsWarehouse AS IsWarehouse
                    FROM
                      [CSCMaster].[dbo].STORE
                    WHERE
                      Data_Path<>'';";

    IEnumerable<StoreOptionDto> options = await Connection.QueryAsync<StoreOptionDto>(sql, null, Transaction);
    return options;
  }

  public async Task<Store> GetStoreByStoreIdAsync(string storeId)
  {
    string query = $@"Select 
                        store_id Store_ID, 
                        store_desc Store_Desc, 
                        addr1 Addr1, 
                        city City, 
                        province Province, 
                        post_code Post_Code, 
                        phone Phone, 
                        fax Fax, 
                        contact Contact, 
                        isWarehouse IsWarehouse, 
                        data_path Data_Path 
                      From 
                        [CSCMaster].[dbo].[Store] 
                      Where store_id = @StoreId";

    Store store = await Connection.QueryFirstOrDefaultAsync<Store>(query, new { StoreId = storeId }, Transaction);
    return store;
  }
  
  public async Task<bool> AddStoreAsync(Store store)
  {
    string query = @$"
            INSERT INTO [CSCMaster].[dbo].[Store]
            (
               [Store_ID]
              ,[Store_Desc]
              ,[Addr1]
              ,[City]
              ,[Province]
              ,[Post_Code]
              ,[Phone]
              ,[Fax]
              ,[Contact]
              ,[Data_Path]
              ,[IsWarehouse]
            )
            VALUES
            (
                @Store_ID,
                @Store_Desc,
                @Addr1, 
                @City,
                @Province,
                @Post_Code,
                @Phone,
                @Fax, 
                @Contact,
                @Data_Path,
                @IsWarehouse 
            )";

    int rowsAffected = await Connection.ExecuteAsync(query, store, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IList<string>> GetTableColumnsAsync(string tableName)
  {
    string query = $@"SELECT A.Name AS FName 
                      FROM SysColumns AS A 
                      INNER JOIN SysObjects AS B 
                      ON A.ID=B.ID WHERE (B.Name = @TableName)";

    IList<string> columns = (IList<string>)await Connection.QueryAsync<string>(query, new { TableName = tableName }, Transaction);
    return columns;
  }

  public async Task<bool> InsertIntoTablesAsync(string table, string sourceFields, string destFields, string sourceStore)
  {
    string query = @$"
                    INSERT INTO [CSCMaster].[dbo].[{table}]
                      ( {sourceFields} ) 
                      ( SELECT {destFields} FROM {table} 
                    WHERE ( StoreID='{sourceStore}' ))";

    int rowsAffected = await Connection.ExecuteAsync(query, null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateStoreAsync(Store store)
  {
    string query = @$"
            UPDATE [CSCMaster].[dbo].[Store]
            SET
               Store_Desc = @Store_Desc,
               Addr1 = @Addr1, 
               City = @City,
               Province = @Province,
               Post_Code = @Post_Code,
               Phone = @Phone,
               Fax = @Fax,
               Contact = @Contact,
               Data_Path = @Data_Path,
               IsWarehouse = @IsWarehouse 
            WHERE
               Store_ID = @Store_ID";

    int rowsAffected = await Connection.ExecuteAsync(query, store, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteStoreAsync(string storeId, string table)
  {
    string query = ((table == "store" || table == "StoreLink") ? "Store_ID" : "StoreID");
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[{table}]
                        WHERE {query} = '{storeId}'";

    int rowsAffected = await Connection.ExecuteAsync(sql, null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IList<Store>?> GetStoreBySelectedRegionAsync(string regionId)
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
                      A.region_id = '{regionId}' 
                    ORDER BY 
                      Store_ID";

    IList<Store>? stores = (IList<Store>)await Connection.QueryAsync<Store>(sql, null, Transaction);
    return stores;
  }
}
