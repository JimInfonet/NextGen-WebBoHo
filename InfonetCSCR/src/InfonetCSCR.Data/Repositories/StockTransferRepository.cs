namespace InfonetCSCR.Data.Repositories;

//CSCTransfer Connection
public class StockTransferRepository : BaseRepository, IStockTransferRepository
{
  public StockTransferRepository(IApplicationConfig applicationConfig, IDbTransaction transaction) : base(transaction) { }

  public async Task<int> GetBatchId()
  {
    const string sqlToQueryBatchIdFromStockTransfer = "SELECT MAX(BatchID) AS BatchID FROM StockTransfer AS BatchID";
    const string sqlToQueryBatchIdFromStockTransferHist = "SELECT MAX(BatchID) AS BatchID FROM StockTransferHist AS BatchID";


    int? batchId = await Connection.QuerySingleOrDefaultAsync<int?>(sqlToQueryBatchIdFromStockTransfer, null, Transaction);

    if (batchId == null)
    {
      batchId = await Connection.QuerySingleOrDefaultAsync<int?>(sqlToQueryBatchIdFromStockTransferHist, null, Transaction);
    }

    if (batchId == null)
    {
      batchId = 0;
    }

    return (int)batchId + 1;

  }

  public async Task<IEnumerable<string>> GetStoreIdsAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT DISTINCT 
	                    Store_Id
                    FROM StockTransfer 
                    WHERE Trans_Flag = 'O' AND (Pricegroup_id is null  and Region_Id is null and (VendorId is null or vendorid =''))
                    ORDER BY Store_id");

