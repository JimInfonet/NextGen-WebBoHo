namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
internal class StockItemRepository : BaseRepository, IStockItemRepository
{
  private readonly IApplicationConfig _applicationConfig;
  public StockItemRepository(IApplicationConfig applicationConfig, IDbTransaction transaction) : base(transaction)
  {
    _applicationConfig = applicationConfig;
  }

  public async Task<IEnumerable<StockItem>> GetStockItemsByFilterAsync(SearchCriteria criteria)
  {
    string sql = PrepareSqlQueryForStockItems(criteria);
    if (criteria.Length > 0)
    {
      sql += $"OFFSET {criteria.Start} ROWS FETCH NEXT {criteria.Length} ROWS ONLY";
    }
    var parameters = GetStockItemQueryParams(criteria);

    IEnumerable<StockItem> resultStockItems = await Connection.QueryAsync<StockItem>(sql, parameters, Transaction);
    return resultStockItems;
  }

  public async Task<IEnumerable<StockItemMinimal>> GetStockItemsByDeptSubDeptAndSubDetailAsync(GetStockItemsByDeptSubDeptAndSubDetailDto searchParams)
  {
    string sql = _applicationConfig.IsBackOffice ?
      PrepareSqlQueryForStockItemsByDeptSubDeptAndSubDetailForBO(searchParams, false) :
      PrepareSqlQueryForStockItemsByDeptSubDeptAndSubDetail(searchParams, false);

    IEnumerable<StockItemMinimal> stockItems = await Connection.QueryAsync<StockItemMinimal>(sql, null, Transaction);
    return stockItems;
  }

  public async Task<int> GetStockItemsByDeptSubDeptAndSubDetailCountAsync(GetStockItemsByDeptSubDeptAndSubDetailDto searchParams)
  {
    string sql = _applicationConfig.IsBackOffice ?
      PrepareSqlQueryForStockItemsByDeptSubDeptAndSubDetailForBO(searchParams, true) :
      PrepareSqlQueryForStockItemsByDeptSubDeptAndSubDetail(searchParams, true);

    int totalStockItems = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return totalStockItems;
  }

  public async Task<IEnumerable<StockItemMinimal>> GetStockItemsByVendorAsync(GetStockItemsByVendorDto searchParams)
  {
    string sql = PrepareSqlQueryForStockItemsByVendor(searchParams, false);

    IEnumerable<StockItemMinimal> stockItems = await Connection.QueryAsync<StockItemMinimal>(sql, null, Transaction);
    return stockItems;
  }

  public async Task<int> GetStockItemsByVendorCountAsync(GetStockItemsByVendorDto searchParams)
  {
    string sql = PrepareSqlQueryForStockItemsByVendor(searchParams, true);

    int totalStockItems = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return totalStockItems;
  }

  public async Task<IEnumerable<StockItemMinimal>> GetStockItemsOfRecentChangesAsync(GetStockItemsByDeptSubDeptAndSubDetailDto searchParams)
  {
    string sql = $@"
                    select distinct 
                     LTRIM(RTRIM(stock_code)) StockCode,
                     Descript Description,
                     dept Dept,
                     sub_dept SubDept,
                     sub_detail SubDetail
                    from stockmst  where Transfer = 1";

    if (searchParams.SearchValue != null)
    {
      string columnName = (searchParams.SearchColumnName == "Product ID" || searchParams.SearchColumnName == "Stock Code") ? "stock_code" : "Descript";
      sql += $@" and {columnName} LIKE '%{searchParams.SearchValue}%' ";
    }

    sql += $@" order by StockCode";

    if (searchParams.Start != null)
    {
      sql += $@" OFFSET {searchParams.Start} ROWS FETCH NEXT {searchParams.Length} ROWS ONLY; ";
    }
    IEnumerable<StockItemMinimal> stockItems = await Connection.QueryAsync<StockItemMinimal>(sql, null, Transaction);
    return stockItems;
  }

  public async Task<int> GetStockItemsOfRecentChangesCountAsync(GetStockItemsByDeptSubDeptAndSubDetailDto searchParams)
  {
    string sql = $@"
                    select distinct 
                     Count(stock_code)
                    from stockmst  where Transfer = 1";

    if (searchParams.SearchValue != null)
    {
      string columnName = (searchParams.SearchColumnName == "Product ID" || searchParams.SearchColumnName == "Stock Code") ? "stock_code" : "Descript";
      sql += $@" and {columnName} LIKE '%{searchParams.SearchValue}%' ";
    }

    int stockItemsCount = await Connection.QueryFirstOrDefaultAsync<int>(sql, null, Transaction);
    return stockItemsCount;
  }

  public async Task<StockMaster> GetStockMasterByStockCodeAsync(string stockCode)
  {
    string sql = $@"SELECT
													LTRIM(RTRIM([STOCK_CODE])) StockCode,
													[DESCRIPT] Description,
													[STOCK_TYPE] StockType,
													[PROD_DISC] ProductDiscount,
													[STD_COST] StandardCost,
													[AVG_COST] AverageCost,
													[AVG_UNITS] AverageUnits,
													[LATE_COST] LateCost,
													[DEPT] Department,
													[SUB_DEPT] SubDepartment,
													[Sub_Detail] SubDetail,
													[SERIAL] Serial,
													[S_BY_WGHT] SBYWGHT,
													[Vendor] Vendor,
													[PR_TYPE] PriceType,
													[PR_UNITS] PriceUnits,
													[PR_FROM] PriceFrom,
													[PR_TO] PriceTo,
													[VendorNumber] VendorNumber,
													[Availability] Availability,
													[Brand] Brand,
													[Characteristic] Characteristic,
													[CreationDate] CreationDate,
													[Family] Family,
													[Format] Format,
													[Generic] Generic,
													[Label] Label,
													[UM] UnitOfMeasure,
													[UpdateDate] UpdateDate,
													[OrderUnit] OrderUnit,
													[Packing] Packing,
													[Transfer] Transfer,
													[ElgLoyalty] EligibleLoyalty,
													[ElgFuelRebate] EligibleFuelRebate,
													[FuelRebate] FuelRebate,
													[QualTaxRebate] QualTaxRebate,
													[ElgTaxRebate] EligibleTaxRebate,
													[ElgTaxExemption] EligibleTaxExemption
												FROM [CSCMaster].[dbo].[STOCKMST]
												WHERE [STOCK_CODE] = @StockCode";

    var parameters = new { StockCode = stockCode };
    StockMaster resultStockMaster = await Connection.QuerySingleOrDefaultAsync<StockMaster>(sql, parameters, Transaction);
    return resultStockMaster;
  }

  public async Task<IEnumerable<StockItem>> GetAllStockItemsAsync()
  {
    const string sql = @"SELECT 
	        LTRIM(RTRIM([CSCMaster].[dbo].[STOCKMST].Stock_Code)) StockCode, 
	        [CSCMaster].[dbo].[STOCKMST].Descript Description, 
	        [CSCMaster].[dbo].[STOCKMST].format Format, 
	        [CSCMaster].[dbo].[UM].Description UnitOfMeasure, 
	        [CSCMaster].[dbo].[Family].Description  Family,
	        [CSCMaster].[dbo].[Brand].Description  Brand  
        FROM (([CSCMaster].[dbo].[STOCKMST]  Left Join [CSCMaster].[dbo].[Brand] ON [CSCMaster].[dbo].[STOCKMST].Brand = [CSCMaster].[dbo].[Brand].BrandID ) 
					        LEFT  JOIN [CSCMaster].[dbo].[Family]  ON [CSCMaster].[dbo].[STOCKMST].Family = [CSCMaster].[dbo].[Family].FamilyID) 
					        LEFT JOIN  [CSCMaster].[dbo].[UM] ON [CSCMaster].[dbo].[STOCKMST].UM = [CSCMaster].[dbo].[UM].UMID
        order by [CSCMaster].[dbo].[STOCKMST].stock_Code";

    IEnumerable<StockItem> resultStockItems = await Connection.QueryAsync<StockItem>(sql, null, Transaction);
    return resultStockItems;
  }

