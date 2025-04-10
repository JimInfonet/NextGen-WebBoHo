namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StockPriceRepository : BaseRepository, IStockPriceRepository
{
  private readonly IApplicationConfig _config;
  public StockPriceRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  #region Private Methods

  private async Task<StockPriceInfo> GetStockPriceAndMargin(GetStockPriceInfoDto model, bool isPrice)
  {
    decimal? Price = 0;
    decimal? Margin = 0;
    string StoreId = model.StoreId;
    short OrderNo = (short)model.OrderNo;
    StringBuilder query = PrepareGetStockPriceInfoQuery(model);
    StockPriceInfo? stockPriceInfo = await Connection.QueryFirstOrDefaultAsync<StockPriceInfo?>(query.ToString(), null, Transaction);

    if (stockPriceInfo != null)
    {
      Price = stockPriceInfo.Price;
      Margin = stockPriceInfo.Margin ?? 0;
    }
    else
    {
      model.StoreId = "ALL";
      query = PrepareGetStockPriceInfoQuery(model);
      stockPriceInfo = await Connection.QuerySingleOrDefaultAsync<StockPriceInfo?>(query.ToString(), null, Transaction);

      if (stockPriceInfo != null)
      {
        Price = stockPriceInfo.Price;
        Margin = stockPriceInfo.Margin ?? 0;
      }
    }

    // If future price is 0.00, then bring the regular price
    if (Price == 0 && model.OrderNo > 0)
    {
      model.StoreId = StoreId;
      model.OrderNo = 0;
      query = PrepareGetStockPriceInfoQuery(model);
      stockPriceInfo = await Connection.QuerySingleOrDefaultAsync<StockPriceInfo?>(query.ToString(), null, Transaction);

      if (stockPriceInfo != null)
      {
        Price = stockPriceInfo.Price;
        Margin = stockPriceInfo.Margin ?? 0;
      }
      else if (isPrice)
      {
        model.StoreId = "ALL";
        query = PrepareGetStockPriceInfoQuery(model);
        stockPriceInfo = await Connection.QuerySingleOrDefaultAsync<StockPriceInfo?>(query.ToString(), null, Transaction);

        if (stockPriceInfo != null)
        {
          Margin = stockPriceInfo.Margin ?? 0;
        }
      }
    }
    short finalOrderNo = (short)model.OrderNo;
    model.StoreId = StoreId;
    model.OrderNo = OrderNo;

    return new StockPriceInfo { Price = (decimal)Price, Margin = Margin, OrderNo = finalOrderNo };
  }

  #endregion

  public async Task<bool> AddStockPriceAsync(StockPrice model)
  {
    string sql = $@"INSERT INTO Stock_Prices (
                            Stock_Code
                           ,Price_Number
                           ,Price
                           ,Margin
                           {(_config.IsBackOffice ? " " : " ,StoreID ")}
                           ,VendorID
                           ,Startdate
                           ,EndDate
                           ,OrderNo)
                    VALUES (
                            @StockCode
                           ,@PriceNumber
                           ,@Price
                           ,@Margin
                           {(_config.IsBackOffice ? " " : " ,@StoreId ")}
                           ,@VendorId
                           ,{(model.StartDate == null ? "NULL" : "@StartDate")}
                           ,{(model.EndDate == null ? "NULL" : "@EndDate")}
                           ,@OrderNo) ";

    int affectedRows = await Connection.ExecuteAsync(sql, model, Transaction);
    return affectedRows == 1;
  }

  public async Task<bool> AddStockPriceListAsync(List<StockPrice> models)
  {
    string sql = $@"INSERT INTO Stock_Prices (
                            Stock_Code
                           ,Price_Number
                           ,Price
                           ,Margin
                           {(_config.IsBackOffice ? " " : " ,StoreID ")}
                           ,VendorID
                           ,Startdate
                           ,EndDate
                           ,OrderNo)
                    VALUES (
                            @StockCode
                           ,@PriceNumber
                           ,@Price
                           ,@Margin
                           {(_config.IsBackOffice ? " " : " ,@StoreId ")}
                           ,@VendorId
                           ,@StartDate
                           ,@EndDate
                           ,@OrderNo) ";

    int affectedRows = await Connection.ExecuteAsync(sql, models, Transaction, 180);
    return affectedRows == 1;
  }

  public async Task<int> AddSpecialPriceAsync(StockSpecialPrice model)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[PRICEL] 
                      (
                           [STOCK_CODE]
                          ,[PR_F_QTY]
                          ,[PR_T_QTY]
                          ,[PRICE]
                          ,[Pr_Type]
                          ,[Pr_Units] 
                          ,[StoreID]
                          ,[VendorID] 
                          ,[StartDate] 
                          ,[EndDate] 
                          ,[OrderNo]
                      )
                      VALUES 
                      (
                           @StockCode
                          ,@PriceFromQuantity
                          ,@PriceToQuantity
                          ,@Price
                          ,@PriceType
                          ,@PriceUnits
                          ,@StoreID
                          ,@VendorID
                          ,@StartDate
                          ,@EndDate
                          ,@OrderNo
                      )";

    int affectedRows = await Connection.ExecuteAsync(sql, model, Transaction);
    return affectedRows;
  }

  public async Task<IEnumerable<StockPriceVendors>?> GetVendorsNotInStockCost(StockPriceModel model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  DISTINCT 'ALL' AS Code, 'ALL' AS Name, ISNULL(S.ActiveVendor, 0) AS ActiveVendor, P.PartNo
                      FROM    
                      ( 
			                  SELECT	VendorId, Stock_Code, ActiveVendor 
                        FROM    [CSCMaster].[dbo].[StockCosts] 
			                  WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
												        RTRIM(LTRIM(VendorId)) = 'ALL' AND
                                {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND" : "")}
					                      OrderNo = {model.OrderNo}
		                  ) AS S
		                  LEFT JOIN 
                      (
			                  SELECT * FROM VendorParts WHERE RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}'
		                  ) P ON RTRIM(LTRIM(S.Stock_Code)) = RTRIM(LTRIM(P.Stock_Code)) AND RTRIM(LTRIM(S.VendorID)) = RTRIM(LTRIM(P.VendorId))

                      UNION ALL

                      SELECT  DISTINCT RTRIM(LTRIM(V.Code)) AS Code, V.Name AS Name, ISNULL(S.ActiveVendor, 0) AS ActiveVendor, P.PartNo
                      FROM    [CSCMaster].[dbo].[Vendors] AS V
                              LEFT JOIN 
		                          ( 
			                          SELECT	VendorId, Stock_Code, ActiveVendor FROM [CSCMaster].[dbo].[StockCosts] 
			                          WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                                        {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND" : "")}
					                              OrderNo = {model.OrderNo}
		                          ) AS S ON V.Code = S.VendorID
		                          LEFT JOIN (
			                          SELECT * FROM VendorParts WHERE RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}'
		                          ) P ON RTRIM(LTRIM(S.Stock_Code)) = RTRIM(LTRIM(P.Stock_Code)) AND RTRIM(LTRIM(S.VendorID)) = RTRIM(LTRIM(P.VendorId))
                      WHERE   ISNULL(S.Stock_Code, '') = ''");

    return await Connection.QueryAsync<StockPriceVendors>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<StockPriceVendors>> GetVendors(StockPriceModel model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  DISTINCT 'ALL' AS Code, 'ALL' AS Name, ISNULL(S.ActiveVendor, 0) AS ActiveVendor, P.PartNo
                      FROM    
                      ( 
			                  SELECT	VendorId, Stock_Code, ActiveVendor FROM [CSCMaster].[dbo].[StockCosts] 
			                  WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
									              RTRIM(LTRIM(VendorId)) = 'ALL' AND
                                {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND" : "")}
					                      OrderNo = {model.OrderNo}
		                  )AS S
		                  LEFT JOIN (
			                  SELECT * FROM VendorParts WHERE RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}'
		                  ) P ON RTRIM(LTRIM(S.Stock_Code)) = RTRIM(LTRIM(P.Stock_Code)) AND RTRIM(LTRIM(S.VendorID)) = RTRIM(LTRIM(P.VendorId))

                      UNION ALL

                      SELECT  DISTINCT RTRIM(LTRIM(V.Code)) AS Code, V.Name AS Name, ISNULL(S.ActiveVendor, 0) AS ActiveVendor, P.PartNo
                      FROM    [CSCMaster].[dbo].[Vendors] AS V
                              INNER JOIN 
		                          ( 
			                          SELECT	VendorId, Stock_Code, ActiveVendor FROM [CSCMaster].[dbo].[StockCosts] 
			                          WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                                        {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND" : "")}
					                              OrderNo = {model.OrderNo}
		                          ) AS S ON V.Code = S.VendorID
		                          LEFT JOIN (
			                          SELECT * FROM VendorParts WHERE RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}'
		                          ) P ON RTRIM(LTRIM(S.Stock_Code)) = RTRIM(LTRIM(P.Stock_Code)) AND RTRIM(LTRIM(S.VendorID)) = RTRIM(LTRIM(P.VendorId))");

    return await Connection.QueryAsync<StockPriceVendors>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<StockCosts>> GetCosts(StockPriceModel model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM(Stock_Code)) AS StockCode, RTRIM(LTRIM(VendorID)) AS VendorID, Cost, StartDate, EndDate, OrderNo, ActiveVendor, UpdateDate
                      FROM    [CSCMaster].[dbo].[StockCosts]
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.QueryAsync<StockCosts>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<StockPrice>> GetPrices(StockPriceModel model, int pricesInUse, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM(Stock_Code)) AS StockCode, Price_Number AS PriceNumber, {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID))  AS StoreID, " : "")} Price, RTRIM(LTRIM(VendorID)) AS VendorID, StartDate, EndDate, OrderNo, Margin, UpdateDate
                      FROM    [CSCMaster].[dbo].Stock_Prices
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              Price_Number <= {pricesInUse} AND
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo} ORDER BY Price_Number");

    return await Connection.QueryAsync<StockPrice>(query.ToString(), null, Transaction);
  }

  public async Task<StockPriceInfo> GetStockPriceInfoAsync(GetStockPriceInfoDto model)
  {
    decimal? Price;
    decimal? Margin;

    StockPriceInfo stockPriceInfo = await GetStockPriceAndMargin(model, true);
    Price = stockPriceInfo.Price;
    Margin = stockPriceInfo.Margin;

    if (stockPriceInfo.OrderNo > 0)
    {
      model.OrderNo = 0; // Hardcoded in vb code for calculating margin
      stockPriceInfo = await GetStockPriceAndMargin(model, false);
      Margin = stockPriceInfo.Margin;
    }

    return new StockPriceInfo { Price = (decimal)Price, Margin = Margin };
  }

  public async Task<IEnumerable<StockPriceInfo>> GetStockPriceInfoBatchAsync(List<GetStockPriceInfoDto> models)
  {
    var result = new List<StockPriceInfo>();
    foreach (var model in models)
    {
      result.Add(new StockPriceInfo
      {
        Price = 0,
        Margin = 0
      });
    }
    var stockPriceInfoList = await QueryStockCostInfoBatchAsync(models);
    var newModels = new List<GetStockPriceInfoDto>();
    for (int i = 0; i < models.Count(); i++)
    {
      if (stockPriceInfoList[i].Price != null || stockPriceInfoList[i].Margin != null)
      {
        result[i].Price = stockPriceInfoList[i].Price ?? 0;
        result[i].Margin = stockPriceInfoList[i].Margin ?? 0;
      }
      else
      {
        models[i].StoreId = "ALL";
        newModels.Add(models[i]);
      }
    }

    var stockPriceInfoTempList = await QueryStockCostInfoBatchAsync(newModels);
    foreach (StockPriceInfoDto item in stockPriceInfoTempList)
    {
      if (item.Price != null || item.Margin != null)
      {
        var x = stockPriceInfoList.FirstOrDefault(val => val.StockCode == item.StockCode && val.PriceNumber == item.PriceNumber
        && val.VendorId == item.VendorId && val.OrderNo == item.OrderNo);
        if (x != null)
        {
          x.Price = item.Price;
          x.Margin = item.Margin ?? 0;
        }
      }
    }

    for (int i = 0; i < stockPriceInfoList.Count(); i++)
    {
      if (stockPriceInfoList[i].Price != null || stockPriceInfoList[i].Margin != null)
      {
        result[i].Price = stockPriceInfoList[i].Price ?? 0;
        result[i].Margin = stockPriceInfoList[i].Margin ?? 0;
      }
    }

    newModels = new List<GetStockPriceInfoDto>();
    for (int i = 0; i < models.Count; i++)
    {
      if (result[i].Price == 0 && models[i].OrderNo > 0)
      {
        models[i].StoreId = stockPriceInfoList[i].StoreId;
        models[i].OrderNo = 0;
        newModels.Add(models[i]);
      }
    }

    stockPriceInfoTempList = await QueryStockCostInfoBatchAsync(newModels);
    foreach (StockPriceInfoDto item in stockPriceInfoTempList)
    {
      if (item.Price != null || item.Margin != null)
      {
        var x = stockPriceInfoList.FirstOrDefault(val => val.StoreId == item.StoreId && val.StockCode == item.StockCode &&
        val.PriceNumber == item.PriceNumber && val.VendorId == item.VendorId);
        if (x != null)
        {
          x.Price = item.Price;
          x.Margin = item.Margin ?? 0;
        }
      }
    }

    for (int i = 0; i < stockPriceInfoList.Count(); i++)
    {
      if (stockPriceInfoList[i].Price != null || stockPriceInfoList[i].Margin != null)
      {
        result[i].Price = stockPriceInfoList[i].Price ?? 0;
        result[i].Margin = stockPriceInfoList[i].Margin ?? 0;
      }
    }

    //According to VB code For the models where margin is 0 we need to get data using OrderNo = 0.
    newModels = new List<GetStockPriceInfoDto>();
    for (int i = 0; i < models.Count; i++)
    {
      if ((result[i].Margin == 0) && models[i].OrderNo > 0)
      {
        models[i].StoreId = stockPriceInfoList[i].StoreId;
        models[i].OrderNo = 0;
        newModels.Add(models[i]);
      }
    }

    stockPriceInfoTempList = await QueryStockCostInfoBatchAsync(newModels);
    foreach (StockPriceInfoDto item in stockPriceInfoTempList)
    {
      if (item.Margin != null)
      {
        var x = stockPriceInfoList.FirstOrDefault(val => val.StoreId == item.StoreId && val.StockCode == item.StockCode &&
        val.PriceNumber == item.PriceNumber && val.VendorId == item.VendorId);
        if (x != null)
        {
          x.Margin = item.Margin ?? 0;
        }
      }
    }

    for (int i = 0; i < stockPriceInfoList.Count(); i++)
    {
      if (stockPriceInfoList[i].Price != null || stockPriceInfoList[i].Margin != null)
      {
        result[i].Price = stockPriceInfoList[i].Price ?? 0;
        result[i].Margin = stockPriceInfoList[i].Margin ?? 0;
      }
    }

    //According to VB code For the models where margin is 0 we need to get data using storeId = ALL and OrderNo = 0.
    newModels = new List<GetStockPriceInfoDto>();
    for (int i = 0; i < models.Count; i++)
    {
      if (result[i].Margin == 0)
      {
        models[i].StoreId = "ALL";
        models[i].OrderNo = 0;
        newModels.Add(models[i]);
      }
    }

    stockPriceInfoTempList = await QueryStockCostInfoBatchAsync(newModels);
    foreach (StockPriceInfoDto item in stockPriceInfoTempList)
    {
      if (item.Margin != null)
      {
        var x = stockPriceInfoList.FirstOrDefault(val => val.StockCode == item.StockCode &&
        val.PriceNumber == item.PriceNumber && val.VendorId == item.VendorId);
        if (x != null)
        {
          x.Margin = item.Margin ?? 0;
        }
      }
    }

    for (int i = 0; i < stockPriceInfoList.Count(); i++)
    {
      if (stockPriceInfoList[i].Price != null || stockPriceInfoList[i].Margin != null)
      {
        result[i].Price = stockPriceInfoList[i].Price ?? 0;
        result[i].Margin = stockPriceInfoList[i].Margin;
      }
    }
    return result;
  }

  private async Task<List<StockPriceInfoDto>> QueryStockCostInfoBatchAsync(List<GetStockPriceInfoDto> models)
  {
    if (models.Count() < 1) return new List<StockPriceInfoDto>();
    using (var connection = new SqlConnection(_config.CSCMasterConnectionString))
    {
      DataTable stockCodesTable = new DataTable();
      stockCodesTable.Columns.Add("StockCodeTemp", typeof(string));
      stockCodesTable.Columns.Add("VendorIdTemp", typeof(string));
      foreach (var model in models)
      {
        stockCodesTable.Rows.Add(model.StockCode, model.VendorId);
      }

      var parameters = new DynamicParameters();

      if (!_config.IsBackOffice)
        parameters.Add("@StoreID", models[0].StoreId);

      parameters.Add("@StockPriceTemp", stockCodesTable.AsTableValuedParameter("StockPriceTableType"));
      parameters.Add("@PriceNum", models[0].PriceNumber);
      parameters.Add("@OrderNo", models[0].OrderNo);

      List<StockPriceInfoDto> batchResults = new List<StockPriceInfoDto>();
      foreach (var model in models)
      {
        batchResults.Add(new StockPriceInfoDto
        {
          StoreId = model.StoreId,
          StockCode = model.StockCode,
          VendorId = model.VendorId,
          PriceNumber = (short)model.PriceNumber,
          OrderNo = (short)model.OrderNo
        });
      }
      var results = await connection.QueryAsync<StockPriceInfoDto>("SProc_StockPriceInfoBatch", parameters, commandType: CommandType.StoredProcedure);

      foreach (var item in results)
      {
        if (_config.IsBackOffice)
        {
          var matchingItems = batchResults.Where(value =>
              value.StockCode == item.StockCode &&
              value.VendorId == item.VendorId &&
              value.PriceNumber == item.PriceNumber &&
              value.OrderNo == item.OrderNo);

          foreach (var x in matchingItems)
          {
            x.Price = item.Price;
            x.Margin = item.Margin;
          }
        }
        else
        {
          StockPriceInfoDto x = batchResults.FirstOrDefault(value => (value.StoreId == item.StoreId) &&
            value.StockCode == item.StockCode && value.VendorId == item.VendorId &&
            value.PriceNumber == item.PriceNumber && value.OrderNo == item.OrderNo);

          if (x != null)
          {
            x.Price = item.Price;
            x.Margin = item.Margin;
          }
        }
      }

      return batchResults;
    }
  }

  public async Task<IEnumerable<StockPriceInfo>> GetStockPriceInfoListAsync(GetStockPriceInfoDto model)
  {
    object parameters;

    if (_config.IsBackOffice)
    {
      parameters = new
      {
        StockCode = model.StockCode,
        Vendorid = model.VendorId,
        PriceNum = model.PriceNumber,
        OrderNo = model.OrderNo
      };
    }
    else
    {
      parameters = new
      {
        StoreID = model.StoreId,
        StockCode = model.StockCode,
        Vendorid = model.VendorId,
        PriceNum = model.PriceNumber,
        OrderNo = model.OrderNo
      };
    }
    IEnumerable<StockPriceInfo> stockPriceInfos = await Connection.QueryAsync<StockPriceInfo>
      ("SProc_StockPriceInfo", parameters, Transaction, commandType: CommandType.StoredProcedure);
    return stockPriceInfos;
  }

  public async Task<IEnumerable<StockPrice>> GetStockPriceListAsync(GetStockPriceInfoDto model)
  {
    StringBuilder query = PrepareGetStockPriceInfoQuery(model);
    IEnumerable<StockPrice>? stockPriceInfos = await Connection.QueryAsync<StockPrice?>(query.ToString(), null, Transaction);
    return stockPriceInfos;
  }

  public async Task<IEnumerable<StockRebates>> GetRebate(string? stockCode, int orderNo, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM(Stock_Code)) AS StockCode, RebateNo, {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) AS StoreID," : "")} RTRIM(LTRIM(VendorID)) AS VendorID, Rebate, StartDate, EndDate, OrderNo
                      FROM    [CSCMaster].[dbo].StockRebates
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{stockCode}' AND
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {orderNo} ORDER BY RebateNo");

    return await Connection.QueryAsync<StockRebates>(query.ToString(), null, Transaction);
  }

  public async Task<StockPricePromotion> GetPromotion(string stockCode)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM(A.PromoID)) AS PromotionId, RTRIM(LTRIM(A.Stock_Code)) AS StockCode, A.Dept, A.Sub_Dept AS SubDept, A.SubDetail, EndDate
                      FROM    [CSCMaster].[dbo].PromoDetail AS A 
                              INNER JOIN [CSCMaster].[dbo].PromoHeader AS B ON A.PromoID = B.PromoID
                      WHERE   EndDate >= GETDATE() AND
                              RTRIM(LTRIM(A.Stock_Code)) = '{stockCode}'");

    return await Connection.QuerySingleOrDefaultAsync<StockPricePromotion>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<StockSpecialPrice>> GetSpecialPriceFromDiskKL(StockPriceModel model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT 
	                      LTRIM(RTRIM(Stock_Code)) AS StockCode, 
	                      Discount AS Price, 
	                      CustType AS PriceFromQuantity, 
	                      PriceType FROM [DISC_KL] 
                      WHERE Stock_Code = '{model.StockCode}'
                      ORDER BY CustType");

    return await Connection.QueryAsync<StockSpecialPrice>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<StockSpecialPrice>> GetSpecialPrice(StockPriceModel model, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM(Stock_Code)) AS StockCode, PR_F_QTY AS PriceFromQuantity, PR_T_QTY AS PriceToQuantity, Price, Pr_Type AS PriceType,
                              Pr_Units AS PriceUnits, RTRIM(LTRIM(VendorID)) AS VendorID, StartDate, EndDate, OrderNo
                              {(!_config.IsBackOffice ? ", RTRIM(LTRIM(StoreID)) AS StoreID" : "")}
                      FROM    [CSCMaster].[dbo].PriceL 
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo} ORDER BY PR_F_QTY");

    return await Connection.QueryAsync<StockSpecialPrice>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<StockCosts>> GetStockCosts(StockPriceModel model, string storeIDs,
    string vendorIDs = "")
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM(Stock_Code)) AS StockCode, {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) AS StoreID," : "")} RTRIM(LTRIM(VendorID)) AS VendorID, Cost, StartDate, EndDate, OrderNo, ActiveVendor, UpdateDate
                      FROM    [CSCMaster].[dbo].[StockCosts]
                      WHERE   Stock_Code = '{model.StockCode}' AND
                              VendorID IN ({(string.IsNullOrWhiteSpace(model.VendorID) ?
                            $@"'ALL'" : string.IsNullOrWhiteSpace(vendorIDs) ?
                            $@"'{model.VendorID}'" : $@"{vendorIDs}")}) AND
                              {(!_config.IsBackOffice ? $@"StoreID IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.QueryAsync<StockCosts>(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteStockCostsForVendor(StockPriceModel model, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM [CSCMaster].[dbo].StockCosts 
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND 
                              RTRIM(LTRIM(VendorID)) ='{model.VendorID}' AND 
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteStockRebatesForVendor(StockPriceModel model, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM [CSCMaster].[dbo].StockRebates 
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND 
                              RTRIM(LTRIM(VendorID)) ='{model.VendorID}' AND 
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteStockPricesForVendor(StockPriceModel model, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM [CSCMaster].[dbo].Stock_Prices 
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND 
                              RTRIM(LTRIM(VendorID)) ='{model.VendorID}' AND 
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteSpecialPriceForVendor(StockPriceModel model, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM [CSCMaster].[dbo].PriceL 
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND 
                              RTRIM(LTRIM(VendorID)) ='{model.VendorID}' AND 
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteVendorpart(StockPriceModel model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM [CSCMaster].[dbo].VendorParts 
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND 
                              RTRIM(LTRIM(VendorID)) ='{model.VendorID}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateProductTaxExemptForVendor(StockPriceModel model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE [CSCMaster].[dbo].ProductTaxExempt SET TEVendor = NULL 
                      WHERE   RTRIM(LTRIM(ProductKey)) = '{model.StockCode}' AND 
                              RTRIM(LTRIM(TEVendor)) ='{model.VendorID}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockMasterForVendor(StockPriceModel model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE [CSCMaster].[dbo].StockMst SET Vendor = 'ALL' 
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> InsertStockCostsBatch(StockCosts model, List<string>? vendors,
    List<string>? stores, bool setVendorActive = false)
  {
    if (vendors == null) return 0;

    if (stores != null && stores.Count == 0)
    {
      stores = new List<string> { "ALL" };
    }

    StringBuilder query = new();
    string commaSeparator = string.Empty;
    query.Append($@"INSERT [CSCMaster].[dbo].[StockCosts] (
                             Stock_Code, 
                             {(_config.IsBackOffice ? "" : "StoreID, ")}
                             VendorID, 
                             Cost, 
                             {(model.StartDate != null ? $@"StartDate, " : "")}
                             {(model.EndDate != null ? $@"EndDate, " : "")}
                             OrderNo 
                             {(setVendorActive ? $@", ActiveVendor" : "")})");

    foreach (var vendor in vendors)
    {
      if (!_config.IsBackOffice)
      {
        foreach (var store in stores)
        {
          query.Append(commaSeparator);
          query.Append($@" VALUES (
                           '{model.StockCode}',
                           '{store}',
                           '{vendor}',
                           {model.Cost},
                           {(model.StartDate != null ? $@"'{model.StartDate}'," : "")}
                           {(model.EndDate != null ? $@"'{model.EndDate}'," : "")}
                           {model.OrderNo}
                           {(setVendorActive ? $@", 1" : "")})");
          commaSeparator = ", ";
        }
      }
      else
      {
        query.Append(commaSeparator);
        query.Append($@" VALUES (
                           '{model.StockCode}',
                           '{vendor}',
                           {model.Cost},
                           {(model.StartDate != null ? $@"'{model.StartDate}'," : "")}
                           {(model.EndDate != null ? $@"'{model.EndDate}'," : "")}
                           {model.OrderNo}
                           {(setVendorActive ? $@", 1" : "")})");
        commaSeparator = ", ";
      }
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> InsertStockCosts(StockCosts model, string? storeIDs, string vendorID = "", bool setVendorActive = false)
  {
    StringBuilder query = new StringBuilder();

    query.Append($@"INSERT  [CSCMaster].[dbo].[StockCosts] (
                              Stock_Code, 
                              VendorID, 
                              {(model.Cost != null ? $@"Cost," : "")}
                              {(model.StartDate != null ? $@"StartDate," : "")}
                              {(model.EndDate != null ? $@"EndDate," : "")}
                              {(_config.IsBackOffice ? "" : "StoreID,")}
                              OrderNo 
                              {(setVendorActive ? $@", ActiveVendor" : "")})");

    query.Append($@" VALUES (
                              '{model.StockCode}',
                              {(string.IsNullOrWhiteSpace(model.VendorID) ? $@"'ALL'," : string.IsNullOrWhiteSpace(vendorID) ? $@"'{model.VendorID}'," : $@"'{vendorID}',")}
                              {(model.Cost != null ? $@"'{model.Cost}'," : "")}
                              {(model.StartDate != null ? $@"'{model.StartDate}'," : "")}
                              {(model.EndDate != null ? $@"'{model.EndDate}'," : "")}
                              {(_config.IsBackOffice ? "" : string.IsNullOrWhiteSpace(storeIDs) ? $@"{model.StoreID}," : $@"{storeIDs},")}
                              {model.OrderNo}
                              {(setVendorActive ? $@", 1 " : "")} 
                        )");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<VendorPart?> GetVendorPart(string? stockCode, string? vendorId)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM(Stock_Code)) AS StockCode, RTRIM(LTRIM(VendorID)) AS VendorID, PartNo
	                    FROM    [CSCMaster].[dbo].[VendorParts]
	                    WHERE   RTRIM(LTRIM(Stock_Code)) = '{stockCode}' AND
                                RTRIM(LTRIM(VendorID)) = '{vendorId}'");

    VendorPart? vendorPart = await Connection.QueryFirstOrDefaultAsync<VendorPart?>(query.ToString(), null, Transaction);

    if (vendorPart == null)
    {
      vendorPart = new VendorPart();
    }

    return vendorPart;
  }

  public async Task<int> InsertVendorPart(string? stockCode, string? vendorId, string? partNo)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT INTO [CSCMaster].[dbo].[VendorParts] (Stock_Code, VendorID, PartNo)
                      VALUES   ('{stockCode}', '{vendorId}', '{partNo}')");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateVendorPart(string? stockCode, string? vendorId, string? partNo)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE [CSCMaster].[dbo].[VendorParts] SET PartNo = '{partNo}'
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{stockCode}' AND
                              RTRIM(LTRIM(VendorId)) = '{vendorId}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateAllVendorInActive(StockPriceModel model, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE [CSCMaster].[dbo].[StockCosts] SET ActiveVendor = 0
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> ToggleVendorStatus(StockPriceModel model, string? storeIDs, bool status)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE  [CSCMaster].[dbo].[StockCosts] SET ActiveVendor = {(status ? "0" : "1")}
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              RTRIM(LTRIM(VendorId)) = '{model.VendorID}' AND
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockMasterCost(string? stockCode, string? vendorId, decimal? cost)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"Update  [CSCMaster].[dbo].[StockMst] SET STD_COST = {cost}, Vendor = '{vendorId}'
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{stockCode}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockCostBatch(StockCosts model, List<StockCostsBatch> stockCosts)
  {
    if (stockCosts == null || stockCosts.Count == 0) return 0;

    StringBuilder query = new StringBuilder();
    query.Append($@"Update  [CSCMaster].[dbo].[StockCosts] SET 
                              Cost = {model.Cost}, 
                              {(model.StartDate != null ? $@" StartDate = '{model.StartDate}', " : " ")} 
                              {(model.StartDate == null ? " " : model.EndDate == null ? $@" EndDate = NULL, " : $@" EndDate = '{model.EndDate}', ")}
                              UpdateDate = GETDATE()
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' 
                              AND OrderNo = {model.OrderNo} AND (");

    string separator = string.Empty;

    foreach (var stockCost in stockCosts)
    {
      query.Append($@" {separator} (VendorId = '{stockCost.vendorId}' AND StoreID = '{stockCost.storeId}')");
      separator = "OR";
    }
    query.Append(")");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockCost(StockCosts model, string? storeIDs, string? vendorId)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"Update  [CSCMaster].[dbo].[StockCosts] SET 
                              Cost = {model.Cost}, 
                              {(model.StartDate != null ? $@"StartDate = '{model.StartDate}'," : "")} 
                              {(model.StartDate == null ? "" : model.EndDate == null ? $@"EndDate = NULL," : $@"EndDate = '{model.EndDate}', ")}
                              UpdateDate = GETDATE()
                      WHERE   Stock_Code = '{model.StockCode}' AND
                              VendorId = {vendorId} AND
                              {(!_config.IsBackOffice ? $@"StoreID IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockPriceBatch(List<StockPrice> stockPrices)
  {
    if (stockPrices == null || stockPrices.Count == 0) return 0;

    var startDate = stockPrices[0].StartDate;
    var endDate = stockPrices[0].EndDate;

    string StockCode = stockPrices[0].StockCode ?? "ALL";
    int OrderNo = stockPrices[0].OrderNo;
    StringBuilder query = new StringBuilder();

    query.Append($@"UPDATE  [CSCMaster].[dbo].[Stock_Prices] SET Price = CASE");

    foreach (var stockPrice in stockPrices)
    {
      query.Append($@" WHEN Price_Number = {stockPrice.PriceNumber} AND VendorId = '{stockPrice.VendorID}' 
                              {(!_config.IsBackOffice ? $@"AND StoreID = '{stockPrice.StoreID}'" : "")} 
                         THEN {stockPrice.Price} ");
    }
    query.Append("ELSE Price END, Margin = CASE");

    foreach (var stockPrice in stockPrices)
    {
      query.Append($@" WHEN Price_Number = {stockPrice.PriceNumber} AND VendorId = '{stockPrice.VendorID}'
                              {(!_config.IsBackOffice ? $@"AND StoreID = '{stockPrice.StoreID}'" : "")} 
                         THEN {stockPrice.Margin} ");
    }
    query.Append($@"ELSE Margin END, 
                          {(startDate != null ? $@" StartDate = '{startDate}', " : " ")} 
                          {(startDate == null ? " " : endDate == null ? $@" EndDate = NULL, " : $@" EndDate = '{endDate}', ")}
                          UpdateDate = GETDATE()
                          WHERE Stock_Code = '{StockCode}' AND OrderNo = {OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockPrice(StockPrice model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE  [CSCMaster].[dbo].[Stock_Prices] SET 
                              Price = {model.Price}, 
                              Margin = {model.Margin},
                              {(model.StartDate != null ? $@"StartDate = '{model.StartDate}'," : "")} 
                              {(model.StartDate == null ? "" : model.EndDate == null ? $@"EndDate = NULL," : $@"EndDate = '{model.EndDate}', ")}
                              UpdateDate = GETDATE()
                      WHERE	RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              Price_Number = {model.PriceNumber} AND
                              RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockPriceListAsync(List<StockPrice> models)
  {
    if (models.Count == 0) return 0;
    StringBuilder query = new StringBuilder();

    foreach (var model in models)
    {
      query.AppendLine($"UPDATE [CSCMaster].[dbo].[Stock_Prices] SET");
      query.AppendLine($"    Price = CASE WHEN RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND Price_Number = {model.PriceNumber} AND RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND {(model.StartDate == null ? "StartDate IS NULL" : $"StartDate = '{model.StartDate}'")} THEN {model.Price} ELSE Price END,");
      query.AppendLine($"    Margin = CASE WHEN RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND Price_Number = {model.PriceNumber} AND RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND {(model.StartDate == null ? "StartDate IS NULL" : $"StartDate = '{model.StartDate}'")} THEN {model.Margin} ELSE Margin END,");
      query.AppendLine($"    EndDate = CASE WHEN RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND Price_Number = {model.PriceNumber} AND RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND {(model.StartDate == null ? "StartDate IS NULL" : $"StartDate = '{model.StartDate}'")} AND {(model.EndDate == null ? "EndDate IS NULL" : $"EndDate = '{model.EndDate}'")} THEN GETDATE() ELSE EndDate END,");
      query.AppendLine($"    UpdateDate = GETDATE()");
      query.AppendLine($"WHERE");
      if (!_config.IsBackOffice)
      {
        query.AppendLine($"    RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND Price_Number = {model.PriceNumber} AND RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND OrderNo = {model.OrderNo};");
      }
      else
      {
        query.AppendLine($"    RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND Price_Number = {model.PriceNumber} AND RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND OrderNo = {model.OrderNo};");
      }
      query.AppendLine();
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction, 180);
  }

  public async Task<int> InsertStockPriceBatch(StockPrice model, List<string> stores, List<string> vendors)
  {
    if (stores.Count == 0 || vendors.Count == 0) return 0;
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT INTO [CSCMaster].[dbo].[Stock_Prices] 
                      (
                          [Stock_Code], 
                          [Price_Number], 
                          [Price], 
                          {(!_config.IsBackOffice ? $@"[StoreID]," : "")}
                          [VendorID], 
                          {(model.OrderNo == 0 ? "" : $@"[StartDate],")}
                          {(model.OrderNo == 0 ? "" : model.EndDate == null ? "" : $@"EndDate,")}
                          [OrderNo], 
                          [Margin], 
                          [UpdateDate]
                      )
                      VALUES");
    for (int i = 0; i < stores.Count && i < vendors.Count; i++)
    {
      query.Append("(");
      query.Append($@"'{model.StockCode}',
                          {model.PriceNumber}, 
                          {model.Price}, 
                          {(!_config.IsBackOffice ? $@"'{stores[i]}'," : "")}
                          '{vendors[i]}',
                          {(model.OrderNo == 0 ? "" : $@"'{model.StartDate}', ")}
                          {(model.OrderNo == 0 ? "" : model.EndDate == null ? "" : $@"'{model.EndDate}', ")}
                          {model.OrderNo},
                          {model.Margin}, 
                          GETDATE()");
      query.Append(")");
      query.Append(i < stores.Count - 1 ? "," : ";");
    }
    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> InsertStockPrice(StockPrice model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT INTO [CSCMaster].[dbo].[Stock_Prices] 
                      (
                          [Stock_Code], 
                          [Price_Number], 
                          [Price], 
                          {(!_config.IsBackOffice ? $@"[StoreID]," : "")}
                          [VendorID], 
                          {(model.OrderNo == 0 ? "" : $@"[StartDate],")}
                          {(model.OrderNo == 0 ? "" : model.EndDate == null ? "" : $@"EndDate,")}
                          [OrderNo], 
                          [Margin], 
                          [UpdateDate]
                      )
                      VALUES 
                      (
                          '{model.StockCode}',
                          {model.PriceNumber}, 
                          {model.Price}, 
                          {(!_config.IsBackOffice ? $@"'{model.StoreID}'," : "")}
                          '{model.VendorID}',
                          {(model.OrderNo == 0 ? "" : $@"'{model.StartDate}', ")}
                          {(model.OrderNo == 0 ? "" : model.EndDate == null ? "" : $@"'{model.EndDate}', ")}
                          {model.OrderNo},
                          {model.Margin}, 
                          NULL
                      )");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> AddRebate(StockRebates model, List<string>? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT INTO [CSCMaster].[dbo].[StockRebates] 
                      (
                          [Stock_Code], 
                          [RebateNo],
                          {(!_config.IsBackOffice ? $@"[StoreID]," : "")}
                          [VendorID], 
                          [Rebate], 
                          [StartDate],
                          [EndDate], 
                          [OrderNo]
                      ) VALUES ");
    if (_config.IsBackOffice)
    {
      query.Append(@$"('{model.StockCode}', {model.RebateNo}, '{model.VendorID}', {model.Rebate}, '{model.StartDate}', '{model.EndDate}', {model.OrderNo})");
    }
    else
    {
      int len = storeIDs.Count;
      for (int i = 0; i < len; i++)
      {
        query.Append(@$"('{model.StockCode}', {model.RebateNo}, '{storeIDs[i]}', '{model.VendorID}', {model.Rebate}, '{model.StartDate}', '{model.EndDate}', {model.OrderNo})");
        if (i < len - 1)
        {
          query.Append(", ");
        }
      }
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteRebate(StockRebates model, string? storeIDs, bool useStoreIDs = true)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE  FROM [CSCMaster].[dbo].[StockRebates] 
                      WHERE   RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              RebateNo = {model.RebateNo} AND
                              RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND 
                              {(!_config.IsBackOffice && useStoreIDs ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> RemoveSpecialPrice(StockSpecialPrice model, string? storeIDs, bool useStoreIDs = true)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE  FROM [CSCMaster].[dbo].[PriceL] 
                      WHERE   Pr_Type <> '{model.PriceType}' AND
                              RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND 
                              {(!_config.IsBackOffice && useStoreIDs ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteSpecialPrice(StockSpecialPrice model, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE  FROM [CSCMaster].[dbo].[PriceL] 
                      WHERE   Pr_Type = '{model.PriceType}' AND
                              PR_F_QTY = {model.PriceFromQuantity} AND
                              RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND 
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteSpecialPriceFromDiscKLTable(StockSpecialPrice model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM DISC_KL
                      WHERE 
                      Stock_Code='{model.StockCode}' and PriceType = '{model.PriceType}' and CustType = '{model.PriceFromQuantity}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> InsertSpecialPrice(StockSpecialPrice model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT  INTO[dbo].[PRICEL] 
                      (
                          [STOCK_CODE], 
                          [PR_F_QTY], 
                          [PR_T_QTY], 
                          [PRICE], 
                          [Pr_Type], 
                          [Pr_Units], 
                          {(!_config.IsBackOffice ? $@"[StoreID]," : "")}
                          [VendorID], 
                          [StartDate], 
                          [EndDate], 
                          [OrderNo]
                      )
                      VALUES ");
    if (_config.IsBackOffice)
    {
      query.Append(@$"(
                          '{model.StockCode}', 
                          {model.PriceFromQuantity}, 
                          {model.PriceToQuantity}, 
                          {model.Price}, 
                          '{model.PriceType}', 
                          '{model.PriceUnits}', 
                          '{model.VendorID}',
                          '{model.StartDate}', 
                          '{model.EndDate}', 
                          {model.OrderNo}
                        )");
    }
    else
    {
      int storeIDCount = model.StoreIDs.Count;
      for (int i = 0; i < storeIDCount; i++)
      {
        query.Append(@$"(
                          '{model.StockCode}', 
                          {model.PriceFromQuantity}, 
                          {model.PriceToQuantity}, 
                          {model.Price}, 
                          '{model.PriceType}', 
                          '{model.PriceUnits}', 
                          {(!_config.IsBackOffice ? $@"'{model.StoreIDs[i]}'," : "")}
                          '{model.VendorID}',
                          '{model.StartDate}', 
                          '{model.EndDate}', 
                          {model.OrderNo}
                        )");

        if (i < storeIDCount - 1)
        {
          query.Append(", ");
        }
      }
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> AddSpecialPriceInDiscKLTableAsync(StockSpecialPrice specialPrice)
  {
    var sql = @"INSERT INTO [CSCMaster].[dbo].[DISC_KL] (
                    [STOCK_CODE],
                    [StoreID],
                    [CustType],
                    [PriceType],
                    [Discount]
                  )
                  VALUES (
                    @StockCode,
                    'KEYLCK',
                    @CustType,
                    @PriceType,
                    @Discount
                  )";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = specialPrice.StockCode;
    cmd.Parameters.Add("@CustType", SqlDbType.VarChar, 2).Value = specialPrice.PriceFromQuantity;
    cmd.Parameters.Add("@PriceType", SqlDbType.VarChar, 1).Value = specialPrice.PriceUnits;
    cmd.Parameters.Add("@Discount", SqlDbType.SmallMoney).Value = specialPrice.Price;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected);
  }


  public async Task<int> UpdateSpecialPrice(StockSpecialPrice model, string? storeIDs, bool applyFromCondition = false)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE  [CSCMaster].[dbo].[PriceL] SET [PR_F_QTY] = {model.PriceFromQuantity}, [PR_T_QTY] = {model.PriceToQuantity}, [PRICE] = {model.Price},
                              [Pr_Units] = '{model.PriceUnits}', [StartDate] = '{model.StartDate}', [EndDate] = '{model.EndDate}'
                      WHERE   Pr_Type = '{model.PriceType}' AND
                              {((applyFromCondition) ? @$"PR_F_QTY = {model.PriceFromQuantity} AND" : "")}
                              RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND
                              RTRIM(LTRIM(VendorID)) = '{model.VendorID}' AND 
                              {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) IN ({storeIDs}) AND" : "")}
                              OrderNo = {model.OrderNo}");

    int rowAffected = await Connection.ExecuteAsync(query.ToString(), null, Transaction);
    return rowAffected;
  }

  public async Task<int> UpdateSpecialPriceForUseARInvoicePolicyAsync(StockSpecialPrice model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE [CSCMaster].[dbo].[DISC_KL]
                      SET Discount = {model.Price}
                      WHERE STOCK_CODE = '{model.StockCode}' AND PriceType = '{model.PriceType}' AND CustType = '{model.PriceFromQuantity}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockMasterForActiveVendor(StockPriceModel model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE S SET S.Vendor = T.VendorID, S.Std_Cost = T.Std_Cost, S.Pr_Type = T.Pr_Type, S.Pr_Units = T.Pr_Units, S.Pr_From = T.Pr_From, S.Pr_To = T.Pr_To 
                      FROM [CSCMaster].[dbo].StockMst S INNER JOIN 
							        (
                        SELECT C.Stock_Code, C.VendorID, C.Cost AS Std_Cost, Pr_Type, Pr_Units, P.Pr_From, Pr_To
                        FROM
                        (
	                        SELECT	Cost, RTRIM(LTRIM(Stock_Code)) AS Stock_Code, RTRIM(LTRIM(VendorID)) AS VendorID, [OrderNo] 
	                        FROM	[CSCMaster].[dbo].StockCosts 
	                        WHERE	RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND 
                                    RTRIM(LTRIM(VendorID)) ='{model.VendorID}' AND 
                                    {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND" : "")}
                                    OrderNo = {model.OrderNo}
                        ) C
                        LEFT JOIN
                        (
	                        SELECT	RTRIM(LTRIM(Stock_Code)) AS Stock_Code, [Pr_Type], [Pr_Units], RTRIM(LTRIM(VendorID)) AS VendorID, [StartDate] AS Pr_From, [EndDate] AS Pr_To, [OrderNo] 
	                        FROM	[CSCMaster].[dbo].[PRICEL]
	                        WHERE	RTRIM(LTRIM(Stock_Code)) = '{model.StockCode}' AND 
                                    RTRIM(LTRIM(VendorID)) ='{model.VendorID}' AND 
                                    {(!_config.IsBackOffice ? $@"RTRIM(LTRIM(StoreID)) = '{model.StoreID}' AND" : "")}
                                    OrderNo = {model.OrderNo}
                        ) P ON C.Stock_Code = P.Stock_Code AND C.VendorID = P.VendorID AND C.OrderNo = P.OrderNo
						          ) T ON S.STOCK_CODE = T.Stock_Code
						          WHERE	RTRIM(LTRIM(S.Stock_Code)) = '{model.StockCode}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<PriceGroup>?> GetPriceGroup()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  RTRIM(LTRIM([PriceGroupID])) AS ID, [PriceGroupName] AS Name 
                      FROM    [CSCMaster].[dbo].[PriceGroups]
                      ORDER BY [PriceGroupID]");

    return await Connection.QueryAsync<PriceGroup>(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockMasterTransferFlag(string? stockCode)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE [CSCMaster].[dbo].StockMst SET Transfer=1 WHERE Stock_Code = '{stockCode}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<bool> AddStockPriceByStockCodeAsync(string stockCode, string baseStockCode)
  {
    string sql = (_config.IsBackOffice) ?
                        @"INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
                              Stock_Code, 
                              Price_Number, 
                              Price,
                              VendorID, 
                              StartDate, 
                              EndDate, 
                              OrderNo, 
                              Margin
                            ) 
                              SELECT 
                                @StockCode AS Stock_Code, 
                                Price_Number, 
                                Price, 
                                VendorID, 
                                StartDate, 
                                EndDate, 
                                OrderNo, 
                                Margin 
                              FROM [CSCMaster].[dbo].[Stock_Prices] 
                              WHERE Stock_Code=@BaseStockCode" :
                        @"INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
                                Stock_Code, 
                                Price_Number, 
                                Price,
                                StoreID,
                                VendorID, 
                                StartDate, 
                                EndDate, 
                                OrderNo, 
                                Margin
                              ) 
                              SELECT 
                                @StockCode AS Stock_Code, 
                                Price_Number, 
                                Price, 
                                StoreID,
                                VendorID, 
                                StartDate, 
                                EndDate, 
                                OrderNo, 
                                Margin 
                              FROM [CSCMaster].[dbo].[Stock_Prices] 
                              WHERE Stock_Code=@BaseStockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@BaseStockCode", SqlDbType.VarChar, 16).Value = baseStockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteStockPriceByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from Stock_Prices
                        Where Stock_Prices.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<double> GetStockPriceByStockCodeAndVendorAsync(string stockCode, string vendor)
  {
    const string sql = @"SELECT Price FROM Stock_Prices 
                          WHERE 
	                          Stock_Prices.Stock_Code = @StockCode AND 
	                          Stock_Prices.Price_Number = '1' AND (VendorID=@Vendor OR VendorID IS NULL) 
	                          AND OrderNo=0 
                          ORDER BY Stock_Prices.Price_Number";

    var parameters = new { StockCode = stockCode, Vendor = vendor };
    double? resultPrice = await Connection.QuerySingleOrDefaultAsync<double?>(sql, parameters, Transaction);

    if (resultPrice == null)
    {
      return (double)(resultPrice = 0.0);
    }

    return (double)resultPrice;
  }

  public StringBuilder PrepareGetStockPriceInfoQuery(GetStockPriceInfoDto model)
  {
    StringBuilder query = new StringBuilder();

    if (_config.IsBackOffice)
      query.Append($@"exec SProc_StockPriceInfo '{model.StockCode}','{model.VendorId}',{model.PriceNumber},{model.OrderNo}");
    else
      query.Append($@"exec SProc_StockPriceInfo '{model.StoreId}','{model.StockCode}','{model.VendorId}',{model.PriceNumber},{model.OrderNo}");

    return query;
  }

  public async Task<StockPrice> GetStockPrices(string stockCode, string? vendorId, int priceNumber, int? orderNo)
  {
    StringBuilder query = new();
    query.Append($@"SELECT
                          LTRIM(RTRIM(Stock_Code)) AS StockCode,
                          Price_Number AS PriceNumber,
                          Price,
                          VendorID AS VendorId,
                          StartDate,
                          EndDate,
                          OrderNo,
                          Margin,
                          UpdateDate
                      FROM
                          STOCK_PRICES
                      WHERE
                          Stock_Code = @StockCode
                          {(vendorId == null ? " " : " AND VendorID = @VendorId ")}
                          AND price_number = @PriceNumber
                          {(orderNo == null ? " " : "AND Orderno = @OrderNo ")} ");

    var parameters = new
    {
      StockCode = stockCode,
      VendorId = vendorId,
      PriceNumber = priceNumber,
      OrderNo = orderNo
    };

    StockPrice stockPrice = await Connection.QueryFirstOrDefaultAsync<StockPrice>(query.ToString(), parameters, Transaction);
    return stockPrice;
  }

  public async Task<IEnumerable<StockPrice>> GetStockPricesListAsync(List<(string stockCode, string vendorId, int orderNo)> parameters)
  {
    if (parameters == null || parameters.Count == 0) return new List<StockPrice>();
    StringBuilder query = new StringBuilder();

    query.AppendLine(@"SELECT
                          LTRIM(RTRIM(Stock_Code)) AS StockCode,
                          Price_Number AS PriceNumber,
                          Price,
                          VendorID AS VendorId,
                          StartDate,
                          EndDate,
                          OrderNo,
                          Margin,
                          UpdateDate
                      FROM
                          STOCK_PRICES
                      WHERE ");

    for (int i = 0; i < parameters.Count; i++)
    {
      var (stockCode, vendorId, orderNo) = parameters[i];

      query.AppendLine($"  (Stock_Code = '{stockCode}' ");
      query.AppendLine($"   AND VendorID = '{vendorId}' ");
      query.AppendLine($"   AND Orderno = {orderNo} ");
      query.AppendLine(") ");

      if (i < parameters.Count - 1)
      {
        query.AppendLine("OR");
      }
    }

    IEnumerable<StockPrice> stockPrices = await Connection.QueryAsync<StockPrice>(query.ToString(), null, Transaction, 180);
    return stockPrices;
  }


  public async Task<int> InsertStockPriceInfo(StockPriceInfo model)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT INTO [CSCMaster].[dbo].[Stock_Prices] 
                      (
                          [Stock_Code], 
                          [Price_Number], 
                          [Price], 
                          {(!_config.IsBackOffice ? $@"[StoreID]," : "")}
                          [VendorID], 
                          {(model.OrderNo == 0 ? "" : $@"[StartDate],")}
                          {(model.OrderNo == 0 ? "" : model.EndDate == null ? "" : $@"EndDate,")}
                          [OrderNo], 
                          [Margin], 
                          [UpdateDate]
                      )
                      VALUES 
                      (
                          '{model.Stock_Code}',
                          {model.Price_Number}, 
                          {model.Price}, 
                          {(!_config.IsBackOffice ? $@"'{model.StoreId}'," : "")}
                          '{model.VendorId}',
                          {(model.OrderNo == 0 ? "" : $@"'{model.StartDate}', ")}
                          {(model.OrderNo == 0 ? "" : model.EndDate == null ? "" : $@"'{model.EndDate}', ")}
                          {model.OrderNo},
                          {model.Margin}, 
                          GETDATE()
                      )");
    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> UpdateStockPriceInfoAsync(StockPriceInfo oldStockPriceInfo, StockPriceInfo newStockPriceInfo)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"UPDATE [CSCMaster].[dbo].Stock_Prices 
                      SET StoreID = @NewStoreId, 
                          Stock_Code = @NewStockCode, 
                          Price_Number = @NewPriceNumber, 
                          VendorID = @NewVendorId, 
                          OrderNo = @NewOrderNo, 
                          Price = @NewPrice,
                          Margin = @NewMargin, 
                          StartDate = @NewStartDate, 
                          EndDate = @NewEndDate,
                          UpdateDate = GETDATE()
                      WHERE StoreID = @OldStoreId 
                          AND Stock_Code = @OldStockCode 
                          AND Price_Number = @OldPriceNumber 
                          AND VendorID = @OldVendorId 
                          AND OrderNo = @OldOrderNo
                      ");

    var parameters = new
    {
      NewStoreId = newStockPriceInfo.StoreId,
      NewStockCode = newStockPriceInfo.Stock_Code,
      NewPriceNumber = newStockPriceInfo.Price_Number,
      NewVendorId = newStockPriceInfo.VendorId,
      NewOrderNo = newStockPriceInfo.OrderNo,
      NewPrice = newStockPriceInfo.Price,
      NewMargin = newStockPriceInfo.Margin,
      NewStartDate = newStockPriceInfo.StartDate,
      NewEndDate = newStockPriceInfo.EndDate,
      OldStoreId = oldStockPriceInfo.StoreId,
      OldStockCode = oldStockPriceInfo.Stock_Code,
      OldPriceNumber = oldStockPriceInfo.Price_Number,
      OldVendorId = oldStockPriceInfo.VendorId,
      OldOrderNo = oldStockPriceInfo.OrderNo
    };
    return await Connection.ExecuteAsync(query.ToString(), parameters, Transaction);
  }

  public async Task<int> CreateOrUpdateStockPriceAsync(PriceDefinition model, bool? updateForVendorAll = false)
  {
    string sql = $@"EXEC SProc_UpdateOrInsertStockPrices @ProductId, @PriceNumber, @Price, @StoreId, @VendorId, @PR_FROM, @PR_TO, @OrderNo, @Margin ";

    int affectedRows = await Connection.ExecuteAsync(sql, model, Transaction);

    if (updateForVendorAll ?? false)
    {
      model.VendorId = "ALL";
      affectedRows += await Connection.ExecuteAsync(sql, model, Transaction);
    }

    return affectedRows;
  }

  public async Task<int> DeleteExistingSpecialPricesAsync(string stockCode, string storeId, int orderNo, string vendorId)
  {
    string sql = $@"DELETE FROM PriceL 
                      WHERE Stock_Code = '{stockCode}' AND StoreId = '{storeId}' 
                            AND OrderNo = {orderNo} AND VendorId = '{vendorId}'";

    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows;
  }

  public async Task<int> GetStockPriceRecordCountAsync(GetStockPriceRecordCountDto model)
  {
    string sql = $@"SELECT COUNT(Stock_Code) FROM Stock_Prices 
                      WHERE Stock_Code = @StockCode AND StoreId = @StoreId
                            AND OrderNO = @OrderNo AND VendorId = @VendorId ";

    int stockCostCount = await Connection.QueryFirstOrDefaultAsync<int>(sql, model, Transaction);
    return stockCostCount;
  }
}
