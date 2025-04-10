namespace InfonetCSCR.Data.Repositories;

public class StockTakesAndReceivingRepository : BaseRepository, IStockTakesAndReceivingRepository
{
  public StockTakesAndReceivingRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> AddRecLineAsync(RecLine recLine)
  {
    const string sql = @"INSERT INTO [dbo].[REC_LINE]
                                ([REC_NUM]
                                ,[BRANCH]
                                ,[Inv_Num]
                                ,[STOCK_CODE]
                                ,[DESCRIPT]
                                ,[I_QTY_RECV]
                                ,[COST]
                                ,[AMOUNT]
                                ,[Assoc]
                                ,[Taxes]
                                ,[Total]
                                ,[Sysqty]
                                ,[Rec_Update]
                                ,[purpose]
                                ,[Print]
                                ,[ActiveVendorCode]
                                ,[InitialCount]
                                ,[DateCount]
                                ,[Sequence])
                          VALUES
                                (@RecNum
                                ,@Branch
                                ,@InvNum
                                ,@StockCode
                                ,@Descript
                                ,@Quantity
                                ,@Cost
                                ,@Amount
                                ,@Assoc
                                ,@Taxes
                                ,@Total
                                ,@SysQty
                                ,@RecUpdate
                                ,@Purpose
                                ,@Print
                                ,@ActiveVendorCode
                                ,@InitialCount
                                ,@DateCount
                                ,@Sequence)";

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), recLine, Transaction);
    return affectedRows > 0;
  }

  public async Task<IEnumerable<StockTakesAndReceiveDto>?> GetStockTakesAndReceiveDataAsync(StockTakesAndReceiveRequestDto stockTakesAndReceiveRequestDto)
  {
    string sql = BuildStockTakesAndReceivedQuery(stockTakesAndReceiveRequestDto, false);

    var parameters = new
    {
      Purpose = stockTakesAndReceiveRequestDto.Purpose,
    };

    IEnumerable<StockTakesAndReceiveDto> receiveData = await Connection.QueryAsync<StockTakesAndReceiveDto>(sql, parameters, Transaction);
    return receiveData;
  }

  public async Task<int> GetStockTakesAndReceiveDataCountAsync(StockTakesAndReceiveRequestDto stockTakesAndReceiveRequest)
  {
    string sql = BuildStockTakesAndReceivedQuery(stockTakesAndReceiveRequest, true);

    var parameters = new
    {
      Purpose = stockTakesAndReceiveRequest.Purpose,
    };

    int recieveDataCount = await Connection.QueryFirstOrDefaultAsync<int>(sql, parameters, Transaction);
    return recieveDataCount;
  }

  public async Task<IEnumerable<StockDto>?> GetStocksAsync(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    string sql = await BuildStockQuery(stockTakesAndRecievingDto);

    var parameters = stockTakesAndRecievingDto.SelectedMonth == 0 ? new
    {
      Vendor = stockTakesAndRecievingDto.Vendor,
      Department = stockTakesAndRecievingDto.Department,
      SubDepartment = stockTakesAndRecievingDto.SubDepartment,
      SubDetail = stockTakesAndRecievingDto.SubDetail,
    } : null;

    IEnumerable<StockDto> stocks = await Connection.QueryAsync<StockDto>(sql, parameters, Transaction);
    return stocks;
  }

  private string BuildStockTakesAndReceivedQuery(StockTakesAndReceiveRequestDto stockTakesAndReceiveRequest, bool isCount)
  {
    StringBuilder query = new StringBuilder();

    if (isCount)
    {
      query.Append(@"SELECT COUNT(*)");
    }
    else
    {
      query.Append(@"SELECT
                    Rec_Line.REC_NUM AS RecNum,
                    Rec_Line.DESCRIPT AS Description,
                    Rec_Line.I_QTY_RECV AS Quantity,
                    LTRIM(RTRIM(Rec_Line.STOCK_CODE)) AS StockCode,
                    Rec_Line.Rec_Update AS RecUpdate,
                    Rec_Line.Sequence");

      if (stockTakesAndReceiveRequest.Purpose == (int)StockCountType.StockReceiving)
      {
        query.Append(@",Rec_Line.Inv_Num AS Invoice,
                  CAST(Rec_Line.COST AS DECIMAL(15, 4)) AS Cost,
                  CAST(Rec_Line.AMOUNT AS DECIMAL(15, 2)) AS Amount");
      }
      else
      {
        query.Append(@",Rec_Line.SYSQty AS SystemQuantity,
                    Rec_Line.InitialCount AS InitialCount,
                    StockMst.Stock_Type AS StockType");
      }
    }

    query.Append(@" FROM REC_LINE");

    if (stockTakesAndReceiveRequest.Purpose == (int)StockCountType.StockTakes)
    {
      query.Append(@" INNER JOIN StockMst ON Rec_Line.Stock_Code = StockMst.Stock_Code");
    }

    query.Append(@" WHERE purpose = @Purpose");

    if (stockTakesAndReceiveRequest.SearchValue != null)
    {
      string columnName = stockTakesAndReceiveRequest.SearchColumnName == "Stock Code" ? "Stock_Code" : "Descript";
      query.Append($@" AND Rec_Line.{columnName} LIKE '{stockTakesAndReceiveRequest.SearchValue}%'");
    }

    if (!isCount)
    {
      query.AppendLine();
      query.Append(@"ORDER BY Rec_num, Sequence, StockCode");

      if (stockTakesAndReceiveRequest.Length > 0)
      {
        query.AppendLine();
        query.Append($"OFFSET {stockTakesAndReceiveRequest.Start} ROWS FETCH NEXT {stockTakesAndReceiveRequest.Length} ROWS ONLY");
      }
    }

    return query.ToString();
  }

  private async Task<string> BuildStockQuery(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    StringBuilder query = new StringBuilder();

    bool sellInactive = stockTakesAndRecievingDto.SellInactivePolicy;
    int selectedMonth = stockTakesAndRecievingDto.SelectedMonth;
    var vendor = stockTakesAndRecievingDto.Vendor;
    var subDepartment = stockTakesAndRecievingDto.SubDepartment;
    var subDetail = stockTakesAndRecievingDto.SubDetail;
    var department = stockTakesAndRecievingDto.Department;

    query.Append($@"SELECT DISTINCT
                LTRIM(RTRIM([STOCKMST].STOCK_CODE)) AS StockCode,
                [VendorParts].PartNo AS VendorPart,
                [STOCKMST].DESCRIPT AS Description
                    FROM
                        (((StockMst
                        INNER JOIN Stock_Br ON Stockmst.stock_code = stock_br.stock_code)
                        LEFT JOIN VendorParts ON (STOCKMST.STOCK_CODE = VendorParts.Stock_Code AND STOCKMST.Vendor = VendorParts.VendorID))
                        LEFT JOIN KIT_MAST ON [STOCKMST].[STOCK_CODE] = [KIT_MAST].[KIT_CODE])
                    WHERE
                        {(sellInactive ? "" : "(Stock_br.activeStock IS NULL OR stock_br.Activestock = 1) AND")}

                         (
                            ([KIT_MAST].[KIT_CODE] IS NULL AND [StockMst].[Stock_Type] = 'V')
                            OR
                            ([KIT_MAST].[KIT_CODE] IS NOT NULL AND [StockMst].[Stock_Type] = 'F')
                        )");

    if (selectedMonth == 0)
    {
      query.Append($@"
                        {(vendor == null || vendor == "All" ? " " : vendor == "Undefined" ? " AND ([StockMst].Vendor IS NULL OR [StockMst].Vendor ='') " : $" AND [StockMst].Vendor = @Vendor")}
                        {(department == null || department == "All" ? " " : department == "Undefined" ? " AND ([StockMst].Dept IS NULL OR [StockMst].Dept ='') " : $" AND [StockMst].Dept = @Department")}
                        {(subDepartment == null || subDepartment == "All" ? " " : subDepartment == "Undefined" ? " AND ([StockMst].Sub_Dept IS NULL OR [StockMst].Sub_Dept ='') " : $" AND [StockMst].Sub_Dept = @SubDepartment")}
                        {(subDetail == null || subDetail == "All" ? " " : subDetail == "Undefined" ? " AND ([StockMst].Sub_Detail IS NULL OR [StockMst].Sub_Detail ='') " : $" AND [StockMst].Sub_Detail = @SubDetail")}
                        ");
    }
    else
    {
      string whereClause = await GenerateWhereClause(selectedMonth);
      query.Append($@"{whereClause}");
    }

    query.Append(@"
                        AND [StockMst].[Dept] <> '01'
                    ORDER BY
                        StockCode
                    ");

    return query.ToString();
  }

  private async Task<string> BuildQueryForAddReclineAsync(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    StringBuilder query = new StringBuilder();
    int selectedMonth = stockTakesAndRecievingDto.SelectedMonth;
    var vendor = stockTakesAndRecievingDto.Vendor;
    var subDepartment = stockTakesAndRecievingDto.SubDepartment;
    var subDetail = stockTakesAndRecievingDto.SubDetail;
    var department = stockTakesAndRecievingDto.Department;

    query.Append($@"INSERT INTO Rec_Line (
                                Rec_Num, 
                                Branch, 
                                Stock_Code, 
                                DESCRIPT, 
                                SysQty, 
                                Purpose, 
                                Rec_update, 
                                DateCount, 
                                Cost, 
                                ActiveVendorCode
                )
                SELECT 
                      @BatchID, 
                      '01',
                      StockMst.Stock_Code,
                      Descript,
                      In_Stock,
                      2,
                      0,
                      GetDate(),
                      std_cost,
                      vendor
                FROM StockMst
                INNER JOIN
                      Stock_Br ON StockMst.Stock_Code = Stock_Br.Stock_Code
                LEFT JOIN
                      KIT_MAST ON[STOCKMST].[STOCK_CODE] = [KIT_MAST].[KIT_CODE]
                WHERE
                (
                      ([KIT_MAST].[KIT_CODE] IS NULL AND[StockMst].[Stock_Type] = 'V')
                      OR
                      ([KIT_MAST].[KIT_CODE] IS NOT NULL AND[StockMst].[Stock_Type] = 'F')
                ) 
                AND
                StockMst.Stock_Code NOT IN(
                    SELECT
                        Stock_Code
                    FROM
                        Rec_Line
                    WHERE
                        purpose = 2
                )");

    if (selectedMonth == 0)
    {
      query.Append($@"
                        {(vendor == null || vendor == "All" ? " " : vendor == "Undefined" ? " AND ([StockMst].Vendor IS NULL OR [StockMst].Vendor ='') " : $" AND [StockMst].Vendor = @Vendor")}
                        {(department == null || department == "All" ? " " : department == "Undefined" ? " AND ([StockMst].Dept IS NULL OR [StockMst].Dept ='') " : $" AND [StockMst].Dept = @Department")}
                        {(subDepartment == null || subDepartment == "All" ? " " : subDepartment == "Undefined" ? " AND ([StockMst].Sub_Dept IS NULL OR [StockMst].Sub_Dept ='') " : $" AND [StockMst].Sub_Dept = @SubDepartment")}
                        {(subDetail == null || subDetail == "All" ? " " : subDetail == "Undefined" ? " AND ([StockMst].Sub_Detail IS NULL OR [StockMst].Sub_Detail ='') " : $" AND [StockMst].Sub_Detail = @SubDetail")}
                        ");
    }
    else
    {
      string whereClause = await GenerateWhereClause(selectedMonth);
      query.Append($@"{whereClause}");
    }

    query.Append(@"
                        AND [StockMst].[Dept] <> '01'
                    ORDER BY
                        StockMst.Stock_Code
                    ");

    return query.ToString();
  }

  private async Task<string> GenerateWhereClause(int selectedMonth)
  {
    if (selectedMonth == 0)
    {
      return string.Empty;
    }

    StringBuilder whereClause = new StringBuilder();
    bool isFirst = true;

    string query = @"
        SELECT Dept, SubDept, SubDetail
        FROM InventorySelection
        WHERE SelMonth = @SelMonth;
    ";

    IEnumerable<dynamic> result = await Connection.QueryAsync(query, new { SelMonth = selectedMonth }, Transaction);

    foreach (var row in result)
    {
      string dept = row.Dept;
      string subDept = row.SubDept;
      string subDetail = row.SubDetail;

      string op = isFirst ? " AND (" : " OR ";
      isFirst = false;

      if (subDept == null && subDetail == null)
      {
        whereClause.AppendFormat("{0}([StockMst].Dept = '{1}')", op, dept);
      }
      else if (subDetail == null)
      {
        whereClause.AppendFormat("{0}([StockMst].Dept = '{1}' AND [StockMst].Sub_Dept = '{2}')", op, dept, subDept);
      }
      else
      {
        whereClause.AppendFormat("{0}([StockMst].Dept = '{1}' AND [StockMst].Sub_Dept = '{2}' AND [StockMst].Sub_Detail = '{3}')", op, dept, subDept, subDetail);
      }
    }

    if (!isFirst)
    {
      whereClause.Append(")");
    }

    return whereClause.ToString();
  }

  public async Task<int> GetSequenceAsync(int batchId)
  {
    string sql = $"SELECT MAX(Sequence) as Sequence from Rec_LIne where rec_num = @BatchId";

    int? maxSequence = await Connection.QuerySingleOrDefaultAsync<int?>(sql, new { BatchId = batchId }, Transaction);
    return (maxSequence ?? 0) + 1;
  }

  public async Task<decimal> GetSystemQuantityAsync(string stockCode)
  {
    string sql = $"SELECT In_Stock FROM Stock_Br WHERE Stock_Code = @StockCode";

    decimal? inStock = await Connection.QuerySingleOrDefaultAsync<decimal?>(sql, new { StockCode = stockCode }, Transaction);
    return inStock ?? 0;
  }

  public async Task<int> GetMaxRecNumFromRecHeadAsync(int purpose)
  {
    string sql = $"Select Max(rec_num) as recnum from Rec_head where Purpose = @Purpose";
    int? maxRecNum = await Connection.QuerySingleOrDefaultAsync<int?>(sql, new { Purpose = purpose }, Transaction);

    return maxRecNum ?? 1;
  }

  public async Task<int> GetMaxRecNumFromRecHeadSusAsync(int purpose)
  {
    string sql = $"Select Max(REC_NUM) as RecNum from Rec_head_sus where Purpose = @Purpose";
    int? maxRecNum = await Connection.QuerySingleOrDefaultAsync<int?>(sql, new { Purpose = purpose }, Transaction);

    return (maxRecNum ?? 0) + 1;
  }

  public async Task<int> GetMaxBatchIdFromStockHistAsync(int purpose)
  {
    string sql = $"SELECT MAX(BatchID) AS RecNum FROM StockHist WHERE Purpose = @Purpose";
    int? maxBatchId = await Connection.QuerySingleOrDefaultAsync<int?>(sql, new { Purpose = purpose }, Transaction);

    return (maxBatchId ?? 1) + 1;
  }

  public async Task<bool> IsRecHeadExistsAsync(int batchId, int purpose)
  {
    string sql = $"SELECT Count(*) FROM Rec_Head WHERE Rec_Num = @BatchId and Purpose = @Purpose";
    var parameters = new { BatchId = batchId, Purpose = purpose };
    int? count = await Connection.QuerySingleOrDefaultAsync<int?>(sql, parameters, Transaction);

    return count > 0;
  }

  public async Task<string> GetStoreAsync()
  {
    string sql = $"SELECT Store FROM [CSCAdmin].[dbo].[Setup_C]";
    string? store = await Connection.QuerySingleOrDefaultAsync<string?>(sql, null, Transaction);

    return store ?? "01";   // Similar to desktop behavior
  }

  public async Task<StockMstDto?> GetCostInfoAsync(string stockCode)
  {
    string sql = $@"SELECT LTRIM(RTRIM(SM.Stock_Code)) AS StockCode,
                           SC.cost AS Cost,
                           SC.VendorId,
                           SC.ActiveVendor
                    FROM StockMst AS SM,
                    (
                        SELECT Stock_Code, Cost, VendorID, ActiveVendor
                        FROM StockCosts
                        WHERE (VendorID=@Vendor AND OrderNo=0)
                        AND Stock_Code NOT IN
                        (
                            SELECT Stock_Code
                            FROM StockCosts
                            WHERE VendorID<>@Vendor AND OrderNo=0 AND ActiveVendor=1
                        )
                        AND Stock_Code=@StockCode
                        UNION
                        SELECT Stock_Code, Cost, VendorID, ActiveVendor
                        FROM StockCosts
                        WHERE VendorID<>@Vendor AND OrderNo=0 AND ActiveVendor=1
                        AND Stock_Code=@StockCode
                    ) AS SC
                    WHERE SC.Stock_Code = SM.Stock_Code";

    StockMstDto? costInfo = await Connection.QuerySingleOrDefaultAsync<StockMstDto?>(sql, new { Vendor = "ALL", StockCode = stockCode }, Transaction);

    return costInfo;
  }

  public async Task<StockCostDto?> GetCostInfoForSalesAsync(string stockCode)
  {
    string sql = $@"SELECT Cost, VendorID
                           FROM StockCosts
                    WHERE Stock_Code = @StockCode 
                    AND (
                    (VendorID = 'ALL' AND OrderNo = 0)
                    OR (
                    VendorID <> 'ALL' AND OrderNo = 0 AND ActiveVendor = 1 
                    AND Stock_Code NOT IN (SELECT Stock_Code FROM StockCosts WHERE VendorID <> 'ALL' AND OrderNo = 0 AND ActiveVendor = 1)
                    ))
                    UNION
                    SELECT Cost, VendorID
                    FROM StockCosts
                    WHERE Stock_Code = @StockCode 
                    AND VendorID <> 'ALL' AND OrderNo = 0 AND ActiveVendor = 1";

    StockCostDto? costInfo = await Connection.QueryFirstOrDefaultAsync<StockCostDto?>(sql, new { StockCode = stockCode }, Transaction);

    return costInfo;
  }

  public async Task<bool> AddRecHeadAsync(int batchId, int purpose, string recType)
  {
    const string sql = @"INSERT INTO Rec_Head(REC_NUM, REC_DATE, REC_TYPE, purpose) VALUES(@BatchId,GETDATE(),@RecType,@Purpose)";

    var parameters = new
    {
      BatchId = batchId,
      RecType = recType,
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<IEnumerable<RecLine?>> GetRecLinesAsync(ReclineDto reclineDto)
  {
    StringBuilder query = new();
    query.Append(@"SELECT [REC_NUM] AS RecNum
                          ,[BRANCH]
                          ,[Inv_Num] AS InvNum
                          ,LTRIM(RTRIM([STOCK_CODE])) AS StockCode
                          ,[DESCRIPT]
                          ,[I_QTY_RECV] AS Quantity
                          ,[COST]
                          ,[AMOUNT]
                          ,[Assoc]
                          ,[Taxes]
                          ,[Total]
                          ,[Sysqty]
                          ,[Rec_Update] AS RecUpdate
                          ,[purpose]
                          ,[Print]
                          ,[ActiveVendorCode]
                          ,[InitialCount]
                          ,[DateCount]
                          ,[Sequence]
                    FROM [CSCMaster].[dbo].[REC_LINE]
                    WHERE");

    if (!reclineDto.IsAllReclines)
    {
      query.Append(@" [STOCK_CODE] = @StockCode
                      AND [Sequence] = @Sequence AND");
    }

    query.Append(@" [purpose] = @Purpose
                    AND [REC_NUM] = @RecNum");

    var parameters = new DynamicParameters();

    if (!reclineDto.IsAllReclines)
    {
      parameters.Add("@StockCode", reclineDto.StockCode);
      parameters.Add("@Sequence", reclineDto.Sequence);
    }
      
    parameters.Add("@Purpose", reclineDto.Purpose);
    parameters.Add("@RecNum", reclineDto.BatchId);

    IEnumerable<RecLine?> recLines = await Connection.QueryAsync<RecLine?>(query.ToString(), parameters, Transaction);
    return recLines;
  }

  public async Task<IEnumerable<Tax>> GetTaxesAsync(string stockCode, string exceptedTax)
  {
    StringBuilder query = new();
    query.Append($@"SELECT LTRIM(RTRIM(A.Stock_Code)) AS StockCode, 
                           A.Tax_Name AS TaxName, 
                           A.Tax_Code AS TaxCode, 
                           B.Tax_Rate AS Rate
                    FROM StockTax AS A
                    INNER JOIN TaxRate AS B ON A.Tax_Name = B.Tax_Name AND A.Tax_Code = B.Tax_Code
                    INNER JOIN TAXMAST C ON B.Tax_Name = C.Tax_Name
                    WHERE B.Tax_Incl = 0 AND C.Tax_Active = 1
                      AND A.Stock_Code = @StockCode AND A.Tax_Name <> @TaxName");
    var parameters = new
    {
      StockCode = stockCode,
      TaxName = exceptedTax
    };

    IEnumerable<Tax> taxes = await Connection.QueryAsync<Tax>(query.ToString(), parameters, Transaction);
    return taxes;
  }

  public async Task<decimal?> GetAssocAsync(string stockCode)
  {
    StringBuilder query = new();
    query.Append($@"SELECT SUM(As_Price) AS Assoc
                    FROM Stock_ac AS A
                    LEFT OUTER JOIN Assoc AS B ON A.as_code = B.as_code
                    WHERE A.Stock_Code = @StockCode");

    decimal? assoc = await Connection.QuerySingleOrDefaultAsync<decimal?>(query.ToString(), new { StockCode = stockCode }, Transaction);
    return assoc;
  }

  public async Task<IEnumerable<AssocTaxDto>> GetAssocTaxesAsync(string stockCode, string exceptedTax)
  {
    StringBuilder query = new();
    query.Append($@"SELECT LTRIM(RTRIM(A.Stock_Code)) AS StockCode,
                           D.As_Price AS Price,
                           C.Tax_Name AS TaxName,
                           C.Tax_Rate AS Rate
                    FROM Stock_Ac AS A
                    INNER JOIN Assoc AS D ON A.as_code = D.as_code
                    INNER JOIN Assoc_Tax AS B ON A.As_Code = B.As_Code
                    INNER JOIN TaxRate AS C ON B.Tax_Name = C.Tax_Name AND B.Tax_code = C.Tax_Code
                    INNER JOIN TAXMAST M ON C.tax_name = M.Tax_name
                    WHERE C.Tax_Incl = 0 AND M.Tax_Active = 1
                      AND A.Stock_Code = @StockCode AND C.Tax_Name <> @TaxName");
    var parameters = new
    {
      StockCode = stockCode,
      TaxName = exceptedTax
    };

    IEnumerable<AssocTaxDto> assocTaxes = await Connection.QueryAsync<AssocTaxDto>(query.ToString(), parameters, Transaction);
    return assocTaxes;
  }

  public async Task<int> UpdateRecLineAsync(RecLine recLine, RecLine oldRecLine)
  {
    const string sql = @"UPDATE [dbo].[REC_LINE]
                          SET [REC_NUM] = @RecNum,
                              [BRANCH] = @Branch,
                              [Inv_Num] = @InvNum,
                              [STOCK_CODE] = @StockCode,
                              [DESCRIPT] = @Descript,
                              [I_QTY_RECV] = @Quantity,
                              [COST] = @Cost,
                              [AMOUNT] = @Amount,
                              [Assoc] = @Assoc,
                              [Taxes] = @Taxes,
                              [Total] = @Total,
                              [Sysqty] = @Sysqty,
                              [Rec_Update] = @RecUpdate,
                              [purpose] = @Purpose,
                              [Print] = @Print,
                              [ActiveVendorCode] = @ActiveVendorCode,
                              [InitialCount] = @InitialCount,
                              [DateCount] = @DateCount,
                              [Sequence] = @Sequence
                          WHERE ([REC_NUM] = @OldRecNum OR ([REC_NUM] IS NULL AND @OldRecNum IS NULL))
                            AND ([STOCK_CODE] = @OldStockCode OR ([STOCK_CODE] IS NULL AND @OldStockCode IS NULL))
                            AND ([Sequence] = @OldSequence OR ([Sequence] IS NULL AND @OldSequence IS NULL))";

    var parameters = new
    {
      RecNum = recLine.RecNum,
      Branch = recLine.Branch,
      InvNum = recLine.InvNum ?? (object)DBNull.Value,
      StockCode = recLine.StockCode,
      Descript = recLine.Descript,
      Quantity = recLine.Quantity,
      Cost = recLine.Cost,
      Amount = recLine.Amount,
      Assoc = recLine.Assoc,
      Taxes = recLine.Taxes,
      Total = recLine.Total,
      SysQty = recLine.SysQty,
      RecUpdate = recLine.RecUpdate,
      Purpose = recLine.Purpose,
      Print = recLine.Print,
      ActiveVendorCode = recLine.ActiveVendorCode,
      InitialCount = recLine.InitialCount,
      DateCount = recLine.DateCount,
      Sequence = recLine.Sequence,
      OldRecNum = oldRecLine.RecNum,
      OldBranch = oldRecLine.Branch,
      OldInvNum = oldRecLine.InvNum,
      OldStockCode = oldRecLine.StockCode,
      OldDescript = oldRecLine.Descript,
      OldQuantity = oldRecLine.Quantity,
      OldCost = oldRecLine.Cost,
      OldAmount = oldRecLine.Amount,
      OldAssoc = oldRecLine.Assoc,
      OldTaxes = oldRecLine.Taxes,
      OldTotal = oldRecLine.Total,
      OldSysQty = oldRecLine.SysQty,
      OldRecUpdate = oldRecLine.RecUpdate,
      OldPurpose = oldRecLine.Purpose,
      OldPrint = oldRecLine.Print,
      OldActiveVendorCode = oldRecLine.ActiveVendorCode,
      OldInitialCount = oldRecLine.InitialCount,
      OldDateCount = oldRecLine.DateCount,
      OldSequence = oldRecLine.Sequence
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows;
  }

  public async Task<bool> AddAllStocksToRecLineForStockReceivingAsync(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    var vendor = stockTakesAndRecievingDto.Vendor;
    var department = stockTakesAndRecievingDto.Department;
    var subDepartment = stockTakesAndRecievingDto.SubDepartment;
    var subDetail = stockTakesAndRecievingDto.SubDetail;
    var invoice = stockTakesAndRecievingDto.Invoice;
    var batchId = stockTakesAndRecievingDto.BatchId;

    string sql = $@"INSERT INTO Rec_Line (
                          Rec_Num,
                          Branch,
                          Stock_Code,
                          DESCRIPT,
                          I_QTY_RECV,
                          Cost,
                          ActiveVendorCode,
                          Inv_Num,
                          Purpose,
                          Rec_update
                        )
                        SELECT
                          @BatchId,
                          '01',
                          StockMst.Stock_Code,
                          StockMst.Descript,
                          0 AS I_QTY_RECV,
                          SC.Cost AS Cost,
                          SC.VendorID AS ActiveVendorCode,
                          @Invoice AS Inv_Num,
                          1 AS Purpose,
                          0 AS Rec_update
                        FROM
                          StockMst AS StockMst,
                          (
                            SELECT
                              StockCosts.Stock_Code,
                              Cost,
                              VendorID,
                              ActiveVendor
                            FROM
                              StockCosts
                            WHERE
                              (
                                VendorID = @VendorId
                                AND OrderNo = 0
                              )
                              AND StockCosts.Stock_Code NOT IN (
                                SELECT
                                  StockCosts.Stock_Code
                                FROM
                                  StockCosts
                                WHERE
                                  VendorID <> @VendorId
                                  AND OrderNo = 0
                                  AND ActiveVendor = 1
                              )
                            UNION
                            SELECT
                              StockCosts.Stock_Code,
                              Cost,
                              VendorID,
                              ActiveVendor
                            FROM
                              StockCosts
                            WHERE
                              VendorID <> @VendorId
                              AND OrderNo = 0
                              AND ActiveVendor = 1
                          ) AS SC
                        WHERE
                          SC.Stock_Code = StockMst.Stock_Code
                          AND StockMst.Stock_Code NOT IN (
                            SELECT
                              Rec_Line.Stock_Code
                            FROM
                              Rec_Line
                            WHERE
                              Purpose = 1)
                              {(vendor == null || vendor == "All" ? " " : vendor == "Undefined" ? " AND ([StockMst].Vendor IS NULL OR [StockMst].Vendor ='') " : $" AND [StockMst].Vendor = @Vendor")}
                              {(department == null || department == "All" ? " " : department == "Undefined" ? " AND ([StockMst].Dept IS NULL OR [StockMst].Dept ='') " : $" AND [StockMst].Dept = @Department")}
                              {(subDepartment == null || subDepartment == "All" ? " " : subDepartment == "Undefined" ? " AND ([StockMst].Sub_Dept IS NULL OR [StockMst].Sub_Dept ='') " : $" AND [StockMst].Sub_Dept = @SubDepartment")}
                              {(subDetail == null || subDetail == "All" ? " " : subDetail == "Undefined" ? " AND ([StockMst].Sub_Detail IS NULL OR [StockMst].Sub_Detail ='') " : $" AND [StockMst].Sub_Detail = @SubDetail")}
                              AND [StockMst].[Dept]<>(SELECT Dept FROM FuelDept);";

    var parameters = new
    {
      Invoice = invoice,
      BatchId = batchId,
      VendorId = "ALL",
      Vendor = vendor,
      Department = department,
      SubDepartment = subDepartment,
      SubDetail = subDetail,
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction, commandTimeout: 120);
    return affectedRows > 0;
  }

  public async Task<bool> AddAllStocksToRecLineForStockTakesAsync(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    string sql = await BuildQueryForAddReclineAsync(stockTakesAndRecievingDto);

    var parameters = new DynamicParameters();
    parameters.Add("@BatchId", stockTakesAndRecievingDto.BatchId);

    if (stockTakesAndRecievingDto.SelectedMonth == 0)
    {
      parameters.Add("@Vendor", stockTakesAndRecievingDto.Vendor);
      parameters.Add("@Department", stockTakesAndRecievingDto.Department);
      parameters.Add("@SubDepartment", stockTakesAndRecievingDto.SubDepartment);
      parameters.Add("@SubDetail", stockTakesAndRecievingDto.SubDetail);
    }

    int affectedRows = await Connection.ExecuteAsync(sql, parameters, Transaction, commandTimeout: 120);
    return affectedRows > 0;
  }

  public async Task<bool> DeleteRecLineAsync(StockTakesAndReceiveDto receiveDto)
  {
    string sql = $@"DELETE FROM Rec_Line
                    WHERE Stock_Code = @StockCode
                      {(receiveDto.RecNum == null && receiveDto.Sequence == null ?
                          " AND Purpose = @Purpose AND (I_QTY_RECV = 0 OR I_QTY_RECV IS NULL) " :
                          " AND Rec_Num = @RecNum AND sequence = @Sequence ")}";

    var parameters = new
    {
      StockCode = receiveDto.StockCode,
      RecNum = receiveDto.RecNum,
      Sequence = receiveDto.Sequence,
      Purpose = receiveDto.Purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> DeleteAllRecLineAsync(int purpose, bool deleteFromImport)
  {
    string sql = $@"DELETE FROM REC_LINE WHERE Purpose = @Purpose";  // Purpose = 1 for stock receiving

    if (deleteFromImport)
    {
      sql += $@" AND I_QTY_RECV IS NULL AND SYSQTY=0";
    }
    var parameters = new
    {
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction, 180);
    return affectedRows > 0;
  }

  public async Task<bool> DeleteAllRecHeadAsync(int purpose)
  {
    string sql = $@"DELETE FROM REC_Head WHERE Purpose = @Purpose";  // Purpose = 1 for stock receiving

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), new { Purpose = purpose }, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> InsertAllRecHeadToRecHeadSusAsync(int purpose)
  {
    string sql = $@"INSERT INTO REC_HEAD_SUS SELECT * From REC_HEAD WHERE Purpose = @Purpose";  // Purpose = 1 for stock receiving

    var parameters = new
    {
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> InsertAllRecLineToRecLineSusAsync(int purpose)
  {
    string sql = $@"INSERT INTO REC_LINE_SUS SELECT * From REC_LINE WHERE Purpose = @Purpose";  // Purpose = 1 for stock receiving

    var parameters = new
    {
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> InsertAllRecHeadSusToRecHeadAsync(int purpose)
  {
    string sql = $@"INSERT INTO REC_HEAD SELECT * From REC_HEAD_SUS WHERE Purpose = @Purpose";  // Purpose = 1 for stock receiving

    var parameters = new
    {
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> InsertAllRecLineSusToRecLineAsync(int purpose)
  {
    string sql = $@"INSERT INTO REC_LINE SELECT * From REC_LINE_SUS WHERE Purpose = @Purpose";  // Purpose = 1 for stock receiving

    var parameters = new
    {
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<int> GetRecLineCountAsync(int purpose)
  {
    string sql = $"Select count(*) as reccount from rec_line WHERE Purpose = @Purpose";

    var parameters = new
    {
      Purpose = purpose
    };

    int count = await Connection.QuerySingleOrDefaultAsync<int>(sql, parameters, Transaction);
    return count;
  }

  public async Task<bool> DeleteAllRecHeadSusAsync(int purpose)
  {
    string sql = $@"DELETE FROM Rec_Head_SUS WHERE Purpose = @Purpose";  // Purpose = 1 for stock receiving

    var parameters = new
    {
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> DeleteAllRecLineSusAsync(int purpose)
  {
    string sql = $@"DELETE FROM Rec_Line_SUS WHERE Purpose = @Purpose";  // Purpose = 1 for stock receiving

    var parameters = new
    {
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<int> GetRecLineSusCountAsync(int purpose)
  {
    string sql = $"Select count(*) as count FROM Rec_Line_SUS WHERE Purpose = @Purpose";

    var parameters = new
    {
      Purpose = purpose
    };

    int count = await Connection.QuerySingleOrDefaultAsync<int>(sql, parameters, Transaction);
    return count;
  }

  public async Task<IEnumerable<StockTakesAndReceivingReportDto>?> GetStockReceivingReportDataAsync()
  {
    StringBuilder query = new();
    query.Append($@"SELECT DISTINCT
                        RH.[REC_NUM] AS RecNum,
                        RH.[REC_DATE] AS RecDate,
                        RL.[Inv_Num] AS InvNum,
                        LTRIM(RTRIM(RL.[STOCK_CODE])) AS StockCode,
                        RL.[DESCRIPT] AS Descript,
                        RL.[I_QTY_RECV] AS IQtyRecv,
                        RL.[COST] AS Cost,
                        RL.[Assoc] AS Assoc,
                        RL.[Taxes] AS Taxes,
                        RL.[Total] AS Total,
                        RL.[Sequence] AS Sequence
                    FROM
                        CSCMaster.dbo.REC_HEAD AS RH
                        INNER JOIN CSCMaster.dbo.REC_LINE AS RL ON RH.[REC_NUM] = RL.[REC_NUM]
                    WHERE
                        RL.[Purpose] = 1
                    ORDER BY
                        RH.[REC_NUM] ASC,
                        RL.[Inv_Num] ASC");

    IEnumerable<StockTakesAndReceivingReportDto> reportData = await Connection.QueryAsync<StockTakesAndReceivingReportDto>(query.ToString(), null, Transaction);
    return reportData;
  }

  public async Task<IEnumerable<StockTakesAndReceivingReportDto>?> GetStockTakesReportDataAsync()
  {
    StringBuilder query = new();
    query.Append($@"SELECT
    CASE
        WHEN STOCKMST.STOCK_TYPE = 'F' THEN LTRIM(RTRIM(KIT_ITEM.STOCK_CODE))
        ELSE LTRIM(RTRIM(REC_LINE.STOCK_CODE))
    END AS GroupingColumn,
    REC_HEAD.REC_NUM AS RecNum,
    REC_HEAD.REC_DATE AS RecDate,
    LTRIM(RTRIM(REC_LINE.STOCK_CODE)) AS StockCode,
    REC_LINE.DESCRIPT AS Descript,
    REC_LINE.I_QTY_RECV AS IQtyRecv,
    REC_LINE.COST AS Cost,
    REC_LINE.Sysqty AS SystemQuantity,
    STOCKMST.STOCK_TYPE AS StockType,
    LTRIM(RTRIM(KIT_ITEM.STOCK_CODE)) AS KitItemStockCode,
    KIT_ITEM.QUANTITY AS Quantity,
    StockCosts.Cost AS StockCost,
    STOCK_BR.IN_STOCK AS InStock,
    0 AS QuantityDifference,
    0 AS QuantityDifferenceInDollar,
    COUNT(*) OVER(PARTITION BY
        REC_HEAD.REC_NUM,
        CASE
            WHEN STOCKMST.STOCK_TYPE = 'F' THEN KIT_ITEM.STOCK_CODE
            ELSE REC_LINE.STOCK_CODE
        END) AS GroupCount
    FROM
        CSCMaster.dbo.REC_HEAD REC_HEAD
    INNER JOIN CSCMaster.dbo.REC_LINE REC_LINE ON REC_HEAD.REC_NUM = REC_LINE.REC_NUM
    LEFT OUTER JOIN CSCMaster.dbo.Kit_Item KIT_ITEM ON REC_LINE.STOCK_CODE = KIT_ITEM.Kit_CODE
    INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON REC_LINE.STOCK_CODE = STOCKMST.STOCK_CODE
    LEFT JOIN stockcosts ON (stockcosts.stock_code = KIT_ITEM.stock_code AND stockcosts.vendorid = STOCKMST.vendor AND orderno = 0)
    LEFT JOIN stock_br ON (stock_br.stock_code = KIT_ITEM.stock_code)
    WHERE
        REC_LINE.purpose = 2
    ORDER BY
        REC_HEAD.REC_NUM ASC;");

    IEnumerable<StockTakesAndReceivingReportDto> reportData = await Connection.QueryAsync<StockTakesAndReceivingReportDto>(query.ToString(), null, Transaction);
    return reportData;
  }

  public async Task<bool> UpdateRecPrint(int purpose)
  {
    string sql = $@"UPDATE CSCMaster.dbo.Rec_Line SET [Print]=1
                  WHERE Purpose=@Purpose";
    var parameters = new
    {
      Purpose = purpose
    };

    int affectedRows = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return affectedRows > 0;
  }

  public async Task<IEnumerable<RecHead?>> GetAllRecHeadsAsync(int purpose)
  {
    StringBuilder query = new();
    query.Append($@"SELECT [REC_NUM] AS RecNum
                        ,[REC_DATE] AS RecDate
                        ,[REC_TYPE] AS RecType
                        ,[purpose] AS Purpose
                    FROM [CSCMaster].[dbo].[REC_HEAD]
                    WHERE Purpose = @Purpose ORDER BY Rec_num");

    IEnumerable<RecHead?> recHeads = await Connection.QueryAsync<RecHead?>(query.ToString(), new { Purpose = purpose }, Transaction);
    return recHeads;
  }

  public async Task<IEnumerable<RecLine>> GetAllRecLinesAsync(int purpose)
  {
    StringBuilder query = new();
    query.Append($@"SELECT [Rec_Line].[REC_NUM] AS RecNum,
                           [Rec_Line].[BRANCH],
                           [Rec_Line].[Inv_Num] AS InvNum,
                           LTRIM(RTRIM([Rec_Line].[STOCK_CODE])) AS StockCode,
                           [Rec_Line].[DESCRIPT],
                           [Rec_Line].[I_QTY_RECV] AS Quantity,
                           [Rec_Line].[COST],
                           [Rec_Line].[AMOUNT],
                           [Rec_Line].[Assoc],
                           [Rec_Line].[Taxes],
                           [Rec_Line].[Total],
                           [Rec_Line].[Sysqty],
                           [Rec_Line].[Rec_Update] AS RecUpdate,
                           [Rec_Line].[purpose],
                           [Rec_Line].[Print],
                           [Rec_Line].[ActiveVendorCode],
                           [Rec_Line].[InitialCount],
                           [Rec_Line].[DateCount],
                           [Rec_Line].[Sequence]
                    FROM REC_Line
                    INNER JOIN StockMst ON Rec_Line.stock_code = StockMst.stock_code
                    WHERE Purpose = @Purpose
                    ORDER BY StockMst.Stock_type DESC, StockCode");

    IEnumerable<RecLine> recLines = await Connection.QueryAsync<RecLine>(query.ToString(), new { Purpose = purpose }, Transaction);
    return recLines;
  }

  public async Task<IEnumerable<StockInfoDto?>> GetStockInfosByStockCodesAsync(List<string?> stockCodes)
  {
    if (stockCodes.Count == 0) return new List<StockInfoDto>();
    string encodedStockCodes = "'" + string.Join("', '", stockCodes) + "'";
    StringBuilder query = new();
    query.Append($@"SELECT RTRIM(LTRIM(A.Stock_Code)) AS StockCode,
                           A.Std_Cost AS StandardCost, 
                           B.In_Stock As InStock, 
                           B.Avail, 
                           B.Branch
                    FROM StockMst AS A 
                    INNER JOIN Stock_Br AS B ON A.Stock_Code = B.Stock_Code 
                    WHERE A.Stock_Code IN ({encodedStockCodes})");

    IEnumerable<StockInfoDto?> stockInfos = await Connection.QueryAsync<StockInfoDto?>(query.ToString(), null, Transaction);
    return stockInfos;
  }

  public async Task<bool> UpdatestockMastersStandardCostBatchAsync(List<Tuple<decimal, string>> listOfCostAndStockCodeTuples)
  {
    if (listOfCostAndStockCodeTuples.Count == 0) return false;
    StringBuilder sqlBuilder = new StringBuilder();
    sqlBuilder.AppendLine("UPDATE StockMst");
    sqlBuilder.AppendLine("SET Std_Cost = CASE");

    foreach (var tuple in listOfCostAndStockCodeTuples)
    {
      decimal cost = tuple.Item1;
      string stockCode = tuple.Item2;
      sqlBuilder.AppendLine($"    WHEN Stock_Code = '{stockCode}' THEN '{cost}'");
    }

    sqlBuilder.AppendLine("    ELSE Std_Cost");
    sqlBuilder.AppendLine("END");

    string encodedStockCodes = "'" + string.Join("', '", listOfCostAndStockCodeTuples.Select(tuple => tuple.Item2)) + "'";
    sqlBuilder.AppendLine($"WHERE Stock_Code IN ({encodedStockCodes});");

    string sql = sqlBuilder.ToString();

    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction, 180);
    return affectedRows > 0;
  }

  public async Task<IEnumerable<StockCostDto?>> GetStockCostByStockCodesAsync(List<string> stockCodes)
  {
    if (stockCodes.Count == 0) return new List<StockCostDto?>();
    string encodedStockCodes = "'" + string.Join("', '", stockCodes) + "'";
    StringBuilder query = new();
    query.Append($@"SELECT RTRIM(LTRIM(Stock_Code)) AS StockCode, Cost, VendorID, ActiveVendor
                    FROM StockCosts 
                    WHERE (VendorID = 'ALL' AND OrderNo = 0) AND Stock_Code NOT IN 
                          (SELECT Stock_Code FROM StockCosts 
                           WHERE VendorID <> 'ALL' AND OrderNo = 0 AND ActiveVendor = 1) AND Stock_Code IN ({encodedStockCodes}) 

                    UNION 

                    SELECT Stock_Code, Cost, VendorID, ActiveVendor
                    FROM StockCosts 
                    WHERE VendorID <> 'ALL' AND OrderNo = 0 AND ActiveVendor = 1 AND Stock_Code IN ({encodedStockCodes})");

    IEnumerable<StockCostDto?> stockInfos = await Connection.QueryAsync<StockCostDto?>(query.ToString(), null, Transaction);
    return stockInfos;
  }

  public async Task<bool> UpdateStockCostListAsync(List<StockCostInfoDto> costList)
  {
    if (costList.Count == 0) return false;

    string query = @$"UPDATE [CSCMaster].[dbo].[StockCosts]
                      SET Cost = @Cost
                      WHERE OrderNO = 0 AND Stock_Code = @StockCode AND VendorID = @VendorId";

    int affectedRows = await Connection.ExecuteAsync(query.ToString(), costList, transaction: Transaction, 180);

    return affectedRows > 0;
  }

  public async Task<bool> AddStockHistBatchAsync(List<StockHist> stockHists)
  {
    if (stockHists.Count == 0) return false;
    string sql = $@"INSERT INTO [CSCTrans].[dbo].[StockHist]
                          ([UserId]
                          ,[BatchDate]
                          ,[Batchid]
                          ,[StockCode]
                          ,[Description]
                          ,[Qty]
                          ,[BookQty]
                          ,[Purpose]
                          ,[Inv_Num]
                          ,[Cost]
                          ,[Amount]
                          ,[Assoc]
                          ,[Taxes]
                          ,[Total]
                          ,[Sequence])
                    VALUES
                          (@UserId
                          ,GetDate()
                          ,@BatchId
                          ,@StockCode
                          ,@Description
                          ,@Quantity
                          ,@BookQty
                          ,@Purpose
                          ,@InvNum
                          ,@Cost
                          ,@Amount
                          ,@Assoc
                          ,@Taxes
                          ,@Total
                          ,@Sequence)";

    int affectedRows = await Connection.ExecuteAsync(sql, stockHists, Transaction, 180);
    return affectedRows > 0;
  }

  public async Task<bool> UpdateInStockAndAvailInStockBrListAsync(List<(decimal inStock, decimal avail, string stockCode, string branch)> updates)
  {
    if (updates.Count == 0) return false;

    StringBuilder query = new StringBuilder();
    query.AppendLine(@"UPDATE [CSCMaster].[dbo].[Stock_Br]");
    query.AppendLine(@"SET In_Stock = CASE");

    foreach (var update in updates)
    {
      query.AppendLine($"    WHEN STOCK_CODE = '{update.stockCode}' AND BRANCH = '{update.branch}' THEN {update.inStock}");
    }

    query.AppendLine("    ELSE In_Stock");
    query.AppendLine("END,");
    query.AppendLine("Avail = CASE");

    foreach (var update in updates)
    {
      query.AppendLine($"    WHEN STOCK_CODE = '{update.stockCode}' AND BRANCH = '{update.branch}' THEN {update.avail}");
    }

    query.AppendLine("    ELSE Avail");
    query.AppendLine("END");
    query.AppendLine($"WHERE STOCK_CODE IN ({string.Join(", ", updates.Select(update => $"'{update.stockCode}'"))})");
    query.AppendLine($"AND BRANCH IN ({string.Join(", ", updates.Select(update => $"'{update.branch}'"))});");

    string sql = query.ToString();

    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction, 180);
    return affectedRows > 0;
  }

  public async Task<IEnumerable<StockReceivingExportDto>?> GetExportDataAsync(int purpose)
  {
    string sql = $@"SELECT LTRIM(RTRIM(A.Stock_Code)) AS StockCode, Descript AS Description, SYSQTY AS SysQuantity, Cost AS Cost, Price
                    FROM Rec_Line AS A 
                    INNER JOIN Stock_Prices AS B 
                        ON ((A.Stock_Code = B.Stock_Code AND A.ActiveVendorCode = B.VendorID AND A.ActiveVendorCode <> '') 
                        OR (A.Stock_Code = B.Stock_Code AND B.VendorID = 'ALL' AND A.ActiveVendorCode = '')) 
                    WHERE B.Price_Number = 1 AND B.OrderNo = 0 
                    AND A.Purpose = @Purpose 
                    ORDER BY A.Sequence";

    IEnumerable<StockReceivingExportDto>? stockReceivingExportData = await Connection.QueryAsync<StockReceivingExportDto>(sql, new { Purpose = purpose }, Transaction);
    return stockReceivingExportData;
  }

  public async Task<Header> GetHeaderByPurposeAsync(int purpose)
  {
    string sql = $@"SELECT *
                    FROM Header
                    WHERE Purpose = @Purpose
                    ORDER BY ImportNumber";

    var parameter = new
    {
      Purpose = purpose
    };
    Header header = await Connection.QueryFirstOrDefaultAsync<Header>(sql, parameter, Transaction);
    return header;
  }

  public async Task<IEnumerable<Fields>> GetFieldsByImportNumberAsync(int importNumber)
  {
    string sql = $@"SELECT *
                    FROM [dbo].[Fields]
                    WHERE ImportNumber = @ImportNumber
                    ORDER BY Item;";

    var parameter = new
    {
      ImportNumber = importNumber
    };
    IEnumerable<Fields> header = await Connection.QueryAsync<Fields>(sql, parameter, Transaction);
    return header;
  }

  public async Task<IEnumerable<Fields>> GetFieldsByImportNumberAndImportTypeAsync(int lastItem, int importNumber)
  {

    string sql = $@"SELECT ImportNumber,
                             Item,
                             Name,
                             DataType,
                             Required,
                             Start,
                             Len
                  FROM [CSCMaster].[dbo].[Fields]
                  WHERE ImportNumber = @ImportNumber
                  UNION
                  SELECT ImportNumber,
                         @LastItem AS Item,
                         'ENDCOUNT' AS Name,
                         'A' AS DataType,
                         1 AS Required,
                         0 AS Start,
                         0 AS Len
                  FROM [CSCMaster].[dbo].[Fields]
                  WHERE ImportNumber = @ImportNumber
                  ORDER BY Item;";

    var parameter = new
    {
      ImportNumber = importNumber,
      LastItem = lastItem
    };

    IEnumerable<Fields> header = (await Connection.QueryAsync<Fields>(sql, parameter, Transaction));
    return header;
  }

  public async Task<int> GetLastItemAsync(int importNumber)
  {
    string sql = $@"SELECT Max(Item) AS LastItem FROM Fields 
           WHERE ImportNumber = @ImportNumber";

    var parameters = new
    {
      ImportNumber = importNumber
    };

    int? lastItem = await Connection.QuerySingleOrDefaultAsync<int>(sql, parameters, Transaction);
    return (lastItem ?? 0) + 1;
  }

  public async Task<int> GetCountOfInventoryDataForMonth(int month)
  {
    string sql = $"SELECT count(*) FROM InventorySelection WHERE SelMonth = @month";

    var paramaters = new
    {
      month = month
    };

    int count = await Connection.QuerySingleOrDefaultAsync<int>(sql, paramaters, Transaction);
    return count;
  }

  public async Task<int> UpdateReclineForSalesAsync(string stockCode, int purpose, decimal cost, string costType)
  {
    string sql;
    int affectedRows;

    if (costType == "Standard")
    {
      sql = $@"UPDATE [CSCMaster].[dbo].[REC_LINE]
                       SET [Cost] = @Cost
                       WHERE [Purpose] = @Purpose AND Stock_Code = @StockCode";

      var parameters = new
      {
        Cost = cost,
        Purpose = purpose,
        StockCode = stockCode
      };

      affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    }
    else
    {
      sql = $@"UPDATE [CSCMaster].[dbo].[REC_LINE]
               SET [Cost] = B.Avg_Cost
               FROM Rec_Line AS A
               INNER JOIN StockMst AS B ON A.Stock_Code = B.Stock_Code
               WHERE A.Purpose = 2 AND A.Stock_Code = @StockCode";

      var parameters = new
      {
        StockCode = stockCode
      };

      affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);
    }

    return affectedRows;
  }

  public async Task<RecLine?> GetReclineByStockCOde(string stockCode, int purpose)
  {
    const string sql = $@"SELECT * FROM Rec_Line
                        WHERE Stock_Code = @StockCode
                        AND purpose = @Purpose;";

    var parameters = new
    {
      StockCode = stockCode,
      Purpose = purpose
    };

    RecLine? recLine = await Connection.QueryFirstOrDefaultAsync<RecLine>(sql, parameters, Transaction);

    return recLine;
  }

  public async Task<RecLine?> GetReclineByBatchIdAsync(string stockCode, int purpose, int batchId)
  {
    const string sql = $@"SELECT [Rec_Line].[REC_NUM] AS RecNum,
                           [Rec_Line].[BRANCH],
                           [Rec_Line].[Inv_Num] AS InvNum,
                           LTRIM(RTRIM([Rec_Line].[STOCK_CODE])) AS StockCode,
                           [Rec_Line].[DESCRIPT],
                           [Rec_Line].[I_QTY_RECV] AS Quantity,
                           [Rec_Line].[COST],
                           [Rec_Line].[AMOUNT],
                           [Rec_Line].[Assoc],
                           [Rec_Line].[Taxes],
                           [Rec_Line].[Total],
                           [Rec_Line].[Sysqty],
                           [Rec_Line].[Rec_Update] AS RecUpdate,
                           [Rec_Line].[purpose],
                           [Rec_Line].[Print],
                           [Rec_Line].[ActiveVendorCode],
                           [Rec_Line].[InitialCount],
                           [Rec_Line].[DateCount],
                           [Rec_Line].[Sequence]
                        FROM REC_Line
                        WHERE Stock_Code = @StockCode
                        AND purpose = @Purpose
                        AND Rec_Num = @BatchId;";

    var parameters = new
    {
      StockCode = stockCode,
      Purpose = purpose,
      BatchId = batchId
    };

    RecLine? recLine = await Connection.QueryFirstOrDefaultAsync<RecLine>(sql, parameters, Transaction);

    return recLine;
  }

  public async Task<decimal> CalculateSalesQuantity(string stockCode, DateTime dateCount)
  {
    string sql = $@"SELECT SUM(A.Quantity) AS Qty
                    FROM SaleLine AS A
                    INNER JOIN SaleHead AS B ON A.Sale_No = B.Sale_No
                    WHERE A.Stock_Code = @StockCode
                    AND CONVERT(DATETIME, B.SD, 20) >= @DateCount";

    var parameters = new
    {
      StockCode = stockCode,
      DateCount = dateCount
    };

    decimal quantity;
    decimal? result = await Connection.QueryFirstOrDefaultAsync<decimal?>(sql, parameters, Transaction);
    quantity = result ?? 0;

    return quantity;
  }

  public async Task<List<(string stockCode, decimal quantity)>?> GetSalesQuantityListAsync(List<(string StockCode, DateTime DateCount)> parameters)
  {
    string sql = $@"SELECT LTRIM(RTRIM(A.Stock_Code)) AS stockCode, SUM(A.Quantity) AS quantity
                    FROM SaleLine AS A
                    INNER JOIN SaleHead AS B ON A.Sale_No = B.Sale_No
                    WHERE";
    for (int i = 0; i < parameters.Count(); i++)
    {
      sql += $@"
                    A.Stock_Code = '{parameters[i].StockCode}'
                    AND CONVERT(DATETIME, B.SD, 20) >= '{parameters[i].DateCount}'";
      if ((i + 1) < parameters.Count()) sql += " OR ";
    }
    sql += @"
                    GROUP BY A.Stock_Code";


    IEnumerable<(string stockCode, decimal quantity)>? result = await Connection.QueryAsync<(string stockCode, decimal quantity)>(sql, null, Transaction);
    return result.ToList();
  }

  public async Task<int> UpdateReclineForSalesQuantityAsync(ReclineDto reclineDto)
  {
    int affectedRows;

    string sql = BuildQueryForReclineUpdate(reclineDto);

    var parameters = new DynamicParameters();
    parameters.Add("@InStock", reclineDto.InStock);
    parameters.Add("@StockCode", reclineDto.StockCode);
    parameters.Add("@Purpose", reclineDto.Purpose);
    parameters.Add("@BatchID", reclineDto.BatchId);
    parameters.Add("@SaleQty", reclineDto.SalesQuantity);

    if (reclineDto.Cost != null && reclineDto.VendorId != null)
    {
      parameters.Add("@Cost", reclineDto.Cost);
      parameters.Add("@VendorId", reclineDto.VendorId);
    }

    if (reclineDto.UpdateInitial)
    {
      parameters.Add("@QuantityReceived", reclineDto.Quantity);
    }
    else
    {
      parameters.Add("@InitialCount", reclineDto.InitialCount);
    }

    affectedRows = await Connection.ExecuteAsync(sql.ToString(), parameters, Transaction);

    return affectedRows;
  }

  private string BuildQueryForReclineUpdate(ReclineDto reclineDto)
  {
    string updateQuery = "UPDATE [CSCMaster].[dbo].[REC_LINE] SET " +
            "[sysqty] = @InStock";

    if (reclineDto.Cost != null && reclineDto.VendorId != null)
    {
      updateQuery += ", [Cost] = @Cost, [ActiveVendorCode] = @VendorID";
    }

    if (reclineDto.UpdateInitial)
    {
      updateQuery += ", [InitialCount] = @QuantityReceived ";
    }

    updateQuery += ", [I_QTY_RECV] = ";

    if (reclineDto.UpdateInitial)
    {
      if (reclineDto.Quantity.HasValue)
      {
        updateQuery += "@QuantityReceived - @SaleQty";
      }
      else
      {
        updateQuery += "NULL";
      }
    }
    else
    {
      if (reclineDto.InitialCount.HasValue)
      {
        updateQuery += "@InitialCount - @SaleQty";
      }
      else
      {
        updateQuery += "NULL";
      }
    }

    updateQuery += " WHERE [Stock_Code] = @StockCode AND [Purpose] = @Purpose AND [Rec_Num] = @BatchID";

    return updateQuery;
  }

  public async Task<int> UpdateQuantityAsync(ReclineDto reclineDto)
  {
    int affectedRows;
    StringBuilder query = new StringBuilder();
    query.AppendLine(@"UPDATE [CSCMaster].[dbo].[REC_LINE]");
    query.AppendLine(@"SET I_QTY_RECV = @Quantity");

    if (reclineDto.DateCount == null)
    {
      query.AppendLine(@", DateCount = GetDate()");
      query.AppendLine(@"WHERE Stock_Code = @StockCode;");
    }
    else
    {
      query.AppendLine(@",DateCount = @DateCount");
      query.AppendLine(@"WHERE Stock_Code = @StockCode AND Purpose = @Purpose AND Rec_Num = @BatchID;");
    }

    var parameters = new DynamicParameters();
    parameters.Add("@Quantity", reclineDto.Quantity);
    parameters.Add("@StockCode", reclineDto.StockCode);

    if (reclineDto.DateCount != null)
    {
      parameters.Add("@DateCount", reclineDto.DateCount);
      parameters.Add("@Purpose", reclineDto.Purpose);
      parameters.Add("@BatchID", reclineDto.BatchId);
    }

    affectedRows = await Connection.ExecuteAsync(query.ToString(), parameters, Transaction);
    return affectedRows;
  }

  public async Task<int> UpdateReclineListAsync(List<RecLine> parameters)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[REC_LINE]
                        SET
                            Cost = COALESCE(@Cost, Cost),
                            ActiveVendorCode = COALESCE(@ActiveVendorCode, ActiveVendorCode),
                            InitialCount = COALESCE(@InitialCount, InitialCount),
                            I_QTY_RECV= COALESCE(@Quantity, I_QTY_RECV)
                        WHERE Stock_Code = @StockCode
                          AND REC_NUM = @RecNum
                          AND Purpose = @Purpose
    ";

    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected;
  }

  public async Task<int> GetNotPrintedRecLinesCount(int purpose)
  {
    string sql = $"SELECT COUNT(*) AS CNT FROM Rec_Line WHERE [Rec_Line].[Print] = 0 AND Purpose = @Purpose";

    int count = await Connection.QuerySingleOrDefaultAsync<int>(sql, new { Purpose = purpose }, Transaction);
    return count;
  }

  public async Task<bool> UpdateInvoiceNumber(ReclineDto reclineDto)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[REC_LINE]
                      SET Inv_Num = @InvNum 
                      WHERE Stock_Code = @StockCode AND Rec_Num = @RecNum AND Purpose = @Purpose";

    var parameters = new
    {
      InvNum = reclineDto.Invoice,
      StockCode = reclineDto.StockCode,
      RecNum = reclineDto.BatchId,
      Purpose = reclineDto.Purpose
    };

    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected > 0;
  }
}
