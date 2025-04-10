namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StockCostRepository : BaseRepository, IStockCostRepository
{
  private readonly IApplicationConfig _applicationConfig;
  public StockCostRepository(IApplicationConfig applicationConfig, IDbTransaction transaction) : base(transaction)
  {
    _applicationConfig = applicationConfig;
  }

  #region Private Methods

  private async Task UpdateQueryStockCostInfoInList(
    List<StockCostInfoDto> list,
    List<GetStockCostInfoDto> newModels, 
    List<StockCostInfo> result,
    bool isCompareOnOrderNo)
  {
    var list2 = await QueryStockCostInfoBatchAsync(newModels);
    foreach (var item in list2)
    {
      if (item.Cost != null || item.VendorId != null)
      {
        var x = list.FirstOrDefault(val => (isCompareOnOrderNo) ? val.OrderNo == item.OrderNo : val.StoreId == item.StoreId && val.StockCode == item.StockCode);
        if (x != null)
        {
          x.Cost = item.Cost ?? 0;
          x.VendorId = item.VendorId;
          x.ActiveVendor = item.ActiveVendor ?? false;
        }
      }
    }

    for (int i = 0; i < list.Count(); i++)
    {
      if (list[i].Cost != null || list[i].VendorId != null)
      {
        result[i].Cost = list[i].Cost ?? 0;
        result[i].VendorId = list[i].VendorId;
        result[i].ActiveVendor = list[i].ActiveVendor ?? false;
      }
    }
  }

  #endregion

  public async Task<bool> AddStockCostAsync(StockCost model)
  {
    string sql = $@"INSERT INTO StockCosts (
                            Stock_Code
                           ,StoreID
                           ,VendorID
                           ,Cost
                           ,Startdate
                           ,EndDate
                           ,OrderNo
                           ,ActiveVendor)
                    VALUES (
                            @StockCode
                           ,@StoreId
                           ,@VendorId
                           ,@Cost
                           ,{(model.StartDate == null ? "NULL" : "@StartDate")}
                           ,{(model.EndDate == null ? "NULL" : "@EndDate")}
                           ,@OrderNo
                           ,@ActiveVendor) ";

    int affectedRows = await Connection.ExecuteAsync(sql, model, Transaction);
    return affectedRows == 1;
  }

  public async Task<StockCostInfo> GetStockCostInfoAsync(GetStockCostInfoDto model)
  {
    decimal? Cost = 0;
    string? VendorId = "ALL";
    string StoreId = model.StoreId;
    StringBuilder query = PrepareGetStockCostInfoQuery(model);
    StockCostInfo? stockCostInfo = await Connection.QueryFirstOrDefaultAsync<StockCostInfo?>(query.ToString(), null, Transaction);

    if (stockCostInfo != null)
    {
      Cost = stockCostInfo.Cost ?? 0;
      VendorId = stockCostInfo.VendorId;
    }
    else
    {
      model.StoreId = "ALL";
      query = PrepareGetStockCostInfoQuery(model);
      stockCostInfo = await Connection.QueryFirstOrDefaultAsync<StockCostInfo?>(query.ToString(), null, Transaction);

      if (stockCostInfo != null)
      {
        Cost = stockCostInfo.Cost ?? 0;
        VendorId = stockCostInfo.VendorId;
      }
    }

    // If future cost is 0.00, then bring the network cost
    if (Cost == 0 && model.OrderNo > 0)
    {
      model.StoreId = StoreId;
      model.OrderNo = 0;

      if (_applicationConfig.IsBackOffice)
      {
        var stockCostInfoParameterBackOffice = new StockCostInfoParameterBODto()
        {
          VendorId = VendorId,
          StockCode = model.StockCode,
          OrderNo = model.OrderNo
        };

        stockCostInfo = await GetStockCostInfoForBackOfficeAsync(stockCostInfoParameterBackOffice);
      }
      else
      {
        query = PrepareGetStockCostInfoQuery(model);
        stockCostInfo = await Connection.QueryFirstOrDefaultAsync<StockCostInfo?>(query.ToString(), null, Transaction);
      }

      if (stockCostInfo != null)
      {
        Cost = stockCostInfo.Cost ?? 0;
        VendorId = stockCostInfo.VendorId;
      }
    }

    return new StockCostInfo { ActiveVendor = stockCostInfo?.ActiveVendor, Cost = Cost, VendorId = VendorId };
  }

  public async Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatchAsync(List<GetStockCostInfoDto> models)
  {
    var result = new List<StockCostInfo>();
    foreach (var model in models)
    {
      result.Add(new StockCostInfo
      {
        Cost = 0,
        VendorId = "ALL",
        ActiveVendor = false
      });
    }
    var list = await QueryStockCostInfoBatchAsync(models);
    var newModels = new List<GetStockCostInfoDto>();
    for (int i = 0; i < models.Count(); i++)
    {
      if (list[i].Cost != null || list[i].VendorId != null)
      {
        result[i].Cost = list[i].Cost ?? 0;
        result[i].VendorId = list[i].VendorId;
        result[i].ActiveVendor = list[i].ActiveVendor ?? false;
      }
      else
      {
        models[i].StoreId = "ALL";
        newModels.Add(models[i]);
      }
    }

    await UpdateQueryStockCostInfoInList(list, newModels, result, true);

    newModels = new List<GetStockCostInfoDto>();
    for (int i = 0; i < models.Count; i++)
    {
      if (result[i].Cost == 0 && models[i].OrderNo > 0)
      {
        models[i].StoreId = list[i].StoreId;
        models[i].OrderNo = 0;
        newModels.Add(models[i]);
      }
    }

    await UpdateQueryStockCostInfoInList(list, newModels, result, false);

    return result;
  }

  private async Task<List<StockCostInfoDto>> QueryStockCostInfoBatchAsync(List<GetStockCostInfoDto> models)
  {
    if (models.Count() < 1) return new List<StockCostInfoDto>();
    using (var connection = new SqlConnection(_applicationConfig.CSCMasterConnectionString))
    {

      DataTable stockCodesTable = new DataTable();
      stockCodesTable.Columns.Add("StockCode", typeof(string));
      foreach (var model in models)
      {
        stockCodesTable.Rows.Add(model.StockCode);
      }

      var parameters = new DynamicParameters();
      parameters.Add("@StoreID", models[0].StoreId);
      parameters.Add("@StockCodes", stockCodesTable.AsTableValuedParameter("StockCodeTableType"));
      parameters.Add("@OrderNo", models[0].OrderNo);

      List<StockCostInfoDto> batchResults = new List<StockCostInfoDto>();
      foreach (var model in models)
      {
        batchResults.Add(new StockCostInfoDto
        {
          StoreId = model.StoreId,
          StockCode = model.StockCode,
          OrderNo = model.OrderNo
        });
      }

      var results = await connection.QueryAsync<StockCostInfoDto>("SProc_StockCostInfoBatch", parameters, commandType: CommandType.StoredProcedure);
      foreach (var item in results)
      {
        StockCostInfoDto x = batchResults.FirstOrDefault(value => value.StoreId == item.StoreId && value.StockCode == item.StockCode && value.OrderNo == item.OrderNo);

        if (x != null)
        {
          if (x.Cost != null || x.VendorId != null) continue;
          x.Cost = item.Cost;
          x.VendorId = item.VendorId;
          x.ActiveVendor = item.ActiveVendor ?? false;
        }
      }
      return batchResults;
    }
  }

  public async Task<StockCostInfo?> GetStockCostInfoSingleAsync(GetStockCostInfoDto model)
  {
    string sql = $"exec SProc_StockCostInfo '{model.StoreId}','{model.StockCode}',{model.OrderNo}";
    StockCostInfo? stockCostInfo = await Connection.QueryFirstOrDefaultAsync<StockCostInfo?>(sql, null, Transaction);
    return stockCostInfo;
  }

  public async Task<int> GetStockCostRecordCountAsync(GetStockCostRecordCountDto model)
  {
    string sql = $@"SELECT COUNT(Stock_Code) FROM StockCosts 
                    WHERE Stock_Code = @StockCode AND StoreId = @StoreId
                            AND OrderNO = @OrderNo AND VendorId = @VendorId ";

    int stockCostCount = await Connection.QueryFirstOrDefaultAsync<int>(sql, model, Transaction);
    return stockCostCount;
  }

  public async Task<bool> AddStockCostByStockCodeAsync(string stockCode, string baseStockCode)
  {
    string sql = (_applicationConfig.IsBackOffice) ?
      @"INSERT INTO [CSCMaster].[dbo].[StockCosts] (
          Stock_Code, 
          VendorID, 
          Cost,
          StartDate, 
          EndDate, 
          ActiveVendor, 
          OrderNo
        ) 
        SELECT 
          @StockCode AS Stock_Code, 
          VendorID, 
          Cost, 
          StartDate,
          EndDate,
          ActiveVendor,
          OrderNo 
        FROM [CSCMaster].[dbo].[StockCosts] 
        WHERE Stock_Code= @BaseStockCode" :
      @"INSERT INTO [CSCMaster].[dbo].[StockCosts] (
        Stock_Code,
        StoreID,
        VendorID, 
        Cost,
        StartDate, 
        EndDate, 
        ActiveVendor, 
        OrderNo
       ) 
        SELECT 
          @StockCode AS Stock_Code, 
          StoreID,
          VendorID, 
          Cost, 
          StartDate,
          EndDate,
          ActiveVendor,
          OrderNo 
        FROM [CSCMaster].[dbo].[StockCosts] 
        WHERE Stock_Code= @BaseStockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@BaseStockCode", SqlDbType.VarChar, 16).Value = baseStockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteStockCostByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from StockCosts
                        Where StockCosts.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  private StringBuilder PrepareGetStockCostInfoQuery(GetStockCostInfoDto model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT TOP 1 [Cost], [VendorId], [ActiveVendor]
                    FROM [CSCMaster].[dbo].[StockCosts] 
                    WHERE (activevendor = 1 or Vendorid = 'ALL')
                          AND Stock_Code = '{model.StockCode}'
                          AND OrderNo = {model.OrderNo}");

    if (!_applicationConfig.IsBackOffice)
    {
      query.Append($"AND StoreID = '{model.StoreId}'");
    }

    return query;
  }

  public async Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatchForBackOfficeAsync(StockCostInfoBatchParameterBackOfficeDto stockCostInfoParameterBackOffice)
  {
    var stockCodeTable = new DataTable();
    stockCodeTable.Columns.Add("StockCode", typeof(string));

    foreach (var stockCode in stockCostInfoParameterBackOffice.StockCodes!)
    {
      stockCodeTable.Rows.Add(stockCode);
    }

    var parameters = new
    {
      StockCodes = stockCodeTable.AsTableValuedParameter("StockCodeTableType"),
      OrderNo = stockCostInfoParameterBackOffice.OrderNo,
      VendorId = stockCostInfoParameterBackOffice.VendorId
    };

    string sql = "SProc_StockCostInfoBatch";
    IEnumerable<StockCostInfo> stockCostInfos = (await Connection.QueryAsync<StockCostInfo?>(
        sql,
        parameters,
        Transaction,
        commandType: CommandType.StoredProcedure
    )).Where(info => info != null && info.StockCode != null).Select(info => info!);

    return stockCostInfos;
  }

  public async Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatchForHeadOfficeAsync(StockCostInfoBatchParameterDto stockCostInfoParameter)
  {
    var stockCodeTable = new DataTable();
    stockCodeTable.Columns.Add("StockCodes", typeof(string));

    foreach (var stockCode in stockCostInfoParameter.StockCodes!)
    {
      stockCodeTable.Rows.Add(stockCode);
    }

    var parameters = new
    {
      StockCodes = stockCodeTable.AsTableValuedParameter("StockCodeTableType"),
      OrderNo = stockCostInfoParameter.OrderNo,
      StoreID = stockCostInfoParameter.StoreID,
    };

    string sql = "SProc_StockCostInfoBatch";
    IEnumerable<StockCostInfo> stockCostInfos = (await Connection.QueryAsync<StockCostInfo?>(
        sql,
        parameters,
        Transaction,
        commandType: CommandType.StoredProcedure
    )).Where(info => info != null && info.StockCode != null).Select(info => info!);

    return stockCostInfos;
  }

  public async Task<IEnumerable<StockPriceInfoDto>> GetStockPriceInfoBatchForBackOfficeAsync(IEnumerable<StockCostInfo> stockCostInfo)
  {
    if (stockCostInfo == null || !stockCostInfo.Any())
    {
      return Enumerable.Empty<StockPriceInfoDto>();
    }

    var stockCodeTable = new DataTable();
    stockCodeTable.Columns.Add("StockCode", typeof(string));
    stockCodeTable.Columns.Add("VendorID", typeof(string));

    foreach (var stockCost in stockCostInfo)
    {
      stockCodeTable.Rows.Add(stockCost.StockCode, stockCost.VendorId);
    }

    string storeID = stockCostInfo.FirstOrDefault()?.StoreId;

    var parameters = new
    {
      StockCodeList = stockCodeTable.AsTableValuedParameter("StockCodeAndVendorIDTableType"), // change param name
      PriceNum = 1,
      OrderNo = 0,
      StoreID = storeID
  };

    string sql = "SProc_StockPriceInfoBatchForDetailReport";
    IEnumerable<StockPriceInfoDto> stockCostInfos = (await Connection.QueryAsync<StockPriceInfoDto?>(
        sql,
        parameters,
        Transaction,
        commandType: CommandType.StoredProcedure
    )).Where(info => info != null && info.StockCode != null).Select(info => info!);

    return stockCostInfos;
  }

  public async Task<IEnumerable<StockTax>> GetStockTaxInfoBatchForBackOfficeAsync(List<string> stockCodes, string storeID)
  {
    var stockCodeTable = new DataTable();
    stockCodeTable.Columns.Add("StockCode", typeof(string));

    foreach (var stockCode in stockCodes)
    {
      stockCodeTable.Rows.Add(stockCode);
    }

    var parameters = new
    {
      StockCodeList = stockCodeTable.AsTableValuedParameter("StockCodeTableType"),
      StoreID  = storeID,
    };

    string sql = "SProc_StockTaxInfoBatch";
    IEnumerable<StockTax> stockCostInfos = (await Connection.QueryAsync<StockTax?>(
        sql,
        parameters,
        Transaction,
        commandType: CommandType.StoredProcedure
    )).Where(info => info != null && info.StockCode != null).Select(info => info!);

    return stockCostInfos;
  }

  public async Task<IEnumerable<StockCharge>> GetStockChargeInfoBatchForBackOfficeAsync(List<string> stockCodes, string storeID)
  {
    var stockCodeTable = new DataTable();
    stockCodeTable.Columns.Add("StockCode", typeof(string));

    foreach (var stockCode in stockCodes)
      stockCodeTable.Rows.Add(stockCode);

    var parameters = new
    {
      StockCodeList = stockCodeTable.AsTableValuedParameter("StockCodeTableType"),
      StoreID = storeID,
    };

    string sql = "SProc_StockChargeInfoBatch";
    IEnumerable<StockCharge> stockCostInfos = (await Connection.QueryAsync<StockCharge?>(
        sql,
        parameters,
        Transaction,
        commandType: CommandType.StoredProcedure
    )).Where(info => info != null && info.StockCode != null).Select(info => info!);

    return stockCostInfos;
  }

  public async Task<IEnumerable<StockCostInfo>> GetStockCostInfoByMultipleVendorBatchForBackOfficeAsync(GetStockCostInfoByMultipleVendorBatchBackOfficeDto model)
  {
    var dataTable = new DataTable();

    dataTable.Columns.Add("StockCode", typeof(string));
    dataTable.Columns.Add("VendorId", typeof(string));

    model.StockCodesAndVendorIds.ForEach(t => dataTable.Rows.Add(t.stockCode, t.vendorId));
    var parameters = new
    {
      StockCodesAndVendorIds = dataTable.AsTableValuedParameter("StockCodeAndVendorIdTableType"),
      OrderNo = model.OrderNo,
    };

    string sql = "SProc_StockCostInfoByMultipleVendorBatch";

    IEnumerable<StockCostInfo> stockCostInfos = (await Connection.QueryAsync<StockCostInfo?>(
          sql,
          parameters,
          Transaction,
          commandType: CommandType.StoredProcedure
      )).Where(info => info != null && info.StockCode != null && info.VendorId != null)
        .Select(info => info!);

    return stockCostInfos;
  }

  public async Task<IEnumerable<StockCostInfo>> GetStockCostInfoByMultipleVendorBatchPBMAsync(GetStockCostInfoByMultipleVendorBatchBackOfficeDto model, string selectedVendor)
  {
    var dataTable = new DataTable();

    dataTable.Columns.Add("StockCode", typeof(string));

    model.StockCodesAndVendorIds.ForEach(t => dataTable.Rows.Add(t.stockCode));
    var parameters = new
    {
      StockCodes = dataTable.AsTableValuedParameter("StockCodeTableType"),
      OrderNo = model.OrderNo,
      VendorID = selectedVendor
    };

    string sql = "SProc_StockCostInfoBatch";

    List<StockCostInfo> stockCostInfos = (await Connection.QueryAsync<StockCostInfo?>(
        sql,
        parameters,
        Transaction,
        commandType: CommandType.StoredProcedure
    )).Where(info => info != null && info.StockCode != null && info.VendorId != null)
      .Select(info => info!).ToList();

    Dictionary<string, string?> stockCodeToActiveVendor = stockCostInfos
        .Where(s => !string.IsNullOrEmpty(s.StockCode))
        .GroupBy(s => s.StockCode)
        .ToDictionary(g => g.Key!, g => g.FirstOrDefault()?.VendorId);

    List<StockCostInfo> result = model.StockCodesAndVendorIds
        .Select(scv => new StockCostInfo { StockCode = scv.stockCode })
        .ToList();

    foreach (var item in result)
    {
      if (item.Cost != null)
        continue;

      var stockCostInfo = stockCostInfos.FirstOrDefault(r => r.StockCode == item.StockCode);

      if (stockCostInfo != null)
        item.Cost = stockCostInfo.Cost;
    }

    // If future cost is 0.00, then bring the network cost
    if (model.OrderNo > 0)
    {
      sql = "SProc_StockCostInfoByMultipleVendorBatch";
      var dataTableForDifferentVendors = new DataTable();

      dataTableForDifferentVendors.Columns.Add("StockCode", typeof(string));
      dataTableForDifferentVendors.Columns.Add("VendorId", typeof(string));

      model.StockCodesAndVendorIds.ForEach(t =>
      {
        string? vendorId = stockCodeToActiveVendor.ContainsKey(t.stockCode)
                          ? stockCodeToActiveVendor[t.stockCode]
                          : t.vendorId;

        dataTableForDifferentVendors.Rows.Add(t.stockCode, vendorId);
      });

      var parameterForDifferentVendors = new
      {
        StockCodesAndVendorIds = dataTableForDifferentVendors.AsTableValuedParameter("StockCodeAndVendorIdTableType"),
        OrderNo = 0,
      };

      List<StockCostInfo> stockCostInfosForOrderNo0 = (await Connection.QueryAsync<StockCostInfo?>(
           sql,
           parameterForDifferentVendors,
           Transaction,
           commandType: CommandType.StoredProcedure
       )).Where(info => info != null && info.StockCode != null && info.VendorId != null)
         .Select(info => info!).ToList();

      foreach (var item in result)
      {
        var stockCostInfo = stockCostInfosForOrderNo0.FirstOrDefault(r => r.StockCode == item.StockCode);

        if (stockCostInfo != null && stockCostInfo.Cost != null && (item.Cost == 0 || item.Cost == null))
          item.Cost = stockCostInfo.Cost;
      }
    }

    return result.AsEnumerable();
  }

  public async Task<StockCostInfo?> GetStockCostInfoForBackOfficeAsync(StockCostInfoParameterBODto stockCostInfoParameterBackOffice)
  {
    string sql = "SProc_StockCostInfo";
    StockCostInfo? stockCostInfo = await Connection.QueryFirstOrDefaultAsync<StockCostInfo?>(
        sql,
        stockCostInfoParameterBackOffice,
        Transaction,
        commandType: CommandType.StoredProcedure
    );

    return stockCostInfo;
  }

  public async Task<string> GetVendorIDByStockCodeAsync(string stockCode, string activeVendor, string orderNo)
  {
    string query = $@"SELECT
                            VendorID
                        FROM
                            [CSCMaster].[dbo].[StockCosts]
                        WHERE
                            Stock_Code = @StockCode
                            AND ActiveVendor = @ActiveVendor
                            AND OrderNo = @OrderNo";
    var parameters = new { StockCode = stockCode, ActiveVendor = activeVendor, OrderNo = orderNo };
    string stockCosts = await Connection.QueryFirstOrDefaultAsync<string>(query, parameters, Transaction);
    return stockCosts;
  }

  public async Task<bool> UpdateCostAsync(double stockCost, string stockCode, string orderNo, string vendorID)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[StockCosts]
                        SET
                            Cost = @StockCost
                        WHERE
                            Stock_Code = @StockCode
                            AND VendorID = @VendorID
                            AND OrderNo = @OrderNo";
    var parameters = new { StockCost = stockCost, StockCode = stockCode, OrderNo = orderNo, VendorID = vendorID };
    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateOrInsertStockCostAsync(double stockCost, string stockCode, string orderNo, string vendorID)
  {
    string query = @"
        MERGE INTO [CSCMaster].[dbo].[StockCosts] AS Target
        USING (SELECT @StockCode AS StockCode, @VendorID AS VendorID, @OrderNo AS OrderNo) AS Source
        ON Target.Stock_Code = Source.StockCode 
            AND Target.VendorID = Source.VendorID
            AND Target.OrderNo = Source.OrderNo
        WHEN MATCHED THEN
            UPDATE SET Target.Cost = @StockCost
        WHEN NOT MATCHED THEN
            INSERT (Stock_Code, VendorID, Cost, OrderNo)
            VALUES (@StockCode, @VendorID, @StockCost, @OrderNo);";
    var parameters = new { StockCost = stockCost, StockCode = stockCode, OrderNo = orderNo, VendorID = vendorID };
    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<StockCost>> GetStockCodesWithNoNetworkValueAsync()
  {
    const string sql = @"
      SELECT DISTINCT 
        SC.Stock_Code AS StockCode,
        SM.Descript AS Description
	    FROM [CSCMaster].[dbo].[StockCosts] AS SC
	    INNER JOIN [CSCMaster].[dbo].[StockMst] AS SM
	    ON SC.Stock_Code = SM.Stock_Code 
	    WHERE
		    VendorID != 'ALL' AND
		    SC.Stock_Code NOT IN (SELECT Stock_Code FROM [CSCMaster].[dbo].[StockCosts] WHERE VendorID = 'ALL')";

    return await Connection.QueryAsync<StockCost>(sql, null, Transaction);
  }

  public async Task<IEnumerable<StockCost>> GetStockCodesWithNetworkVendorSettingAsync()
  {
    const string sql = @"
      SELECT
        SM.Stock_Code AS StockCode,
        SM.Descript AS Description,
        SC.StoreID AS StoreId
      FROM [CSCMaster].[dbo].[StockMst] AS SM
      INNER JOIN [CSCMaster].[dbo].[StockCosts] AS SC
        ON SM.Stock_Code = SC.Stock_Code
      WHERE SC.VendorID = 'ALL'";

    return await Connection.QueryAsync<StockCost>(sql, null, Transaction);
  }

  public async Task<IEnumerable<StockCost>> GetStockCodesWithNoVendorSettingAsync()
  {
    const string sql = @"
      SELECT
        SM.Stock_Code AS StockCode,
        SM.Descript AS Description
      FROM [CSCMaster].[dbo].[StockMst] AS SM
      LEFT JOIN [CSCMaster].[dbo].[StockCosts] AS SC
	      ON SM.Stock_Code = SC.Stock_Code
      WHERE SC.Stock_Code = 'ALL'";

    return await Connection.QueryAsync<StockCost>(sql, null, Transaction);
  }
}
