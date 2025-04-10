namespace InfonetCSCR.Data.Repositories;

public class PricebookTransferRepository : BaseRepository, IPricebookTransferRepository
{
  private readonly IApplicationConfig _config;
  public PricebookTransferRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  public async Task<int> GetNewBatchId(string dataTransferType)
  {
    string tabelName = GetDataTransferTableName(dataTransferType);
    string sql = $"SELECT MAX(batchid) AS [maxSequenceNumber] FROM {tabelName}";

    int? maxSequenceNumber = await Connection.QuerySingleOrDefaultAsync<int?>(sql, null, Transaction);
    int? maxSequenceNumberHist = await Connection.QuerySingleOrDefaultAsync<int?>(sql + "Hist", null, Transaction);

    if (dataTransferType == DataTransferType.PriceChange)
    {
      sql = $"SELECT MAX(batchid) AS [maxSequenceNumber] FROM PriceDefinition";
      int? maxPriceDefinitionBatchId = await Connection.QuerySingleOrDefaultAsync<int?>(sql, null, Transaction);
      int? maxPriceDefinitionBatchIdHist = await Connection.QuerySingleOrDefaultAsync<int?>(sql + "Hist", null, Transaction);
      maxSequenceNumber = Math.Max(maxSequenceNumber ?? 0, maxPriceDefinitionBatchId ?? 0);
      maxSequenceNumberHist = Math.Max(maxSequenceNumberHist ?? 0, maxPriceDefinitionBatchIdHist ?? 0);
    }

    int newBatchId = Math.Max((maxSequenceNumber ?? 0), (maxSequenceNumberHist ?? 0)) + 1;
    return newBatchId;
  }