  public async Task<bool> AddStockMasterAsync(StockMaster stockMaster)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[STOCKMST] (
													[STOCK_CODE],
													[DESCRIPT],
													[STOCK_TYPE],
													[PROD_DISC],
													[STD_COST],
													[AVG_COST],
													[AVG_UNITS],
													[LATE_COST],
													[DEPT],
													[SUB_DEPT],
													[Sub_Detail],
													[SERIAL],
													[S_BY_WGHT],
													[Vendor],
													[PR_TYPE],
													[PR_UNITS],
													[PR_FROM],
													[PR_TO],
													[VendorNumber],
													[Availability],
													[Brand],
													[Characteristic],
													[CreationDate],
													[Family],
													[Format],
													[Generic],
													[Label],
													[UM],
													[UpdateDate],
													[OrderUnit],
													[Packing],
													[Transfer],
													[ElgLoyalty],
													[ElgFuelRebate],
													[FuelRebate],
													[QualTaxRebate],
													[ElgTaxRebate],
													[ElgTaxExemption]
												)
												VALUES (
													@StockCode,
													@Description,
													@StockType,
													@ProductDiscount,
													@StandardCost,
													@AverageCost,
													@AverageUnits,
													@LateCost,
													@Department,
													@SubDepartment,
													@SubDetail,
													@Serial,
													@SBYWGHT,
													@Vendor,
													@PriceType,
													@PriceUnits,
													@PriceFrom,
													@PriceTo,
													@VendorNumber,
													@Availability,
													@Brand,
													@Characteristic,
													@CreationDate,
													@Family,
													@Format,
													@Generic,
													@Label,
													@UnitOfMeasure,
													@UpdateDate,
													@OrderUnit,
													@Packing,
													@Transfer,
													@EligibleLoyalty,
													@EligibleFuelRebate,
													@FuelRebate,
													@QualTaxRebate,
													@EligibleTaxRebate,
													@EligibleTaxExemption
												)";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockMaster.StockCode;
    cmd.Parameters.Add("@Description", SqlDbType.VarChar, 35).Value = stockMaster.Description ?? (object)DBNull.Value;
    cmd.Parameters.Add("@StockType", SqlDbType.VarChar, 1).Value = stockMaster.StockType ?? (object)DBNull.Value;
    cmd.Parameters.Add("@ProductDiscount", SqlDbType.TinyInt).Value = stockMaster.ProductDiscount ?? 0;
    cmd.Parameters.Add("@StandardCost", SqlDbType.Money).Value = stockMaster.StandardCost ?? 0;
    cmd.Parameters.Add("@AverageCost", SqlDbType.Money).Value = stockMaster.AverageCost ?? 0;
    cmd.Parameters.Add("@AverageUnits", SqlDbType.Money).Value = stockMaster.AverageUnits ?? 0;
    cmd.Parameters.Add("@LateCost", SqlDbType.Money).Value = stockMaster.LateCost ?? 0;
    cmd.Parameters.Add("@Department", SqlDbType.VarChar, 10).Value = stockMaster.Department ?? "";
    cmd.Parameters.Add("@SubDepartment", SqlDbType.VarChar, 10).Value = stockMaster.SubDepartment ?? "";
    cmd.Parameters.Add("@SubDetail", SqlDbType.VarChar, 10).Value = stockMaster.SubDetail ?? "";
    cmd.Parameters.Add("@Serial", SqlDbType.Bit).Value = stockMaster.Serial ?? false;
    cmd.Parameters.Add("@SBYWGHT", SqlDbType.Bit).Value = stockMaster.SBYWGHT ?? false;
    cmd.Parameters.Add("@Vendor", SqlDbType.VarChar, 10).Value = stockMaster.Vendor ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceType", SqlDbType.VarChar, 1).Value = stockMaster.PriceType ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceUnits", SqlDbType.VarChar, 1).Value = stockMaster.PriceUnits ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceFrom", SqlDbType.SmallDateTime).Value = stockMaster.PriceFrom ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceTo", SqlDbType.SmallDateTime).Value = stockMaster.PriceTo ?? (object)DBNull.Value;
    cmd.Parameters.Add("@VendorNumber", SqlDbType.VarChar, 16).Value = stockMaster.VendorNumber ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Availability", SqlDbType.Bit).Value = stockMaster.Availability ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Brand", SqlDbType.VarChar, 3).Value = stockMaster.Brand ?? "";
    cmd.Parameters.Add("@Characteristic", SqlDbType.VarChar, 3).Value = stockMaster.Characteristic ?? "";
    cmd.Parameters.Add("@CreationDate", SqlDbType.SmallDateTime).Value = stockMaster.CreationDate ?? DateTime.Now;
    cmd.Parameters.Add("@Family", SqlDbType.VarChar, 5).Value = stockMaster.Family ?? "";
    cmd.Parameters.Add("@Format", SqlDbType.VarChar, 6).Value = stockMaster.Format ?? "";
    cmd.Parameters.Add("@Generic", SqlDbType.VarChar, 5).Value = stockMaster.Generic ?? "";
    cmd.Parameters.Add("@Label", SqlDbType.Bit).Value = stockMaster.Label ?? (object)DBNull.Value;
    cmd.Parameters.Add("@UnitOfMeasure", SqlDbType.VarChar, 3).Value = stockMaster.UnitOfMeasure ?? "";
    cmd.Parameters.Add("@UpdateDate", SqlDbType.SmallDateTime).Value = stockMaster.UpdateDate ?? DateTime.Now;
    cmd.Parameters.Add("@OrderUnit", SqlDbType.VarChar, 3).Value = stockMaster.OrderUnit ?? "";
    cmd.Parameters.Add("@Packing", SqlDbType.SmallInt).Value = stockMaster.Packing ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Transfer", SqlDbType.Bit).Value = stockMaster.Transfer ?? (object)DBNull.Value;
    cmd.Parameters.Add("@EligibleLoyalty", SqlDbType.Bit).Value = stockMaster.EligibleLoyalty ?? (object)DBNull.Value;
    cmd.Parameters.Add("@EligibleFuelRebate", SqlDbType.Bit).Value = stockMaster.EligibleFuelRebate ?? (object)DBNull.Value;
    cmd.Parameters.Add("@FuelRebate", SqlDbType.Money).Value = stockMaster.FuelRebate ?? (object)DBNull.Value;
    cmd.Parameters.Add("@QualTaxRebate", SqlDbType.Bit).Value = stockMaster.QualTaxRebate ?? true;
    cmd.Parameters.Add("@EligibleTaxRebate", SqlDbType.Bit).Value = stockMaster.EligibleTaxRebate ?? true;
    cmd.Parameters.Add("@EligibleTaxExemption", SqlDbType.Bit).Value = stockMaster.EligibleTaxExemption ?? true;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> UpdateStockMasterAsync(StockMaster stockMaster)
  {
    string sql = $@"UPDATE [CSCMaster].[dbo].[STOCKMST]
											SET
													[DESCRIPT] = @Description,
													[STOCK_TYPE] = @StockType,
													[PROD_DISC] = @ProductDiscount,
													[STD_COST] = @StandardCost,
													[AVG_COST] = @AverageCost,
													[AVG_UNITS] = @AverageUnits,
													[LATE_COST] = @LateCost,
													[DEPT] = @Department,
													[SUB_DEPT] = @SubDepartment,
													[Sub_Detail] = @SubDetail,
													[SERIAL] = @Serial,
													[S_BY_WGHT] = @SBYWGHT,
													[Vendor] = @Vendor,
													[PR_TYPE] = @PriceType,
													[PR_UNITS] = @PriceUnits,
													[PR_FROM] = @PriceFrom,
													[PR_TO] = @PriceTo,
													[VendorNumber] = @VendorNumber,
													[Availability] = @Availability,
													[Brand] = @Brand,
													[Characteristic] = @Characteristic,
													[CreationDate] = @CreationDate,
													[Family] = @Family,
													[Format] = @Format,
													[Generic] = @Generic,
													[Label] = @Label,
													[SaleCode] = @SaleCode,
													[ShelfLabel] = @ShelfLabel,
													[UM] = @UnitOfMeasure,
													[UpdateDate] = @UpdateDate,
													[OrderUnit] = @OrderUnit,
													[Packing] = @Packing,
													[Transfer] = @Transfer,
													[ElgLoyalty] = @EligibleLoyalty,
													[ElgFuelRebate] = @EligibleFuelRebate,
													[FuelRebate] = @FuelRebate,
													[QualTaxRebate] = @QualTaxRebate,
													[ElgTaxRebate] = @EligibleTaxRebate,
													[ElgTaxExemption] = @EligibleTaxExemption
											WHERE [STOCK_CODE] = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockMaster.StockCode;
    cmd.Parameters.Add("@Description", SqlDbType.VarChar, 35).Value = stockMaster.Description ?? (object)DBNull.Value;
    cmd.Parameters.Add("@StockType", SqlDbType.VarChar, 1).Value = stockMaster.StockType ?? (object)DBNull.Value;
    cmd.Parameters.Add("@ProductDiscount", SqlDbType.TinyInt).Value = stockMaster.ProductDiscount ?? 0;
    cmd.Parameters.Add("@StandardCost", SqlDbType.Money).Value = stockMaster.StandardCost ?? 0;
    cmd.Parameters.Add("@AverageCost", SqlDbType.Money).Value = stockMaster.AverageCost ?? 0;
    cmd.Parameters.Add("@AverageUnits", SqlDbType.Money).Value = stockMaster.AverageUnits ?? 0;
    cmd.Parameters.Add("@LateCost", SqlDbType.Money).Value = stockMaster.LateCost ?? 0;
    cmd.Parameters.Add("@Department", SqlDbType.VarChar, 10).Value = stockMaster.Department ?? "";
    cmd.Parameters.Add("@SubDepartment", SqlDbType.VarChar, 10).Value = stockMaster.SubDepartment ?? "";
    cmd.Parameters.Add("@SubDetail", SqlDbType.VarChar, 10).Value = stockMaster.SubDetail ?? "";
    cmd.Parameters.Add("@Serial", SqlDbType.Bit).Value = stockMaster.Serial ?? false;
    cmd.Parameters.Add("@SBYWGHT", SqlDbType.Bit).Value = stockMaster.SBYWGHT ?? false;
    cmd.Parameters.Add("@Vendor", SqlDbType.VarChar, 10).Value = stockMaster.Vendor ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceType", SqlDbType.VarChar, 1).Value = stockMaster.PriceType ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceUnits", SqlDbType.VarChar, 1).Value = stockMaster.PriceUnits ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceFrom", SqlDbType.SmallDateTime).Value = stockMaster.PriceFrom ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PriceTo", SqlDbType.SmallDateTime).Value = stockMaster.PriceTo ?? (object)DBNull.Value;
    cmd.Parameters.Add("@VendorNumber", SqlDbType.VarChar, 16).Value = stockMaster.VendorNumber ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Availability", SqlDbType.Bit).Value = stockMaster.Availability ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Brand", SqlDbType.VarChar, 3).Value = stockMaster.Brand ?? "";
    cmd.Parameters.Add("@Characteristic", SqlDbType.VarChar, 3).Value = stockMaster.Characteristic ?? "";
    cmd.Parameters.Add("@CreationDate", SqlDbType.SmallDateTime).Value = stockMaster.CreationDate ?? DateTime.Now;
    cmd.Parameters.Add("@Family", SqlDbType.VarChar, 5).Value = stockMaster.Family ?? "";
    cmd.Parameters.Add("@Format", SqlDbType.VarChar, 6).Value = stockMaster.Format ?? "";
    cmd.Parameters.Add("@Generic", SqlDbType.VarChar, 5).Value = stockMaster.Generic ?? "";
    cmd.Parameters.Add("@Label", SqlDbType.Bit).Value = stockMaster.Label ?? (object)DBNull.Value;
    cmd.Parameters.Add("@SaleCode", SqlDbType.Bit).Value = 1;
    cmd.Parameters.Add("@ShelfLabel", SqlDbType.Bit).Value = 1;
    cmd.Parameters.Add("@UnitOfMeasure", SqlDbType.VarChar, 3).Value = stockMaster.UnitOfMeasure ?? "";
    cmd.Parameters.Add("@UpdateDate", SqlDbType.SmallDateTime).Value = stockMaster.UpdateDate ?? DateTime.Now;
    cmd.Parameters.Add("@OrderUnit", SqlDbType.VarChar, 3).Value = stockMaster.OrderUnit ?? "";
    cmd.Parameters.Add("@Packing", SqlDbType.SmallInt).Value = stockMaster.Packing ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Transfer", SqlDbType.Bit).Value = (_applicationConfig.IsBackOffice == false) ? true : stockMaster.Transfer ?? (object)DBNull.Value;
    cmd.Parameters.Add("@EligibleLoyalty", SqlDbType.Bit).Value = stockMaster.EligibleLoyalty ?? (object)DBNull.Value;
    cmd.Parameters.Add("@EligibleFuelRebate", SqlDbType.Bit).Value = stockMaster.EligibleFuelRebate ?? (object)DBNull.Value;
    cmd.Parameters.Add("@FuelRebate", SqlDbType.Money).Value = stockMaster.FuelRebate ?? (object)DBNull.Value;
    cmd.Parameters.Add("@QualTaxRebate", SqlDbType.Bit).Value = stockMaster.QualTaxRebate ?? true;
    cmd.Parameters.Add("@EligibleTaxRebate", SqlDbType.Bit).Value = stockMaster.EligibleTaxRebate ?? true;
    cmd.Parameters.Add("@EligibleTaxExemption", SqlDbType.Bit).Value = stockMaster.EligibleTaxExemption ?? true;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> UpdateStockMasterAsync(decimal stockCost, string stockCode)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[STOCKMST]
                        SET
                            STD_Cost = @StockCost
                        WHERE
                            Stock_Code = @StockCode";
    var parameters = new { StockCost = stockCost, StockCode = stockCode };
    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteStockMasterByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from StockMst
                        Where StockMst.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<StockMaster> GetStockItemByCode(string stockCode)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT LTRIM(RTRIM(Stock_Code)) AS StockCode, DESCRIPT AS Description, STOCK_TYPE AS StockType, PROD_DISC AS ProductDiscount, 
											STD_COST AS StandardCost, AVG_COST AS AverageCost,AVG_UNITS AS AverageUnits, LATE_COST AS LateCost, 
											DEPT AS Department, SUB_DEPT AS SubDepartment, Sub_Detail AS SubDetail, SERIAL AS Serial, S_BY_WGHT,
											Vendor AS Vendor, PR_TYPE AS PriceType, PR_UNITS AS PriceUnits, PR_FROM AS PriceForm, PR_TO AS PriceTo, 
											VendorNumber, Availability, Brand, Characteristic, CreationDate, Family, Format, Generic, Label, SaleCode,
											ShelfLabel, UM, UpdateDate, OrderUnit, Packing, Transfer, ElgLoyalty, ElgFuelRebate, FuelRebate, QualTaxRebate,
											ElgTaxRebate, ElgTaxExemption

									FROM    [CSCMaster].[dbo].[STOCKMST]
									WHERE   Stock_Code = '{stockCode}'");

    return await Connection.QuerySingleOrDefaultAsync<StockMaster>(query.ToString(), null, Transaction);
  }

  public async Task<PackingInfo> GetPackingInfo(string stockCode)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT PACKING 
                      FROM STOCKMST
									    WHERE Stock_Code = '{stockCode}'");

    return await Connection.QuerySingleOrDefaultAsync<PackingInfo>(query.ToString(), null, Transaction);
  }

  public async Task<List<PackCountDto>> GetPackCountBatchAsync(List<string> stockCodes)
  {
    string encodedStockCodes = "'" + string.Join("', '", stockCodes) + "'";

    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT LTRIM(RTRIM(Stock_Code)) StockCode, CASE WHEN Packing IS NULL THEN 1 ELSE Packing END AS Packing
                      FROM   StockMst
                      WHERE  Stock_Code IN ({encodedStockCodes}) ");

    var packCounts = (await Connection.QueryAsync<PackCountDto>(query.ToString(), null, Transaction)).ToList();
    return packCounts;
  }

  public async Task<IEnumerable<PackingInfo>> GetPackingInfoBatch(List<string> stockCodes)
  {
    if (stockCodes.Count() < 1) return new List<PackingInfo>();
    using (var connection = new SqlConnection(_applicationConfig.CSCMasterConnectionString))
    {
      DataTable stockCodesTable = new DataTable();
      stockCodesTable.Columns.Add("StockCode", typeof(string));
      foreach (var stockCode in stockCodes)
      {
        stockCodesTable.Rows.Add(stockCode);
      }
      var parameters = new DynamicParameters();
      parameters.Add("@StockCodes", stockCodesTable.AsTableValuedParameter("StockCodeTableType"));

      var stockCodePackingDictionary = new Dictionary<string, Int16>();
      foreach (string stockCode in stockCodes)
      {
        if (!stockCodePackingDictionary.ContainsKey(stockCode))
          stockCodePackingDictionary.Add(stockCode, 1);
      }

      var results = await connection.QueryAsync<PackingInfoDto>("SProc_PackingInfoBatch", parameters, commandType: CommandType.StoredProcedure);
      foreach (var result in results)
      {
        stockCodePackingDictionary[result.StockCode] = result.Packing ?? 1;
      }

      var batchResult = new List<PackingInfo>();
      foreach (var stockCode in stockCodes)
      {
        batchResult.Add(new PackingInfo { Packing = stockCodePackingDictionary[stockCode] });
      }

      return batchResult;
    }
  }

  public async Task<bool> UpdateStockItemTransfer(string stockCode)
  {
    const string sql = @"UPDATE STOCKMST SET Transfer = 1 WHERE STOCK_CODE = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<int> GetStockItemsByFilterCountAsync(SearchCriteria criteria)
  {
    string sql = PrepareSqlQueryForStockItems(criteria, true);
    var parameters = GetStockItemQueryParams(criteria);

    IEnumerable<int> StockItemsCounts = await Connection.QueryAsync<int>(sql, parameters, Transaction);
    return StockItemsCounts.FirstOrDefault();
  }

  public async Task<int> UpdateStockItemUpdateDateField(string stockCode)
  {
    string sql = $"UPDATE StockMst SET UpdateDate = '{DateTime.Now}' WHERE Stock_Code = '{stockCode}'";
    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows;
  }

  private string PrepareSqlQueryForStockItemsByDeptSubDeptAndSubDetail(GetStockItemsByDeptSubDeptAndSubDetailDto searchParams, bool countOnly = false)
  {
    string sql = $@"
          SELECT
              {(countOnly ? "COUNT(MST.Stock_Code) " :
            $@"
              {((searchParams.IsDistinct == true) ? "distinct" : "")}
              RTRIM(LTRIM(MST.Stock_Code)) StockCode, 
              RTRIM(LTRIM(MST.Descript)) Description,
              RTRIM(LTRIM(MST.Dept)) Dept, 
              RTRIM(LTRIM(MST.SUB_DEPT)) SubDept,
              RTRIM(LTRIM(MST.Sub_Detail)) SubDetail")}       
          FROM 
              [CSCMaster].[dbo].[STOCKMST] AS MST 
          ";

    bool haveAtLeastOneDeptOrSubDeptOrSubDetail = false;
    string separator = "";

    if (searchParams.Depts?.Count > 0 || searchParams.SubDepts?.Count > 0 || searchParams.SubDetails?.Count > 0)
    {
      haveAtLeastOneDeptOrSubDeptOrSubDetail = true;
      sql += " WHERE ( ";

      if (searchParams.Depts?.Count > 0)
      {
        separator = "";
        foreach (Department dept in searchParams.Depts)
        {
          sql += $"{separator} Dept = {dept.DeptId}";
          separator = " OR ";
        }
      }

      if (searchParams.SubDepts?.Count > 0)
      {
        separator = (searchParams.Depts?.Count > 0) ? " OR " : " ";
        foreach (SubDepartment subDept in searchParams.SubDepts)
        {
          sql += $"{separator} (Dept = {subDept.DeptId} AND Sub_Dept = {subDept.SubDeptId}) ";
          separator = " OR ";
        }
      }

      if (searchParams.SubDetails?.Count > 0)
      {
        separator = (searchParams.Depts?.Count > 0) || (searchParams.SubDepts?.Count > 0) ? " OR " : " ";
        foreach (SubDetail subDetail in searchParams.SubDetails)
        {
          sql += $"{separator} (Dept = {subDetail.DeptId} AND Sub_Dept = {subDetail.SubDeptId} AND Sub_Detail = {subDetail.SubDetailId}) ";
          separator = " OR ";
        }
      }

      sql += " ) ";
    }

    if (searchParams.SearchValue != null)
    {
      separator = haveAtLeastOneDeptOrSubDeptOrSubDetail ? " AND " : " WHERE ";
      string columnName = (searchParams.SearchColumnName == "Product ID" || searchParams.SearchColumnName == "Stock Code") ? "Stock_Code" : "Descript";
      sql += $@" {separator} {columnName} LIKE '%{searchParams.SearchValue}%' ";
    }

    if (!countOnly) sql += "ORDER BY StockCode ";

    if (searchParams.Length != null && searchParams.Length > 0 && !countOnly)
    {
      sql += $@" OFFSET {searchParams.Start} ROWS FETCH NEXT {searchParams.Length} ROWS ONLY; ";
    }

    return sql;
  }

  private string PrepareSqlQueryForStockItemsByDeptSubDeptAndSubDetailForBO(GetStockItemsByDeptSubDeptAndSubDetailDto searchParams, bool countOnly = false)
  {
    var sql = new StringBuilder();

    sql.AppendLine($@"
      SELECT
          {(countOnly ? "Count (MST.Stock_Code)" :
              $@"RTRIM(LTRIM(MST.Stock_Code)) StockCode,
          RTRIM(LTRIM(MST.Descript)) Description,
          RTRIM(LTRIM(MST.Dept)) Dept,
          RTRIM(LTRIM(MST.SUB_DEPT)) SubDept,
          RTRIM(LTRIM(MST.Sub_Detail)) SubDetail,
          RTRIM(LTRIM(MST.Vendor)) VendorId")}
      FROM 
          [CSCMaster].[dbo].[STOCKMST] AS MST
      Inner Join [CSCMaster].[dbo].Stock_Br ON Stock_br.Stock_code = MST.Stock_Code");

    string separator = "";
    bool haveAtLeastOneDeptOrSubDeptOrSubDetail = false;

    if (searchParams.IsShowAllProduct != true)
    {
      if ((searchParams.Depts!.Any() && searchParams.IncludeProductWithoutSubDeptOrSubDetail == true) || searchParams.SubDepts?.Count > 0 || searchParams.SubDetails?.Count > 0)
      {
        haveAtLeastOneDeptOrSubDeptOrSubDetail = true;
        sql.AppendLine(" WHERE ( ");

        if (searchParams.Depts?.Count > 0 && searchParams.IncludeProductWithoutSubDeptOrSubDetail == true)
        {
          foreach (Department dept in searchParams.Depts)
          {
            sql.AppendLine($"{separator} (Dept = '{dept.DeptId}' AND (Sub_Dept Is Null Or Sub_Dept = '') AND (Sub_Detail Is Null Or Sub_Detail = ''))");
            separator = " OR ";
          }
        }

        if (searchParams.SubDepts?.Count > 0)
        {
          separator = (searchParams.Depts!.Any() && searchParams.IncludeProductWithoutSubDeptOrSubDetail == true) ? " OR " : " ";
          foreach (SubDepartment subDept in searchParams.SubDepts)
          {
            sql.AppendLine($"{separator} (Dept = '{subDept.DeptId}' AND Sub_Dept = '{subDept.SubDeptId}' AND (Sub_Detail Is Null Or Sub_Detail = ''))");
            separator = " OR ";
          }
        }

        if (searchParams.SubDetails!.Any())
        {
          separator = (searchParams.SubDepts!.Any()) ? " OR " : " ";
          foreach (SubDetail subDetail in searchParams.SubDetails)
          {
            sql.AppendLine($"{separator} (Dept = '{subDetail.DeptId}' AND Sub_Dept = '{subDetail.SubDeptId}' AND Sub_Detail = '{subDetail.SubDetailId}' )");
            separator = " OR ";
          }
        }

        sql.AppendLine(" ) ");
      }
    }

    if (searchParams.SearchValue != null)
    {
      separator = haveAtLeastOneDeptOrSubDeptOrSubDetail ? " AND " : " WHERE ";
      string columnName = (searchParams.SearchColumnName == "Product ID") ? "MST.Stock_Code" : "MST.Descript";
      sql.AppendLine($@" {separator} {columnName} LIKE '%{searchParams.SearchValue}%' ");
    }

    if (!countOnly) sql.AppendLine(" ORDER BY StockCode ");

    if (searchParams.Length != null && searchParams.Length > 0 && !countOnly)
    {
      sql.AppendLine($@"OFFSET {searchParams.Start} ROWS FETCH NEXT {searchParams.Length} ROWS ONLY; ");
    }

    return sql.ToString();
  }

  private string PrepareSqlQueryForStockItemsByVendor(GetStockItemsByVendorDto searchParams, bool countOnly = false)
  {
    bool isBackOffice = _applicationConfig.IsBackOffice;

    var sql = new StringBuilder();
    string distinct = "DISTINCT";

    if (searchParams.DataTransferType == DataTransferType.LabelPrinting || (_applicationConfig.IsBackOffice && searchParams.DataTransferType == DataTransferType.CostChange))
      distinct = "";

    sql.AppendLine($@"
                  SELECT
                      {(countOnly ? $"Count( {distinct} StockMst.Stock_Code)" :
                  @$"{distinct}
                      RTRIM(LTRIM(StockMst.Stock_Code)) StockCode, 
                      RTRIM(LTRIM(StockMst.Descript)) Description,
                      RTRIM(LTRIM(StockMst.Dept)) Dept, 
                      RTRIM(LTRIM(StockMst.SUB_DEPT)) SubDept,
                      RTRIM(LTRIM(StockMst.Sub_Detail)) SubDetail")}
                  FROM 
                      {(isBackOffice ?
                      @"( [CSCMaster].[dbo].stockmst Inner Join [CSCMaster].[dbo].Stock_Br  ON Stock_br.Stock_code = Stockmst.Stock_Code ) 
                      INNER JOIN [CSCMaster].[dbo].stockcosts ON stockmst.stock_code = stockcosts.stock_code "
                      : "StockMst, StockCosts ")}
                  WHERE
                      {(isBackOffice ? $"StockCosts.VendorId = '{searchParams.VendorId}' "
                      : $"StockMst.Stock_Code = StockCosts.Stock_Code AND StockCosts.VendorId = '{searchParams.VendorId}' ")}");

    if (searchParams.DataTransferType == DataTransferType.LabelPrinting)
      sql.AppendLine($@"AND (StockMst.Stock_type = 'V' OR StockMst.Stock_type = 'F')");

    if (searchParams.SearchValue != null)
    {
      string columnName = searchParams.SearchColumnName == "Product ID" ? "Stock_Code" : "Descript";
      sql.AppendLine($@"AND StockMst.{columnName} LIKE '{searchParams.SearchValue}%' ");
    }

    if (!countOnly) sql.AppendLine(" ORDER BY StockCode ");

    if (searchParams.Length != null && searchParams.Length > 0 && !countOnly)
    {
      sql.AppendLine($@"OFFSET {searchParams.Start} ROWS FETCH NEXT {searchParams.Length} ROWS ONLY; ");
    }

    return sql.ToString();
  }

  private string PrepareSqlQueryForStockItems(SearchCriteria criteria, bool countOnly = false)
  {
    string sql = @$"SELECT
          {(countOnly ? "COUNT(MST.[STOCK_CODE]) " :
        @$"{(criteria.VendorPartSelected
        ? "VendorParts.Vendorid Vendor, "
        : @" "
        )}  
	        LTRIM(RTRIM(MST.Stock_Code)) StockCode,
	        MST.Descript Description,
	        MST.Format Format, MST.Vendor Vendor,
	        UM.Description UnitOfMeasure,
          {(criteria.ShowDetails
        ? " "
        : @"DPT.DEPT_NAME Department,
              SubDPT.SUB_NAME SubDepartment, "
        )}        
          COSTS.Cost Cost, 
			    STOCK_PRICES.price Price,
			    Stock_Br.AVAIL Available,
          CASE WHEN KIT_MAST.KIT_CODE IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END IsKit,
          CASE WHEN Stock_Br.ActiveStock IS NULL THEN CAST(1 AS BIT) ELSE Stock_Br.ActiveStock END IsActive ")}";

    if (criteria.VendorPartSelected && !countOnly) sql += ", [CSCMaster].[dbo].[VendorParts].PartNo VendorPart ";

    sql += @$"FROM [CSCMaster].[dbo].[STOCKMST] AS MST
            {(criteria.ShowDetails
            ? " " : @"LEFT JOIN [CSCMaster].[dbo].[DEPT] AS DPT ON MST.DEPT = DPT.DEPT 
                      LEFT JOIN [CSCMaster].[dbo].[SubDept] AS SubDPT ON MST.SUB_DEPT = SubDPT.SUB_DEPT and MST.DEPT = SubDPT.DEPT "
          )}  
					        
            LEFT JOIN stock_prices on MST.stock_code = stock_prices.stock_code 
            {(_applicationConfig.IsBackOffice
            ? " and stock_prices.vendorid = MST.vendor and stock_prices.price_number = 1 and stock_prices.orderno = 0 "
            : criteria.ShowDetails
            ? " and stock_prices.storeid = 'ALL' and stock_prices.vendorid='ALL' "
            : " and stock_prices.storeid = '*' and stock_prices.vendorid='*' "
          )}
						LEFT JOIN Stock_Br on MST.stock_code = stock_br.stock_code 
						LEFT JOIN [CSCMaster].[dbo].[StockCosts] AS COSTS ON MST.stock_code = COSTS.stock_code 
              {(_applicationConfig.IsBackOffice
              ? " AND MST.vendor = COSTS.vendorid AND COSTS.orderno = 0 "
              : criteria.ShowDetails
              ? " and COSTS.storeid = 'ALL' and COSTS.vendorid='ALL' "
              : " and COSTS.storeid = '*' and COSTS.vendorid='*' "
            )}
	          LEFT JOIN dbo.KIT_MAST ON KIT_MAST.KIT_CODE = MST.stock_code
					  LEFT JOIN  [CSCMaster].[dbo].[UM] AS UM ON MST.UM = UM.UMID ";

    if (criteria.AlternateCodeSearchText != "") sql += "INNER JOIN [CSCMaster].[dbo].plumast on MST.stock_code = plumast.plu_prim ";
    if (criteria.VendorPartSelected == true) sql += "INNER JOIN [CSCMaster].[dbo].VendorParts ON MST.stock_code = VendorParts.Stock_Code ";
    if (criteria.TaxExemptItemsOnly != "%") sql += "INNER JOIN [CSCMaster].[dbo].productTaxExempt ON MST.[STOCK_CODE] = [ProductTaxExempt].[ProductKey] ";
    if (criteria.InactiveItemsOnly != "%") sql += "INNER JOIN [CSCMaster].[dbo].Stock_br AS BR ON MST.[STOCK_CODE] = BR.[Stock_code] ";

    sql += " WHERE MST.Stock_Code LIKE @StockCode ";

    if (criteria.SelectVendor != "%") sql += "AND Vendor LIKE @SelectVendor ";
    if (criteria.DescriptionSearchText != "%") sql += "AND MST.Descript LIKE @StockDescription ";
    if (criteria.SelectPriceType != "%") sql += "AND PR_TYPE LIKE @SelectPriceType ";
    if (criteria.SaleItemsOnly != "%") sql += "AND PR_TYPE LIKE @SaleItemsOnly ";
    if (criteria.KeywordSearchText != "%%") sql += "AND MST.Descript LIKE @KeywordSearchText ";
    if (criteria.AlternateCodeSearchText != "") sql += "AND plumast.plu_code LIKE @AlternateCodeSearchText ";
    if (criteria.VendorPartSearchText != "%") sql += "AND PARTNO LIKE @VendorPartSearchText ";
    if (criteria.InactiveItemsOnly != "%") sql += "AND BR.ActiveStock LIKE @InactiveItemsOnly ";
    if (criteria.SelectDepartment != "%") sql += "AND MST.DEPT LIKE @SelectDepartment ";
    if (criteria.SelectSubDepartment != "%") sql += "AND MST.SUB_DEPT LIKE @SelectSubDepartment ";
    if (criteria.SelectSubDetail != "%") sql += "AND Sub_Detail LIKE @SelectSubDetail ";
    if (criteria.ScalableItemsOnly != "%") sql += "AND MST.S_BY_WGHT LIKE @ScalableItemsOnly ";

    if (!countOnly)
    {
      sql += "ORDER by MST.stock_Code ";
    }

    return sql;
  }

  public async Task<string> GetStockName(string stockCode)
  {
    string sql = $"SELECT Descript FROM Stockmst WHERE Stock_code = '{stockCode}'";
    return await Connection.QuerySingleOrDefaultAsync<string>(sql, null, Transaction);
  }

  private object GetStockItemQueryParams(SearchCriteria criteria)
  {
    var parameters = new
    {
      StockCode = criteria.StockCodeSearchText,
      StockDescription = criteria.DescriptionSearchText,
      SelectPriceType = criteria.SelectPriceType,
      SelectVendor = criteria.SelectVendor,
      SaleItemsOnly = criteria.SaleItemsOnly,
      KeywordSearchText = criteria.KeywordSearchText,
      AlternateCodeSearchText = criteria.AlternateCodeSearchText,
      VendorPartSearchText = criteria.VendorPartSearchText,
      InactiveItemsOnly = criteria.InactiveItemsOnly,
      SelectDepartment = criteria.SelectDepartment,
      SelectSubDepartment = criteria.SelectSubDepartment,
      SelectSubDetail = criteria.SelectSubDetail,
      ScalableItemsOnly = criteria.ScalableItemsOnly
    };
    return parameters;
  }

  public async Task<bool> IsVendorActive(string stockCode, string storeId, string vendorId, short orderNo)
  {
    string sql = @$"SELECT storeid 
                      FROM StockCosts 
                      LEFT JOIN Vendors ON stockcosts.Vendorid = Vendors.Code 
                      WHERE stockcosts.stock_code = '{stockCode}' 
                        AND (activevendor = 1 OR Vendorid = '{vendorId}')
                        AND orderno = {orderNo} 
                        AND storeid = '{storeId}'";

    IEnumerable<string>? stockCosts = await Connection.QueryAsync<string>(sql, null, Transaction);
    return (stockCosts.Count() > 0);
  }

  public async Task<bool> UpdateStockMasterFromStockTransferAsync()
  {
    string sql = $@"UPDATE [CSCMaster].[dbo].[STOCKMST] set transfer = 0 
                      FROM [CSCTransfer].[dbo].[StockTransfer], [CSCMaster].[dbo].[STOCKMST]
                      where productid = stock_code and  trans_flag = 'O' and transfer =1 ";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<int> GetCountOfProductsAsync(string departmentId, string? subDepartmentId = null, string? detailId = null)
  {
    string sql = @"Select Count(*) as [Used] From [CSCMaster].[dbo].[STOCKMST] Where Dept = @DepartmentId";
    if (subDepartmentId != null)
    {
      sql += " AND Sub_Dept = @SubDepartmentId";
    }

    if (detailId != null)
    {
      sql += " AND Sub_Detail = @DetailId";
    }

    var parameters = new { DepartmentId = departmentId, SubDepartmentId = subDepartmentId, DetailId = detailId };
    int countOfItems = await Connection.QueryFirstOrDefaultAsync<int>(sql, parameters, Transaction);
    return countOfItems;
  }

  public async Task<bool> UpdateDeptSubDeptSubDetailsToNullInStockMasterAsync(string departmentId, string? subDepartmentId = null, string? detailId = null)
  {
    string? dept = null;
    string? subDept = null;
    string sql = @"Update [CSCMaster].[dbo].[STOCKMST] Set Dept = @Dept, Sub_Dept = @SubDept, Sub_Detail = NULL Where Dept = @DepartmentId";

    if (subDepartmentId != null)
    {
      dept = departmentId;
      sql += " AND Sub_Dept = @SubDepartmentId";
    }

    if (detailId != null)
    {
      subDept = subDepartmentId;
      sql += " AND Sub_Detail = @DetailId";
    }

    var parameters = new
    {
      DepartmentId = departmentId,
      SubDepartmentId = subDepartmentId,
      DetailId = detailId,
      Dept = dept,
      SubDept = subDept
    };

    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);

    return rowsAffected > 0;
  }

  public async Task<IEnumerable<StockCodesForModalDto>?> GetStockMstForPromotion(bool sellInactive)
  {
    bool isBackOffice = _applicationConfig.IsBackOffice;

    string sqlForBackOfficeFalseAndSellInactiveTrue = $@"Select LTRIM(RTRIM(Stock_Code)) as [StockCode], Descript as [Description] 
                                         from StockMst
                                         order by Stock_Code asc";

    string sqlForbackofficeFalseAndSellInactiveFalse = $@"Select LTRIM(RTRIM(stockmst.Stock_Code)) as [StockCode], Descript as [Description] 
                                          from StockMst  Inner Join Stock_Br 
                                          ON stockmst.stock_code = stock_br.stock_code 
                                          Where ( stock_br.activestock is null or Stock_br.activestock =1 ) 
                                          order by stockmst.stock_code asc";

    string sqlForBackOfficeTrueAndSellInactiveTrue = $@"Select LTRIM(RTRIM(Stockmst.Stock_Code)) as [StockCode],StockMst.Descript as [Description] 
                                                          FROM StockMst 
                                                          ORDER BY StockMst.Stock_Code";

    string sqlForBackOfficeTrueAndSellInactiveFalse = $@"Select LTRIM(RTRIM(stockmst.Stock_Code)) as [StockCode], Descript as [Description] 
                                                            from StockMst  Inner Join Stock_Br 
                                                            ON stockmst.stock_code = stock_br.stock_code 
                                                            Where ( stock_br.activestock is null or Stock_br.activestock =1 )
                                                            order by stockmst.stock_code asc";

    if (!isBackOffice && sellInactive)
    {
      return await Connection.QueryAsync<StockCodesForModalDto>(sqlForBackOfficeFalseAndSellInactiveTrue, null, Transaction);
    }
    else if (!isBackOffice && !sellInactive)
    {
      return await Connection.QueryAsync<StockCodesForModalDto>(sqlForbackofficeFalseAndSellInactiveFalse, null, Transaction);
    }
    else if (isBackOffice && sellInactive)
    {
      return await Connection.QueryAsync<StockCodesForModalDto>(sqlForBackOfficeTrueAndSellInactiveTrue, null, Transaction);
    }
    else if (isBackOffice && !sellInactive)
    {
      return await Connection.QueryAsync<StockCodesForModalDto>(sqlForBackOfficeTrueAndSellInactiveFalse, null, Transaction);
    }

    return null;
  }

  public async Task<IEnumerable<StockItem>> GetStockItemsByDepartmentSubDepartmentAndDetailAsync(DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    string sql = @"SELECT 
	                          LTRIM(RTRIM(Stock_Code)) StockCode, 
	                          Descript Description
                          FROM [CSCMaster].[dbo].[STOCKMST] 
                          WHERE Dept = @Department";

    if (departmentSubDepartmentAndDetail.SubDepartment != null)
    {
      sql += " AND SUB_DEPT = @SubDepartment";
    }

    if (departmentSubDepartmentAndDetail.Detail != null)
    {
      sql += " AND Sub_Detail = @Detail";
    }

    IEnumerable<StockItem> stockItems = await Connection.QueryAsync<StockItem>(sql, departmentSubDepartmentAndDetail, Transaction);
    return stockItems;
  }

  public async Task<bool> AddProductDiscountCodeAsync(string productDiscountCode, DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    string sql = @"Update [CSCMaster].[dbo].[STOCKMST] Set PROD_DISC = @ProductDiscountCode Where StockMst.Dept = @DepartmentId";


    if (departmentSubDepartmentAndDetail.SubDepartment != null)
    {
      sql += " AND SUB_DEPT = @SubDepartment";
    }

    if (departmentSubDepartmentAndDetail.Detail != null)
    {
      sql += " AND Sub_Detail = @Detail";
    }

    var parameters = new
    {
      ProductDiscountCode = productDiscountCode,
      DepartmentId = departmentSubDepartmentAndDetail.Department,
      SubDepartment = departmentSubDepartmentAndDetail.SubDepartment,
      Detail = departmentSubDepartmentAndDetail.Detail
    };

    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);

    return true;
  }

  public async Task<bool> DeleteProductDiscountCodeAsync(DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    string sql = @"Update [CSCMaster].[dbo].[STOCKMST] Set PROD_DISC = 0 Where StockMst.Dept = @Department";

    if (departmentSubDepartmentAndDetail.SubDepartment != null)
    {
      sql += " AND SUB_DEPT = @SubDepartment";
    }

    if (departmentSubDepartmentAndDetail.Detail != null)
    {
      sql += " AND Sub_Detail = @Detail";
    }

    int rowsAffected = await Connection.ExecuteAsync(sql, departmentSubDepartmentAndDetail, Transaction);

    return true;
  }

  public async Task<IEnumerable<StockItem>> GetAllStockMasterAsync()
  {
    const string sql = @"SELECT LTRIM(RTRIM(Stock_Code)) AS StockCode,
                                  Stock_Type AS StockType,
                                  DESCRIPT AS Description,
								                  STD_COST AS Cost, 
                                  DEPT AS Department 
                           FROM [CSCMaster].[dbo].[STOCKMST]";

    IEnumerable<StockItem> stockItems = await Connection.QueryAsync<StockItem>(sql, null, Transaction);
    return stockItems;
  }

  public async Task<bool> IsProductsExistAsync(string departmentId, string? subDepartmentId, string? detailId)
  {
    string sql = @"SELECT COUNT([STOCK_CODE]) 
                   FROM [CSCMaster].[dbo].[STOCKMST] 
                   WHERE DEPT = @Department 
                   AND (@SubDepartment IS NULL OR SUB_DEPT = @SubDepartment) 
                   AND (@SubDetail IS NULL OR Sub_Detail = @SubDetail)";

    var parameters = new
    {
      Department = departmentId,
      SubDepartment = subDepartmentId,
      SubDetail = detailId
    };

    int countOfProducts = await Connection.QuerySingleOrDefaultAsync<int>(sql, parameters, Transaction);
    return countOfProducts > 0;
  }

  public async Task<IEnumerable<VendorProductDto>> GetVendorProductsByStockCodes(List<string> stockCodes)
  {
    string sql = @"SELECT LTRIM(RTRIM(Stock_Code)) AS StockCode
                           ,Descript Description
                           ,UM UnitOfMeasure
                           ,OrderUnit
                           ,Packing
                           ,Dept Department
                           ,Sub_Dept SubDepartment
                           ,Sub_Detail SubDetail
                     FROM   [CSCMaster].[dbo].[StockMST]
                     WHERE  StockMST.Stock_Code IN @stockCodes ";

    var vendorProducts = await Connection.QueryAsync<VendorProductDto>(sql, new { stockCodes }, Transaction);
    return vendorProducts;
  }

  public async Task<IEnumerable<VendorProductDto>> GetVendorProducts(IList<VendorProductQueryParameterDto> vendorProductQueryParameters)
  {
    string sql = @"SELECT
                              LTRIM(RTRIM(stockmst.Stock_Code)) StockCode,
                              Stock_br.ActiveStock ActiveStock,
                              Descript Description,
                              UM UnitofMeasure,
                              OrderUnit,
                              Packing,
                              Dept Department,
                              Sub_Dept SubDepartment
                          FROM
                              [CSCMaster].[dbo].[STOCKMST]
                          INNER JOIN
                              [CSCMaster].[dbo].[StockCosts] ON Stockmst.Stock_code = Stockcosts.Stock_code
                          INNER JOIN
                              [CSCMaster].[dbo].[STOCK_BR] ON Stockmst.stock_code = stock_br.stock_code";

    if (vendorProductQueryParameters.Count() > 0)
    {
      sql += " WHERE (";
      bool isFirstIteration = true;
      foreach (VendorProductQueryParameterDto vendorProductQueryParameter in vendorProductQueryParameters)
      {
        if (!isFirstIteration)
        {
          sql += " OR";
        }

        isFirstIteration = false;
        sql += @$" (stockmst.stock_code = '{vendorProductQueryParameter.StockCode}'
                  and Stockcosts.Vendorid = '{vendorProductQueryParameter.VendorId}')";
      }
    }
    else
    {
      sql += " WHERE ((stockmst.stock_code = null and Stockcosts.Vendorid = null)";
    }

    sql += $@") AND Orderno = 0 AND (Stock_br.ActiveStock = 1 OR Stock_br.ActiveStock IS NULL);";

    IEnumerable<VendorProductDto> vendorProducts = await Connection.QueryAsync<VendorProductDto>(sql, null, Transaction);
    return vendorProducts;
  }

  public async Task<IEnumerable<OrderQuantityDataDto>> GetOrderQuantityDataAsync(IList<string> stockCodes)
  {
    const string sql = @"SELECT
                              LTRIM(RTRIM(Stockmst.Stock_Code)) StockCode,
                              UM UnitOfMeasure, 
                              Packing,
                              OrderUnit,
                              Min_Qty MinQuantity,
                              Max_Qty MaxQuantity,
                              Reord_Qty ReordQuantity,
                              In_stock InStock,
                              Vendor VendorId
                          FROM
                              [CSCMaster].[dbo].[STOCKMST]
                          INNER JOIN
                              [CSCMaster].[dbo].[STOCK_BR] ON StockMst.stock_code = Stock_Br.Stock_Code
                          WHERE
                              Stockmst.Stock_code IN @StockCodes";

    var parameters = new { StockCodes = stockCodes };
    IEnumerable<OrderQuantityDataDto> orderQuantityDatas = await Connection.QueryAsync<OrderQuantityDataDto>(sql, parameters, Transaction);
    return orderQuantityDatas;
  }

  public async Task<IEnumerable<DepartmentSubDepartmentAndDetailDto>> GetDeptSubDeptAndDetailDataByStockCodesAsync(IList<string> stockCodes)
  {
    const string sql = @"SELECT
	                          LTRIM(RTRIM([STOCK_CODE])) StockCode,
	                          [DEPT] Department,
	                          [SUB_DEPT] SubDepartment,
	                          [Sub_Detail] Detail
                            FROM [CSCMaster].[dbo].[STOCKMST]
                            Where Stock_code IN @StockCodes";

    var parameters = new { StockCodes = stockCodes };
    IEnumerable<DepartmentSubDepartmentAndDetailDto> departmentSubDepartmentAndDetails = await Connection.QueryAsync<DepartmentSubDepartmentAndDetailDto>(sql, parameters, Transaction);
    return departmentSubDepartmentAndDetails;
  }

  public async Task<IEnumerable<StockCodesForModalDto>> GetStockItemsByCriteriaAsync(StockItemsCriteriaDto criteria)
  {
    var selectStockCodesQuery = new StringBuilder();

    selectStockCodesQuery.AppendLine(@"
				SELECT
					LTRIM(RTRIM(ST.[STOCK_CODE])) AS [StockCode],
					ST.[DESCRIPT] AS [Description]
				FROM [CSCMaster].[dbo].[STOCKMST] AS ST
        INNER JOIN [CSCMaster].[dbo].[STOCK_BR] AS SR 
				ON LTRIM(RTRIM(ST.[STOCK_CODE])) = LTRIM(RTRIM(SR.[STOCK_CODE]))");

    bool andCondition = false;

    if (!criteria.SellInactivePolicy)
    {
      selectStockCodesQuery.AppendLine("WHERE (SR.[ACTIVESTOCK] IS NULL OR SR.[ACTIVESTOCK] = 1)");
      andCondition = true;
    }

    if (criteria.Department != criteria.AllOption)
    {
      selectStockCodesQuery.Append(andCondition ? "AND" : "WHERE");
      selectStockCodesQuery.AppendLine(" ST.[DEPT] = @Department");
      andCondition = true;
    }

    if (criteria.SubDept != criteria.AllOption)
    {
      selectStockCodesQuery.Append(andCondition ? "AND" : "WHERE");
      selectStockCodesQuery.AppendLine(" ST.[SUB_DEPT] = @SubDept");
      andCondition = true;
    }

    if (criteria.SubDetail != criteria.AllOption)
    {
      selectStockCodesQuery.Append(andCondition ? "AND" : "WHERE");
      selectStockCodesQuery.AppendLine(" ST.[SUB_DETAIL] = @SubDetail");
    }

    selectStockCodesQuery.Append("ORDER BY 1");

    return await Connection.QueryAsync<StockCodesForModalDto>(selectStockCodesQuery.ToString(), criteria, Transaction);
  }

  public async Task<bool> UpdatePriceTypeInStockKMasterTableAsync(StockSpecialPrice stockSpecialPrice)
  {
    string sql = $"UPDATE [CSCMaster].[dbo].[STOCKMST] SET Pr_Type=@PriceType,Pr_Units=@PriceUnits, Pr_From=@PriceFromQuantity, PR_To=@PriceToQuantity WHERE Stock_Code=@StockCode";
    int affectedRows = await Connection.ExecuteAsync(sql, stockSpecialPrice, Transaction);
    return affectedRows > 0;
  }

  public async Task<IEnumerable<StockItem>> GetAllStockItemsListAsync()
  {
    const string query = @"
      SELECT
	      LTRIM(RTRIM([STOCK_CODE])) AS [StockCode],
	      [DESCRIPT] AS [Description]
      FROM [CSCMaster].[dbo].[STOCKMST]
      ORDER BY STOCK_CODE";

    return await Connection.QueryAsync<StockItem>(query, null, Transaction);
  }

  public async Task<IEnumerable<StockMaster?>> GetStockMasterInfoAsync(string startDepartment, string endDepartment)
  {
    string sql = "SProc_StockMasterInfo";
    var stockMasterData = await Connection.QueryAsync<dynamic?>(
        sql,
        new
        {
          SDept = startDepartment ?? "",
          EDept = endDepartment ?? ""
        },
        Transaction,
        commandType: CommandType.StoredProcedure
    );

    IEnumerable<StockMaster?> stockMasterInfo = stockMasterData.Select(data => new StockMaster
    {
      StockCode = data.STOCK_CODE,
      Description = data.DESCRIPT,
      Department = data.DEPT,
      DepartmentName = data.DEPT_NAME,
      SubDepartment = data.SUB_DEPT,
      SubDepartmentName = data.SUB_NAME,
      SubDetail = data.Sub_Detail,
      SubDetailName = data.SubDetail_Name,
    });

    return stockMasterInfo;
  }
}