    IEnumerable<string> storeIds = await Connection.QueryAsync<string>(query.ToString(), null, Transaction);
    return storeIds;
  }

  public async Task<IEnumerable<string>> GetRegionIdsAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT DISTINCT 
	                    Region_id
                    FROM StockTransfer 
                    WHERE Trans_Flag = 'O' AND Region_id IS NOT NULL 
                    ORDER BY region_id");

    IEnumerable<string> regionId = await Connection.QueryAsync<string>(query.ToString(), null, Transaction);
    return regionId;
  }

  public async Task<IEnumerable<string>> GetPriceGroupIdsAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT DISTINCT 
	                    pricegroup_id
                    FROM StockTransfer WHERE Trans_Flag = 'O' AND pricegroup_id IS NOT NULL 
                    ORDER BY pricegroup_id");

    IEnumerable<string> regionId = await Connection.QueryAsync<string>(query.ToString(), null, Transaction);
    return regionId;
  }

  public async Task<IEnumerable<string>> GetVendorIdsAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT DISTINCT 
	                    VendorId as [Id] 
                    FROM StockTransfer WHERE Trans_Flag = 'O' AND VendorId IS NOT NULL 
                    ORDER BY VendorId");

    IEnumerable<string> regionId = await Connection.QueryAsync<string>(query.ToString(), null, Transaction);
    return regionId;
  }

  public async Task<IEnumerable<StockTransferDto>> GetStockTransfersByStoreIdsAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    ProductID,
	                    Description
                    FROM StockTransfer  
                    WHERE Trans_Flag = 'O' and 
	                    trans_type <> 'R' and 
	                    Store_id in @StoreIds  and 
	                    (Region_id is null or Region_Id = '') and 
	                    (Pricegroup_Id is null or PriceGroup_id = '') and 
	                    ( VendorId is null or Vendorid = '') 
                    order by ProductID");

    if (productSearchParameterDto.IsAllProductSelected == false || productSearchParameterDto.IsAllProductSelected == null)
    {
      query.Append($@" OFFSET @SearchStart ROWS FETCH NEXT @SearchLength ROWS ONLY");
    }

    var parameters = new { 
      StoreIds = productSearchParameterDto.StoreIds,
      SearchStart = productSearchParameterDto.Start,
      SearchLength = productSearchParameterDto.Length,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos;
  }

  public async Task<int> CountStockTransfersByStoreIdsAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    ProductID,
	                    Description
                    FROM StockTransfer  
                    WHERE Trans_Flag = 'O' and 
	                    trans_type <> 'R' and 
	                    Store_id in @StoreIds  and 
	                    (Region_id is null or Region_Id = '') and 
	                    (Pricegroup_Id is null or PriceGroup_id = '') and 
	                    ( VendorId is null or Vendorid = '')");

    var parameters = new
    {
      StoreIds = productSearchParameterDto.StoreIds,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos.Count();
  }

  public async Task<IEnumerable<StockTransferDto>> GetStockTransfersByRegionIdsAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    ProductID,
	                    Description 
                    FROM StockTransfer  WHERE Trans_Flag = 'O' and trans_type <> 'R' and Region_id in @RegionIds 
                    order by ProductID");

    if(productSearchParameterDto.IsAllProductSelected == false || productSearchParameterDto.IsAllProductSelected == null)
    {
      query.Append ($@" OFFSET @SearchStart ROWS FETCH NEXT @SearchLength ROWS ONLY");
    }

    var parameters = new
    {
      RegionIds = productSearchParameterDto.RegionIds,
      SearchStart = productSearchParameterDto.Start,
      SearchLength = productSearchParameterDto.Length,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos;
  }

  public async Task<int> CountStockTransfersByRegionIdsAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct  
	                    ProductID,
	                    Description 
                    FROM StockTransfer  WHERE Trans_Flag = 'O' and trans_type <> 'R' and Region_id in @RegionIds");

    var parameters = new
    {
      RegionIds = productSearchParameterDto.RegionIds,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos.Count();
  }

  public async Task<IEnumerable<StockTransferDto>> GetStockTransfersByVendorIdsAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    ProductID,
	                    Description 
                    FROM StockTransfer  WHERE Trans_Flag = 'O' and trans_type <> 'R' and VendorId in @VendorIds 
                    order by ProductID");


    if (productSearchParameterDto.IsAllProductSelected == false || productSearchParameterDto.IsAllProductSelected == null)
    {
      query.Append($@" OFFSET @SearchStart ROWS FETCH NEXT @SearchLength ROWS ONLY");
    }

    var parameters = new
    {
      VendorIds = productSearchParameterDto.VendorIds,
      SearchStart = productSearchParameterDto.Start,
      SearchLength = productSearchParameterDto.Length,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos;
  }

  public async Task<int> CountStockTransfersByVendorIdsAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    ProductID,
	                    Description
                    FROM StockTransfer  WHERE Trans_Flag = 'O' and trans_type <> 'R' and VendorId in @VendorIds");

    var parameters = new
    {
      VendorIds = productSearchParameterDto.VendorIds,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos.Count();
  }

  public async Task<IEnumerable<StockTransferDto>> GetStockTransfersByPriceGroupIdsAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    ProductID,
	                    Description 
                    FROM StockTransfer  WHERE Trans_Flag = 'O' and trans_type <> 'R' and priceGroup_Id in @PriceGroupIds 
                    order by ProductID");


    if (productSearchParameterDto.IsAllProductSelected == false || productSearchParameterDto.IsAllProductSelected == null)
    {
      query.Append($@" OFFSET @SearchStart ROWS FETCH NEXT @SearchLength ROWS ONLY");
    }

    var parameters = new
    {
      PriceGroupIds = productSearchParameterDto.PriceGroupIds,
      SearchStart = productSearchParameterDto.Start,
      SearchLength = productSearchParameterDto.Length,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos;
  }

  public async Task<int> CountStockTransfersByPriceGroupIdsAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    ProductID,
	                    Description
                    FROM StockTransfer  WHERE Trans_Flag = 'O' and trans_type <> 'R' and priceGroup_Id in @PriceGroupIds");

    var parameters = new
    {
      PriceGroupIds = productSearchParameterDto.PriceGroupIds,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos.Count();
  }

  public async Task<bool> AddStockTransferForBatchDeleteAsync(string stockCode, string userCode)
  {
    int batchId = await GetBatchId();

    const string sql = @"INSERT INTO StockTransfer (
	                        BatchId,
	                        ProductID, 
	                        Description, 
	                        Store_id, 
	                        PriceGroup_id, 
	                        Region_id, 
	                        VendorId, 
	                        Trans_Date, 
	                        Trans_User, 
	                        Trans_Flag, 
	                        Trans_Type
	                      )
	                        SELECT 
                          @BatchId AS BatchId, 
	                        Stock_Code AS ProductID, 
	                        Descript AS Description, 
	                        B.Store_ID, 
	                        Null AS PriceGroup_ID, 
	                        Null AS Region_ID, 
	                        'ALL' AS Vendor_ID, 
	                        @DataNow AS Trans_Date, 
	                        @UserCode AS Trans_User, 
	                        'N' AS Trans_Flag, 
	                        'R' AS Trans_Type 
                        FROM CSCMaster.dbo.StockMst AS A CROSS JOIN CSCMaster.dbo.Store AS B 
                        WHERE Stock_Code=@StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@UserCode", SqlDbType.VarChar, 11).Value = userCode;
    cmd.Parameters.Add("@BatchId", SqlDbType.Int).Value = batchId;
    cmd.Parameters.Add("@DataNow", SqlDbType.SmallDateTime).Value = DateTime.Now;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<IEnumerable<StockTransferDto>> GetStockTransfersByStoreIdAsync(ProductSearchParameterDto productSearchParameterDto)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    BatchID, 
	                    ProductID, 
	                    Description,
	                    trans_type TransType 
                    FROM [CSCTransfer].[dbo].[StockTransfer] 
                    WHERE Trans_Flag = 'O' 
                    {((productSearchParameterDto.StoreIds == null || productSearchParameterDto.StoreIds?.Count() == 0 ) ? " " : "AND Store_id in @StoreIds")}
                    ORDER BY BatchID,ProductID OFFSET @SearchStart ROWS FETCH NEXT @SearchLength ROWS ONLY");

    var parameters = new
    {
      StoreIds = productSearchParameterDto.StoreIds,
      SearchStart = productSearchParameterDto.Start,
      SearchLength = productSearchParameterDto.Length,
    };
    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos;
  }

  public async Task<int> CountStockTransfersByStoreIdAsync(string storeId)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT distinct 
	                    BatchID, 
	                    ProductID, 
	                    Description,
	                    trans_type TransType 
                    FROM [CSCTransfer].[dbo].[StockTransfer] 
                    WHERE Trans_Flag = 'O' 
                    {(storeId == null ? " " : "AND Store_id = @StoreId")}
                    ORDER BY BatchID,ProductID");

    var parameters = new
    {
      StoreId = storeId,
    };

    IEnumerable<StockTransferDto> stockTransferDtos = await Connection.QueryAsync<StockTransferDto>(query.ToString(), parameters, Transaction);
    return stockTransferDtos.Count();
  }

  public async Task<bool> UpdateStockTransferFlagAsync()
  {
    string sql = $@"UPDATE [CSCTransfer].[dbo].[StockTransfer] SET Trans_Flag = 'N' WHERE Trans_Flag ='O'";

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows > 0;
  }
}