  public async Task<int> InsertPriceDefinition(PriceDefinition model)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[PriceDefinition] ( 
                             Batchid
                            ,ProductId
                            ,PR_Type
                            ,Store_Id
                            ,PR_Units
                            ,Price_Number
                            ,Price
                            ,Margin
                            ,PR_FROM_QTY
                            , PR_TO_QTY
                            ,PR_FROM
                            ,PR_TO
                            ,OrderNo
                            ,VendorId
                            ,Trans_Date
                            ,Trans_User
                            ,Trans_Flag ) 
                     VALUES (
                            {model.BatchId}
                            ,'{model.ProductId}'
                            ,'{model.PR_Type}'
                            ,'{model.StoreId}'
                            ,{(string.IsNullOrWhiteSpace(model.PR_Units) ? "NULL" : $@"'{model.PR_Units}'")}
                            ,{model.PriceNumber}
                            ,{model.Price}
                            ,{model.Margin}
                            ,{model.PR_From_Qty}
                            ,{model.PR_To_Qty}
                            ,{(model.PR_FROM == null ? "NULL" : $@"'{model.PR_FROM}'")}
                            ,{(model.PR_TO == null ? "NULL" : $@"'{model.PR_TO}'")}
                            ,{model.OrderNo}
                            ,'{model.VendorId}'
                            ,'{DateTime.Now}'
                            ,'{model.Trans_User}'
                            ,'{model.Trans_Flag}')";

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<int> UpdatePriceDefinition(PriceDefinition model)
  {
    string sql = @"
        UPDATE [CSCTransfer].[dbo].[PriceDefinition]
        SET
            PR_Units = @PR_Units,
            Price_Number = @PriceNumber,
            Price = @Price,
            Margin = @Margin,
            PR_FROM_QTY = @PR_From_Qty,
            PR_TO_QTY = @PR_To_Qty,
            PR_FROM = @PR_FROM,
            PR_TO = @PR_TO,
            OrderNo = @OrderNo,
            VendorId = @VendorId,
            Trans_User = @Trans_User
        WHERE
            BatchId = @BatchId
            AND ProductId = @ProductId
            AND PR_Type = @PR_Type
            AND Store_Id = @StoreId
            AND Trans_Flag = 'N'
    ";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@PR_Units", SqlDbType.VarChar, 1).Value = model.PR_Units ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceNumber", SqlDbType.Int).Value = model.PriceNumber;
    cmd.Parameters.Add("@Price", SqlDbType.Money).Value = model.Price ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Margin", SqlDbType.Decimal).Value = model.Margin ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PR_From_Qty", SqlDbType.Decimal).Value = model.PR_From_Qty;
    cmd.Parameters.Add("@PR_To_Qty", SqlDbType.Decimal).Value = model.PR_To_Qty ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PR_FROM", SqlDbType.SmallDateTime).Value = model.PR_FROM ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PR_TO", SqlDbType.SmallDateTime).Value = model.PR_TO ?? (object)DBNull.Value;
    cmd.Parameters.Add("@OrderNo", SqlDbType.Int).Value = model.OrderNo;
    cmd.Parameters.Add("@VendorId", SqlDbType.VarChar, 10).Value = model.VendorId ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Trans_User", SqlDbType.VarChar, 10).Value = model.Trans_User ?? (object)DBNull.Value;
    cmd.Parameters.Add("@BatchId", SqlDbType.Int).Value = model.BatchId;
    cmd.Parameters.Add("@ProductId", SqlDbType.VarChar, 16).Value = model.ProductId ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PR_Type", SqlDbType.VarChar, 1).Value = model.PR_Type ?? (object)DBNull.Value;
    cmd.Parameters.Add("@StoreId", SqlDbType.VarChar, 6).Value = model.StoreId ?? (object)DBNull.Value;

    cmd.CommandType = CommandType.Text;
    int rowAffected = await cmd.ExecuteNonQueryAsync();

    return rowAffected;
  }


  public async Task<int> InsertPriceDefinitionBatch(List<PriceDefinition> models)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[PriceDefinition] ( 
                             Batchid
                            ,ProductId
                            ,PR_Type
                            ,Store_Id
                            ,PR_Units
                            ,Price_Number
                            ,Price
                            ,Margin
                            ,PR_FROM_QTY
                            , PR_TO_QTY
                            ,PR_FROM
                            ,PR_TO
                            ,OrderNo
                            ,VendorId
                            ,Trans_Date
                            ,Trans_User
                            ,Trans_Flag ) 
                     VALUES  ";

    string separator = "";
    foreach (var model in models)
    {
      sql += separator;
      sql += $@"(
                    {model.BatchId}
                   ,'{model.ProductId}'
                   ,'{model.PR_Type}'
                   ,'{model.StoreId}'
                   ,{(string.IsNullOrWhiteSpace(model.PR_Units) ? "NULL" : $@"'{model.PR_Units}'")}
                   ,{model.PriceNumber}
                   ,{model.Price}
                   ,{model.Margin}
                   ,{model.PR_From_Qty}
                   ,{model.PR_To_Qty}
                   ,{(model.PR_FROM == null ? "NULL" : $@"'{model.PR_FROM}'")}
                   ,{(model.PR_TO == null ? "NULL" : $@"'{model.PR_TO}'")}
                   ,{model.OrderNo}
                   ,'{model.VendorId}'
                   ,'{DateTime.Now}'
                   ,'{model.Trans_User}'
                   ,'{model.Trans_Flag}'
                  )";

      separator = ", ";
    }

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<int> InsertStockDefinition(StockDefinition model)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[StockTransfer] ( 
                             [BatchId]
                            ,[ProductID]
                            ,[Description]
                            ,[Store_id]
                            ,[PriceGroup_id]
                            ,[Region_id]
                            ,[Trans_Date]
                            ,[Trans_User]
                            ,[Trans_Flag]
                            ,[Trans_Type]
                            ,[VendorId] ) 
                     VALUES (
                            {model.BatchId}
                            ,'{model.ProductId}'
                            ,'{model.Description}'
                            ,'{model.StoreId}'
                            ,{(model.PriceGroupId == null ? "NULL" : $@"'{model.PriceGroupId}'")}
                            ,{(model.RegionId == null ? "NULL" : $@"'{model.RegionId}'")}
                            ,'{DateTime.Now}'
                            ,'{model.TransUser}'
                            ,'O'
                            ,'{model.TransType}'
                            ,'{model.VendorId}')";

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<int> InsertStockDefinitionBatch(List<StockDefinition> models)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[StockTransfer] ( 
                             [BatchId]
                            ,[ProductID]
                            ,[Description]
                            ,[Store_id]
                            ,[PriceGroup_id]
                            ,[Region_id]
                            ,[Trans_Date]
                            ,[Trans_User]
                            ,[Trans_Flag]
                            ,[Trans_Type]
                            ,[VendorId] ) 
                     VALUES  (@BatchId, @ProductId, @Description, @StoreId, @PriceGroupId, @RegionId, @TransDate, @TransUser, 'O', @TransType, @VendorId)";



    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), models, Transaction);
    return affectedRows;
  }

  public async Task<int> InsertPriceChangeTransfer(PriceChangeTransfer model)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[PriceChangeTransfer] ( 
                             BatchId
                            ,ProductID
                            ,Description
                            ,PriceType
                            ,Store_Id
                            ,PriceGroup_id
                            ,Region_id
                            ,VendorId
                            ,OrderNo
                            ,Trans_Date
                            ,Trans_User
                            ,Trans_Flag ) 
                      VALUES (
                            {model.BatchId}
                            ,'{model.ProductId}'
                            ,'{model.Description.ToEncodedSqlString()}'
                            ,'{model.PriceType}'
                            ,'{model.StoreId}'
                            ,{(string.IsNullOrWhiteSpace(model.PriceGroupId) ? "NULL" : $"'{model.PriceGroupId}'")}
                            ,{(string.IsNullOrWhiteSpace(model.RegionId) ? "NULL" : $"'{model.RegionId}'")}
                            ,'{model.VendorId}'
                            ,{model.OrderNo}
                            ,'{DateTime.Now}'
                            ,'{model.TransUser}'
                            ,'{model.TransFlag}')";

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<int> InsertPriceChangeTransferBatch(List<PriceChangeTransfer> models)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[PriceChangeTransfer] ( 
                             BatchId
                            ,ProductID
                            ,Description
                            ,PriceType
                            ,Store_Id
                            ,PriceGroup_id
                            ,Region_id
                            ,VendorId
                            ,OrderNo
                            ,CurrentPrice
                            ,Trans_Date
                            ,Trans_User
                            ,Trans_Flag ) 
                      VALUES 
      ";

    string separator = "";
    foreach (var model in models)
    {
      sql += separator;
      sql += $@"(
                   {model.BatchId}
                   ,'{model.ProductId}'
                   ,'{model.Description.ToEncodedSqlString()}'
                   ,'{model.PriceType}'
                   ,'{model.StoreId}'
                   ,{(string.IsNullOrWhiteSpace(model.PriceGroupId) ? "NULL" : $"'{model.PriceGroupId}'")}
                   ,{(string.IsNullOrWhiteSpace(model.RegionId) ? "NULL" : $"'{model.RegionId}'")}
                   ,'{model.VendorId}'
                   ,{model.OrderNo}
                   ,{((model.CurrentPrice == null) ? "NULL" : model.CurrentPrice)}
                   ,'{DateTime.Now}'
                   ,'{model.TransUser}'
                   ,'{model.TransFlag}'
                 )";

      separator = ", ";
    }

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<int> InsertCostChangeTransferBatch(List<CostChangeTransfer> models)
  {
    string sql = $@"INSERT INTO [dbo].[CostChangeTransfer]
                          ([BatchId]
                          ,[ProductID]
                          ,[Description]
                          ,[StartDate]
                          ,[EndDate]
                          ,[OldCost]
                          ,[NewCost]
                          ,[Store_id]
                          ,[PriceGroup_id]
                          ,[Region_id]
                          ,[VendorId]
                          ,[OrderNo]
                          ,[Trans_Date]
                          ,[Trans_User]
                          ,[Trans_Flag]
                          ,[ActiveVendor]
                          ,[PriceChangeWithCost])
                    VALUES 
      ";

    string separator = "";
    foreach (var model in models)
    {
      sql += separator;
      sql += $@"(
                   {model.BatchId}
                   ,'{model.ProductId}'
                   ,'{model.Description.ToEncodedSqlString()}'
                   ,{(model.StartDate == null ? "NULL" : $"'{model.StartDate}'")}
                   ,{(model.EndDate == null ? "NULL" : $"'{model.EndDate}'")}
                   ,'{model.OldCost}'
                   ,'{model.NewCost}'
                   ,'{model.StoreId}'
                   ,{(model.PriceGroupId == null ? "NULL" : $"'{model.PriceGroupId}'")}
                   ,{(model.RegionId == null ? "NULL" : $"'{model.RegionId}'")}
                   ,'{model.VendorId}'
                   ,'{model.OrderNo}'
                   ,'{model.TransDate}'
                   ,'{model.TransUser}'
                   ,'{model.TransFlag}'
                   ,'{model.ActiveVendor}'
                   ,'{model.PriceChangeWithCost}'
                  )";

      separator = ", ";
    }

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<int> InsertCommodityTransferBatch(List<CommodityTransferDto> models, string? userCode)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[CommodityTransfer] ( 
                             BatchId,
                              Dept,
                              SubDept,
                              SubDetail,
                              Description,
                              Store_Id,
                              Trans_Date,
                              Trans_User,
                              Trans_Flag,
                              Trans_Type ) 
                      VALUES 
      ";
    string separator = "";
    foreach (var model in models)
    {
      sql += separator;
      sql += $@"(
                   {model.BatchId}
                   ,'{model.Dept}'
                   ,'{model.SubDept}'
                   ,'{model.SubDetail}'
                   ,'{model.Description.ToEncodedSqlString()}'
                   ,'{model.StoreID}'
                   ,'{DateTime.Now}'
                   ,'{userCode}'
                   ,'O'
                   ,'{model.TransType}'
                  )";

      separator = ", ";
    }

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<int> InsertHotButtonsTransferBatch(List<HotButtonsTransferDto> models, string? userCode)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[HotButtonTransfer] ( 
                             BatchId,
                              Button,
                              ProductId,
                              Description,
                              Caption1,
                              Caption2,
                              Store_Id,
                              Trans_Date,
                              Trans_User,
                              Trans_Flag,
                              Trans_Type ) 
                      VALUES 
      ";
    string separator = "";
    foreach (var model in models)
    {
      sql += separator;
      sql += $@"(
                   {model.BatchId}
                   ,'{model.Button}'
                   ,'{model.ProductId}'
                   ,'{model.Description.ToEncodedSqlString()}'
                   ,'{model.Caption1}'
                   ,'{model.Caption2}'
                   ,'{model.StoreID}'
                   ,'{DateTime.Now}'
                   ,'{userCode}'
                   ,'O'
                   ,'{model.TransType}'
                  )";

      separator = ", ";
    }

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<int> InsertPromotionTransferBatch(List<PromotionTransfer> models)
  {
    string sql = $@" INSERT INTO PromotionTransfer            
                     ( 
                            BatchId
                           ,PromoID
                           ,Description
                           ,StartDate
                           ,EndDate
                           ,Store_Id
                           ,Trans_Date
                           ,Trans_User
                           ,Trans_Flag
                           ,Trans_Type 
                     ) 
                     VALUES 
                     (
                            @BatchId
                           ,@PromoID
                           ,@Description
                           ,@StartDate
                           ,@EndDate
                           ,@StoreId
                           ,@TransDate
                           ,@TransUser
                           ,@TransFlag
                           ,@TransType 
                     )";

    int affectedRows = await Connection.ExecuteAsync(sql, models, Transaction);
    return affectedRows;
  }

  public async Task<int> DeleteFromTransferList(DeleteFromTransferListDto model)
  {
    string tableName = GetDataTransferTableName(model.DataTransferType);
    string sql = $@"DELETE FROM {tableName} WHERE Trans_Flag = 'O' ";
    string notDeleteAllSql = "";

    if (!(model.DeleteAll ?? false))
    {
      string columnName = GetDataTransferDeleteColumnName(model.DataTransferType);
      notDeleteAllSql += model.StoreId == null ? $" AND batchid = {model.BatchId} AND {columnName} ='{model.ItemId}' "
                                   : $" AND Store_id = '{model.StoreId}' ";

      if (model.Description != null)
      {
        notDeleteAllSql += $" AND Description = '{model.Description}' ";
      }
      sql += notDeleteAllSql;
    }

    if (model.DataTransferType == DataTransferType.PriceChange)
    {
      sql += "; DELETE FROM PriceDefinition WHERE Trans_Flag = 'O' ";
      sql += notDeleteAllSql;
    }

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<IEnumerable<PriceChangeTransferMinimalDto>> GetPriceChangeTransfersAsync(string? storeId)
  {
    string sql = $@"SELECT DISTINCT 
                           BatchId
                          ,ProductId
                          ,Description
                          ,PriceType 
                    FROM PriceChangeTransfer  
                    WHERE Trans_Flag = 'O' ";
    if (storeId != null) sql += $" AND Store_Id = '{storeId}' ";
    sql += " ORDER BY BatchId, ProductId ";

    var priceChangeTransfers = await Connection.QueryAsync<PriceChangeTransferMinimalDto>(sql, null, Transaction);
    return priceChangeTransfers;
  }

  public async Task<IEnumerable<CommodityTransferDto>> GetCommodityTransfersAsync(string? storeId)
  {
    string sql = $@"SELECT DISTINCT
                        CommodityTransfer.BatchId, 
                        CommodityTransfer.Dept, 
                        CommodityTransfer.SubDept, 
                        CommodityTransfer.SubDetail, 
                        CommodityTransfer.Description,
                        CommodityTransfer.Trans_Type AS TransType
                    FROM 
                        CSCTransfer.dbo.CommodityTransfer CommodityTransfer 
                    WHERE 
                        CommodityTransfer.Trans_Flag = 'O' {(storeId == null ? "" : $" AND Store_Id = '{storeId}' ")}
                    ORDER BY 
                        batchId,Dept,SubDept,SubDetail";
    IEnumerable<CommodityTransferDto> commodityTransfers = await Connection.QueryAsync<CommodityTransferDto>(sql, null, Transaction);
    return commodityTransfers;
  }

  public async Task<IEnumerable<HotButtonsTransferDto>> GetHotButtonsTransfersAsync(string? storeId)
  {
    string sql = $@"SELECT DISTINCT
                        BatchId, 
                        Button,
                        ProductId,
                        Description, 
                        Caption1,
                        Caption2,
                        trans_type AS TransType
                    FROM 
                        [CSCTransfer].[dbo].[HotButtonTransfer] 
                    WHERE 
                        Trans_Flag = 'O' {(storeId == null ? "" : $" AND Store_Id = '{storeId}' ")}
                    ORDER BY 
                        Batchid, ProductID;";

    var hotButtonsTransfers = await Connection.QueryAsync<HotButtonsTransferDto>(sql, null, Transaction);
    return hotButtonsTransfers;
  }

  public async Task<IEnumerable<PromotionTransfer>> GetPromotionTransfersAsync(string? storeId)
  {
    string sql = $@"SELECT DISTINCT
                        BatchId, 
                        PromoId, 
                        Description, 
                        StartDate,
                        EndDate,
                        Trans_Type TransType
                    FROM 
                        [CSCTransfer].[dbo].[PromotionTransfer] 
                    WHERE 
                        Trans_Flag = 'O' {(storeId == null ? "" : $" AND Store_Id = '{storeId}' ")}
                    ORDER BY 
                        BatchId, PromoId;";

    var promotionTransfers = await Connection.QueryAsync<PromotionTransfer>(sql, null, Transaction);
    return promotionTransfers;
  }

  public async Task<IEnumerable<CostChangeTransferDto>> GetCostChangeTransfersAsync(string? storeId)
  {
    string sql = $@"SELECT DISTINCT
                        BatchId, 
                        ProductID AS Product, 
                        Description
                    FROM 
                        CSCTransfer.dbo.CostChangeTransfer
                    WHERE 
                        Trans_Flag = 'O' 
                        {(storeId != null ? $" AND Store_Id = '{storeId}' " : " ")}
                    ORDER BY 
                        batchid,ProductID";
    IEnumerable<CostChangeTransferDto> costChangeTransfers = await Connection.QueryAsync<CostChangeTransferDto>(sql, null, Transaction);
    return costChangeTransfers;
  }

  public async Task<IEnumerable<CostChangeTransferDto>> GetAllCostChangeTransfersAsync()
  {
    string sql = $@"SELECT
                      ProductID AS Product,
                      NewCost AS NewCost,
                      Store_id AS StoreId,
                      VendorId AS VendorId,
                      StartDate AS StartDate,
                      EndDate AS EndDate,
                      OrderNo AS OrderNo,
                      ActiveVendor AS ActiveVendor,
                      PriceChangeWithCost AS PriceChangeWithCost
                    FROM CostChangeTransfer
                    WHERE Trans_Flag = 'O' 
                    ORDER BY batchid, ProductID";
    IEnumerable<CostChangeTransferDto> costChangeTransfers = await Connection.QueryAsync<CostChangeTransferDto>(sql, null, Transaction);
    return costChangeTransfers;
  }

  public async Task<IEnumerable<Store>?> GetCommodityTransferStoresAsync(string dataTransferType)
  {
    string tableName = GetDataTransferTableName(dataTransferType);
    IEnumerable<Store>? stores = null;

    string sql = $@"SELECT DISTINCT Store_Id FROM {tableName} WHERE Trans_Flag = 'O' ORDER BY Store_Id";
    var storeIds = await Connection.QueryAsync<string>(sql, null, Transaction);

    if (storeIds != null)
    {
      string storeIdsString = $"'{string.Join("', '", storeIds)}'"; // ['001', '002'] --> "'001', '002'"
      sql = $@"SELECT Store_Id, Store_Desc FROM [CSCMaster].[dbo].[STORE] WHERE Store_Id IN ({storeIdsString})";
      stores = await Connection.QueryAsync<Store>(sql, null, Transaction);
    }

    return stores;
  }

  public async Task<IEnumerable<Store>> GetPendingTransferStoresAsync(string dataTransferType)
  {
    IEnumerable<Store>? stores = null;
    string sql = $@"SELECT Store_ID AS Store_Id,
                           STORE_DESC as STORE_DESC 
                    FROM [CSCMaster].[dbo].STORE 
                    WHERE Store_ID IN (
                        SELECT DISTINCT store_id 
                        FROM [CSCTransfer].[dbo].{GetDataTransferTableName(dataTransferType)} 
                        WHERE Trans_Flag = 'O')";
    stores = await Connection.QueryAsync<Store>(sql, null, Transaction);

    return stores;
  }

  public async Task<IEnumerable<PriceDefinition>> GetPriceDefinitions()
  {
    string sql = $@"SELECT  Batchid
                            ,ProductId
                            ,PR_Type
                            ,Store_Id StoreId
                            ,PR_Units
                            ,Price_Number PriceNumber
                            ,Price
                            ,Margin
                            ,PR_FROM_QTY
                            ,PR_TO_QTY
                            ,PR_FROM
                            ,PR_TO
                            ,OrderNo
                            ,VendorId
                            ,Trans_Date
                            ,Trans_User
                            ,Trans_Flag
                    FROM PriceDefinition 
                    WHERE trans_flag = 'O' 
                    ORDER BY BatchID, Store_id";

    var priceDefinitions = await Connection.QueryAsync<PriceDefinition>(sql, null, Transaction);
    return priceDefinitions;
  }

  public async Task<bool> CheckPriceDefinitionExists(PriceDefinition priceDefinition)
  {
    string sql = $@"SELECT 
                      BatchID,
                      ProductID,
                      PR_Type,
                      Store_ID AS StoreId,
                      PR_Units,
                      Price_Number AS PriceNumber,
                      Price,
                      Margin,
                      PR_FROM_QTY,
                      PR_TO_QTY,
                      PR_FROM,
                      PR_TO,
                      OrderNo,
                      VendorID,
                      Trans_Date,
                      Trans_User,
                      Trans_Flag
                  FROM 
                      [CSCTransfer].[dbo].[PriceDefinition]
                  WHERE 
                      Trans_Flag = 'N'
                      AND BatchID = @BatchId
                      AND PR_Type = 'R'
                      AND ProductID = @ProductId
                      AND Store_ID = @StoreId
                  ";

    var parameters = new
    {
      BatchId = priceDefinition.BatchId,
      ProductId = priceDefinition.ProductId,
      StoreId = priceDefinition.StoreId
    };

    IEnumerable<PriceDefinition> priceDefinitions = await Connection.QueryAsync<PriceDefinition>(sql, parameters, Transaction);
    return (priceDefinitions.Count() > 0);
  }

  public async Task<bool> InsertCostChangeTransferAsync(CostChangeTransfer model)
  {
    string sql = $@"INSERT INTO [CSCTransfer].[dbo].[CostChangeTransfer] ( 
                             BatchId
                            ,ProductId
                            ,Description
                            ,StartDate
                            ,EndDate
                            ,NewCost
                            ,Store_Id
                            ,VendorId
                            ,OrderNo
                            ,Trans_Date
                            ,Trans_User
                            ,Trans_Flag  
                            ,ActiveVendor)
                     VALUES (
                             @BatchId
                            ,@ProductId
                            ,@Description
                            ,@StartDate
                            ,@EndDate
                            ,@NewCost
                            ,@StoreId
                            ,@VendorId
                            ,@OrderNo
                            ,'{DateTime.Now}'
                            ,''
                            ,@TransFlag
                            ,@ActiveVendor)";

    int affectedRows = await Connection.ExecuteAsync(sql, model, Transaction);
    return affectedRows == 1;
  }

  public async Task<int> PerformPriceChangeTransfer(PricebookTransferPoliciesDto policies, string? userCode)
  {
    var parameters = new DynamicParameters();
    parameters.Add("@UpdateForVendorAll ", !policies.SetVend && policies.UpdateAllVendor);
    parameters.Add("@UserCode ", userCode);
    parameters.Add("@NumberOfPrice", policies.NumberOfPrice);

    if (_config.IsBackOffice)
    {
      IUnitOfWork _unitOfWorkCSCAdmin = new UnitOfWork(_config, _config.CSCAdminConnectionString);
      Company? company = await _unitOfWorkCSCAdmin.CompanySetups.GetCompanyAsync();
      parameters.Add("@StoreId", company.StoreCode);
      _unitOfWorkCSCAdmin.Commit();
    }

    int affectedRows = await Connection.ExecuteAsync("SProc_PerformPriceChangeTransferAll",
      parameters,
      Transaction, commandTimeout: 180, commandType: CommandType.StoredProcedure);
    return affectedRows;
  }

  public async Task<int> PerformCommodityTransfer()
  {
    string sql = $@"UPDATE CommodityTransfer 
                    SET Trans_Flag = 'N' 
                    WHERE Trans_Flag ='O'";
    var affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows;
  }

  public async Task<int> PerformHotButtonsTransfer()
  {
    string sql = $@"UPDATE HotButtonTransfer 
                    SET Trans_Flag = 'N' 
                    WHERE Trans_Flag ='O'";
    var affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows;
  }

  public async Task<int> PerformPromotionTransfer()
  {
    string sql = $@"UPDATE PromotionTransfer SET Trans_Flag = 'N' WHERE Trans_Flag ='O'";
    var affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows;
  }

  private string GetDataTransferTableName(string dataTransferType)
  {
    string tableName = "";
    switch (dataTransferType)
    {
      case DataTransferType.PriceChange:
        tableName = "PriceChangeTransfer";
        break;
      case DataTransferType.Stocks:
        tableName = "StockTransfer";
        break;
      case DataTransferType.CostChange:
        tableName = "CostChangeTransfer";
        break;
      case DataTransferType.Commodity:
        tableName = "CommodityTransfer";
        break;
      case DataTransferType.Promotions:
        tableName = "PromotionTransfer";
        break;
      case DataTransferType.HotButtons:
        tableName = "HotButtonTransfer";
        break;
      case DataTransferType.LabelPrinting:
        tableName = "LabelPrintSelected";
        break;
    }

    return tableName;
  }

  private string GetDataTransferDeleteColumnName(string dataTransferType)
  {
    string columnName = "";
    switch (dataTransferType)
    {
      case DataTransferType.PriceChange:
      case DataTransferType.Stocks:
      case DataTransferType.CostChange:
        columnName = "ProductID";
        break;
      case DataTransferType.Commodity:
        columnName = "Dept";
        break;
      case DataTransferType.Promotions:
        columnName = "PromoID";
        break;
      case DataTransferType.HotButtons:
        columnName = "Button";
        break;
    }
    return columnName;
  }

  public async Task<int> PerformCostChangeTransfer(PricebookTransferPoliciesDto policies, string? userCode)
  {
    int affectedRows = await Connection.ExecuteAsync("SProc_PerformCostChangeTransfer",
      new {
        policySetVend = policies.SetVend,
        policyUpdateAllVendor = policies.UpdateAllVendor,
        policyMarginCalcCostMkup = policies.MarginCalcCostMkup,
        policyNumPrice = policies.NumberOfPrice,
        userCode
      },
      Transaction, commandTimeout: 180, commandType: CommandType.StoredProcedure);
    return affectedRows;
  }

  public async Task<IEnumerable<PriceChangeTransfer>> GetPriceChangeTransfer(PriceChangeTransfer model)
  {
    string sql = $@"SELECT
                        BatchId,
                        ProductID AS ProductId,
                        Description,
                        PriceType,
                        Store_id AS StoreId,
                        PriceGroup_id AS PriceGroupId,
                        Region_id AS RegionId,
                        Trans_Date AS TransDate,
                        Trans_User AS TransUser,
                        Trans_Flag AS TransFlag,
                        VendorId,
                        OrderNo,
                        CurrentPrice,
                        Cost,
                    FROM 
                        CSCTransfer.dbo.PriceChangeTransfer
                    WHERE 
                        Trans_Flag = @TransFlag
                        AND BatchId = @BatchId
                        AND PriceType = @PriceType
                        AND ProductID = @ProductId
                        AND Store_ID = @StoreId";

    IEnumerable<PriceChangeTransfer> priceChangeTransfers = await Connection.QueryAsync<PriceChangeTransfer>(sql, model, Transaction);
    return priceChangeTransfers;
  }

  public async Task<int> InsertLabelPrintingTransferBatch(List<StockItemMinimal> stockItems)
  {
    StringBuilder sql = new();
    sql.AppendLine($@"INSERT INTO [dbo].[LabelPrintSelected]
                          ([Stock_Code]
                          ,[Description] )
                    VALUES ");

    string separator = "";
    foreach (var stockItem in stockItems)
    {
      sql.AppendLine(separator);
      sql.AppendLine($@"(
                  '{stockItem.StockCode}'
                  ,'{stockItem.Description.ToEncodedSqlString()}')");

      separator = ", ";
    }

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return affectedRows;
  }

  public async Task<IEnumerable<StockItemMinimal>?> GetLabelPrintsAsync()
  {
    string sql = $@"SELECT [Stock_Code] AS StockCode,
                        Description AS Description
                    FROM 
                        CSCTransfer.dbo.LabelPrintSelected";

    IEnumerable<StockItemMinimal>? labelPrints = await Connection.QueryAsync<StockItemMinimal>(sql, null, Transaction);
    return labelPrints;
  }

  public async Task<bool> DeleteLabelPrintAsync(string? stockCode)
  {
    var sql = new StringBuilder();
    sql.Append("DELETE FROM [dbo].[LabelPrintSelected] ");

    if (!string.IsNullOrEmpty(stockCode))
      sql.Append($"WHERE [Stock_Code] = '{stockCode}'");

    await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return true;
  }

  public async Task<int> DeleteLabelPrintingTransferBatch(List<StockItemMinimal> stockItems)
  {
    if (stockItems == null || !stockItems.Any())
    {
      return 0;
    }

    string sql = $@"DELETE FROM [dbo].[LabelPrintSelected] 
                    WHERE [Stock_Code] IN @StockCodes";

    var stockCodes = stockItems.Select(_ => _.StockCode).ToArray();

    int affectedRows = await Connection.ExecuteAsync(sql, new { StockCodes = stockCodes }, Transaction);
    return affectedRows;
  }
}
