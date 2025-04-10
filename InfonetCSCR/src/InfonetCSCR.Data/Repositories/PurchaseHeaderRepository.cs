namespace InfonetCSCR.Data.Repositories;

public class PurchaseHeaderRepository : BaseRepository, IPurchaseHeaderRepository
{
  public PurchaseHeaderRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<PurchaseHeader>> GetPurchaseHeadersByPOStatusAsync(string poStatus, int columnIndex)
  {
    string query = $@"SELECT 
                            PurchaseNo,
                            AccountNo, 
                            VendorName, 
                            CONVERT(varchar, PurchaseDate, 101) AS PurchaseDate,
                            Paid,
                            Lines,
                            ShiftID,
                            ShiftNum,
                            Emailed
                        FROM 
                            [CSCTrans].[dbo].[PurchaseHeader] 
                        WHERE 
                            (TaxexemptPO = 0 OR TaxexemptPO IS NULL) AND POStatus = @POStatus
                        ORDER BY  
                            {columnIndex} DESC";

    if (poStatus == "C" && (columnIndex == 2 || columnIndex == 3))
    {
      query += $@", PurchaseDate DESC, PurchaseNo DESC";
    }

    var parameters = new { POStatus = poStatus };
    IEnumerable<PurchaseHeader> purchaseHeaders = await Connection.QueryAsync<PurchaseHeader>(query, parameters, Transaction);
    return (purchaseHeaders);
  }

  public async Task<bool> AddPurchaseHeaderAsync(PurchaseHeader purchaseHeader)
  {
    const string sql = @"INSERT INTO [CSCTrans].[dbo].[PurchaseHeader] (
                          [PurchaseNo],
                          [AccountNo],
                          [VendorName],
                          [PurchaseDate],
                          [Paid],
                          [Lines],
                          [ShiftID],
                          [ShiftNum],
                          [SurChgDisc],
                          [SurChgDiscType],
                          [Emailed],
                          [DeliveryDate],
                          [SendDate],
                          [ReceivedDate],
                          [PostedDate],
                          [CreatedBy],
                          [SendBy],
                          [PostedBy],
                          [SubTotal],
                          [SurChargeTotal],
                          [TotalGST],
                          [PurchaseTotal],
                          [PayType],
                          [Terms],
                          [Buyer],
                          [ShipVia],
                          [FOB],
                          [Freight],
                          [Notes],
                          [POStatus],
                          [CustAccount],
                          [StoreID],
                          [HeaderFileName],
                          [Reposted],
                          [RepostedBy],
                          [RepostedDate],
                          [TaxexemptPO],
                          [Charges],
                          [InvoiceNo],
                          [TransStatus],
                          [CalcDiscount]
                        ) VALUES (
                          @PurchaseNo,
                          @AccountNo,
                          @VendorName,
                          @PurchaseDate,
                          @Paid,
                          @Lines,
                          @ShiftID,
                          @ShiftNum,
                          @SurChargeTotalDiscount,
                          @SurChargeTotalDiscountType,
                          @Emailed,
                          @DeliveryDate,
                          @SendDate,
                          @ReceivedDate,
                          @PostedDate,
                          @CreatedBy,
                          @SendBy,
                          @PostedBy,
                          @SubTotal,
                          @SurChargeTotal,
                          @TotalGST,
                          @PurchaseTotal,
                          @PayType,
                          @Terms,
                          @Buyer,
                          @ShipVia,
                          @FOB,
                          @Freight,
                          @Notes,
                          @POStatus,
                          @CustAccount,
                          @StoreID,
                          @HeaderFileName,
                          @Reposted,
                          @RepostedBy,
                          @RepostedDate,
                          @TaxexemptPO,
                          @Charges,
                          @InvoiceNo,
                          @TransStatus,
                          @CalculateDiscount
                        )";

    int rowsAffected = await Connection.ExecuteAsync(sql, purchaseHeader, Transaction);
    return rowsAffected > 0;
  }

  public async Task<long> GetNextPurchaseNumberAsync()
  {
    string query = $@"select max(Purchaseno) as NextPurchaseNo from [CSCTrans].[dbo].[PurchaseHeader]";

    long nextPurchaseNo = await Connection.QueryFirstOrDefaultAsync<long>(query, null, Transaction);
    nextPurchaseNo++;
    return nextPurchaseNo;
  }

  public async Task<bool> DeletePurchaseHeaderByPurchaseNoAsync(string purchaseNo)
  {
    const string sql = @"DELETE FROM [CSCTrans].[dbo].[PurchaseHeader] WHERE PurchaseNo = @PurchaseNo";

    var parameters = new { PurchaseNo = purchaseNo };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<PurchaseHeader?> GetPurchaseHeadersByPurchaseNoAsync(long purchaseNo)
  {
    string query = $@"SELECT
                          PurchaseNo,
                          AccountNo,
                          VendorName,
                          PurchaseDate,
                          Paid,
                          Lines,
                          ShiftID,
                          ShiftNum,
                          SurChgDisc AS SurChargeTotalDiscount,
                          SurChgDiscType AS SurChargeTotalDiscountType,
                          Emailed,
                          DeliveryDate,
                          SendDate,
                          ReceivedDate,
                          PostedDate,
                          CreatedBy,
                          SendBy,
                          PostedBy,
                          SubTotal,
                          SurchargeTotal,
                          TotalGST,
                          PurchaseTotal,
                          PayType,
                          Terms,
                          Buyer,
                          ShipVia,
                          FOB,
                          Freight,
                          Notes,
                          POStatus,
                          CustAccount,
                          StoreID,
                          HeaderFileName,
                          Reposted,
                          RepostedBy,
                          RepostedDate,
                          TaxexemptPO,
                          Charges,
                          InvoiceNo,
                          TransStatus,
                          CalcDiscount AS CalculateDiscount
                        FROM 
                          [CSCTrans].[dbo].[PurchaseHeader]
                        WHERE
	                        Purchaseno = @PurchaseNo";

    PurchaseHeader? purchaseHeaders = await Connection.QueryFirstOrDefaultAsync<PurchaseHeader>(query, new { PurchaseNo = purchaseNo }, Transaction);
    return (purchaseHeaders);
  }

  public async Task<bool> UpdatePurchaseHeaderEmailedStatusAsync(PurchaseHeader purchaseheader)
  {
    string query = $@"UPDATE [CSCTrans].[dbo].[PurchaseHeader]
                        SET
                            Emailed = COALESCE(@Emailed, Emailed),
                            POStatus = COALESCE(@POStatus, POStatus),
                            SendBy = COALESCE(@SendBy, SendBy),
                            SendDate = CASE WHEN @SendBy IS NOT NULL THEN GetDate() ELSE SendDate END,
                            Reposted = COALESCE(@Reposted, Reposted),
                            RepostedBy = COALESCE(@RepostedBy, RepostedBy),
                            RepostedDate = COALESCE(@RepostedDate, RepostedDate),
                            Postedby = COALESCE(@Postedby, Postedby),
                            PostedDate = COALESCE(@PostedDate, PostedDate),
                            Paid = COALESCE(@Paid, Paid),
                            ShiftNum = COALESCE(@ShiftNum, ShiftNum),
                            ShiftID = COALESCE(@ShiftID, ShiftID),
                            PayType = COALESCE(@PayType, PayType)
                        WHERE PurchaseNo = @PurchaseNo;";

    int rowsAffected = await Connection.ExecuteAsync(query, purchaseheader, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateCurrentStockByPurchaseNoAndLineNumAsync(long purchaseNo, int lineNum, decimal currentStock)
  {
    string query = $@"UPDATE
                            [CSCTrans].[dbo].[purchasedetail]
                        SET
                            CurrentStock = @CurrentStock
                        WHERE
                            purchaseno = @PurchaseNo
                            AND lineNum = @LineNum";

    var parameters = new { PurchaseNo = purchaseNo, LineNum = lineNum, CurrentStock = currentStock };

    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);

    return rowsAffected > 0;
  }

  public async Task<bool> UpdatePurchaseHeaderAsync(PurchaseHeader purchaseheader)
  {
    string query = $@"UPDATE [CSCTrans].[dbo].[PurchaseHeader]
                        SET
                          [AccountNo] = @AccountNo,
                          [VendorName] = @VendorName,
                          [PurchaseDate] = @PurchaseDate,
                          [Paid] = @Paid,
                          [Lines] = @Lines,
                          [ShiftID] = @ShiftID,
                          [ShiftNum] = @ShiftNum,
                          [SurChgDisc] = @SurChargeTotalDiscount,
                          [SurChgDiscType] = @SurChargeTotalDiscountType,
                          [Emailed] = @Emailed,
                          [DeliveryDate] = @DeliveryDate,
                          [SendDate] = @SendDate,
                          [ReceivedDate] = @ReceivedDate,
                          [PostedDate] = @PostedDate,
                          [CreatedBy] = @CreatedBy,
                          [SendBy] = @SendBy,
                          [PostedBy] = @PostedBy,
                          [SubTotal] = @SubTotal,
                          [SurChargeTotal] = @SurChargeTotal,
                          [TotalGST] = @TotalGST,
                          [PurchaseTotal] = @PurchaseTotal,
                          [PayType] = @PayType,
                          [Terms] = @Terms,
                          [Buyer] = @Buyer,
                          [ShipVia] = @ShipVia,
                          [FOB] = @FOB,
                          [Freight] = @Freight,
                          [Notes] = @Notes,
                          [POStatus] = @POStatus,
                          [CustAccount] = @CustAccount,
                          [StoreID] = @StoreID,
                          [HeaderFileName] = @HeaderFileName,
                          [Reposted] = @Reposted,
                          [RepostedBy] = @RepostedBy,
                          [RepostedDate] = @RepostedDate,
                          [TaxexemptPO] = @TaxexemptPO,
                          [Charges] = @Charges,
                          [InvoiceNo] = @InvoiceNo,
                          [TransStatus] = @TransStatus,
                          [CalcDiscount] = @CalculateDiscount
                        WHERE
                          [PurchaseNo] = @PurchaseNo;";

    int rowsAffected = await Connection.ExecuteAsync(query, purchaseheader, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<PurchasingAndReceivingReportDto>> GetPurchasingAndReceivingReportDataAsync(int purchaseNo, string vendorSortCondition)
  {
    string query = $@"SELECT
                            PurchaseHeader.PurchaseNo,
                            PurchaseHeader.CustAccount,
	                          PurchaseHeader.AccountNo,
	                          PurchaseHeader.SubTotal,
                            PurchaseHeader.SurchargeTotal,
                            PurchaseHeader.TotalGST,
                            PurchaseHeader.PurchaseTotal,
	                          PurchaseHeader.Terms,
                            PurchaseHeader.Buyer,
                            PurchaseHeader.ShipVia,
                            PurchaseHeader.FOB,
                            PurchaseHeader.Freight,
	                          PurchaseHeader.Notes,
	                          PurchaseHeader.PurchaseDate,
                            PurchaseHeader.DeliveryDate,
                            PurchaseHeader.ReceivedDate,
	                          PurchaseDetail.LineNum,
                            PurchaseDetail.PartNum,
                            PurchaseDetail.OrderedQty,
                            PurchaseDetail.UOM,
                            PurchaseDetail.UnitQty,
                            PurchaseDetail.Cost,
                            PurchaseDetail.Description,
                            PurchaseDetail.ExtCost,
                            PurchaseDetail.ApplyGST,
                            PurchaseDetail.GST,
                            PurchaseDetail.CalcDiscount,
                            Vendors.Address,
                            Vendors.City VendorsCity,
                            Vendors.Province VendorsProvince,
                            Vendors.Postal,
                            Vendors.Phone,
                            Vendors.Fax,
                            Vendors.Name,
                            Setup_C.Company_Name CompanyName,
                            Setup_C.Address_1 Address1,
                            Setup_C.Address_2 Address2,
                            Setup_C.City SetupCity,
                            Setup_C.Province SetupProvince,
                            Setup_C.Postal_code PostalCode,
                            Setup_C.Tel_Num TelNum,
                            Setup_C.Fax_Num FaxNum,
                            P_SET.P_SET PSet
                        FROM CSCTrans.dbo.PurchaseHeader PurchaseHeader
                        INNER JOIN CSCTrans.dbo.PurchaseDetail PurchaseDetail ON PurchaseHeader.PurchaseNo = PurchaseDetail.PurchaseNo
                        LEFT JOIN CSCMaster.dbo.Vendors Vendors ON PurchaseHeader.AccountNo = Vendors.Code
                        LEFT JOIN cscmaster.dbo.stockmst stockmst ON purchasedetail.stockcode = stockmst.stock_code
                        LEFT JOIN cscmaster.dbo.vendorparts vendorparts ON Vendorparts.Stock_code = Stockmst.Stock_code
                                                                       AND Vendorparts.vendorid = PurchaseHeader.AccountNo
                        LEFT JOIN CSCAdmin.dbo.Setup_C Setup_C ON PurchaseHeader.StoreID = Setup_C.Store
                        LEFT JOIN CSCADMIN.dbo.P_SET P_SET ON
                            (P_set.P_Value = PurchaseDetail.StockCode AND P_set.P_name = 'QUANT_DEC' AND P_set.P_LEVEL = 'STOCK')
                            OR
                            (P_set.P_Value = PurchaseDetail.Dept AND P_set.P_name = 'QUANT_DEC' AND P_set.P_LEVEL = 'DEPT')
                            OR
                            (P_set.P_Value = (PurchaseDetail.Dept + CHAR(255) + PurchaseDetail.SubDept) AND P_set.P_name = 'QUANT_DEC' AND P_set.P_LEVEL = 'SUBDEPT')
                            OR
                            (P_set.P_Value = (PurchaseDetail.Dept + CHAR(255) + PurchaseDetail.SubDept + CHAR(255) + PurchaseDetail.SubDetail) AND P_set.P_name = 'QUANT_DEC' AND P_set.P_LEVEL = 'SUBDEPT')
                        WHERE PurchaseHeader.PurchaseNo = @PurchaseNo
                        ORDER BY {vendorSortCondition}";

    IEnumerable<PurchasingAndReceivingReportDto> data = await Connection.QueryAsync<PurchasingAndReceivingReportDto>(query, new { PurchaseNo = purchaseNo }, Transaction);
    return data;
  }

  public async Task<bool> UpdatePurchaseHeaderSurChargeDiscountAsync(PurchaseHeader purchaseheader)
  {
    string query = $@"UPDATE [CSCTrans].[dbo].[PurchaseHeader]
                        SET
                          [SurChgDisc] = @SurChargeTotalDiscount,
                          [SurChgDiscType] = @SurChargeTotalDiscountType,
                          [SurChargeTotal] = @SurChargeTotal,
                          [PurchaseTotal] = @PurchaseTotal
                        WHERE
                          [PurchaseNo] = @PurchaseNo;";

    int rowsAffected = await Connection.ExecuteAsync(query, purchaseheader, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateOrInsertPurchaseCostChangeAsync(PurchaseCostChange purchaseCostChange)
  {
    string query = $@"
                MERGE INTO PurchaseCostChange AS target
                USING(
                    SELECT
                        @PurchaseNo AS PurchaseNo,
                        @StockCode AS StockCode,
                        @LineNum AS LineNum,
                        @VendorID AS VendorID,
                        @TransDate AS TransDate,
                        @PostedDate AS PostedDate,
                        @NewCost AS NewCost,
                        @PostedBy AS PostedBy
                ) AS source
                ON target.PurchaseNo = source.PurchaseNo
                AND target.StockCode = source.StockCode
                AND target.LineNum = source.LineNum
                AND target.VendorID = source.VendorID
                AND target.TransDate = source.TransDate
                AND target.PostedDate = source.PostedDate
                AND target.NewCost = source.NewCost
                AND target.PostedBy = source.PostedBy
                WHEN MATCHED THEN
                    UPDATE SET
                        target.PurchaseNo = @PurchaseNo,
                        target.StockCode = @StockCode,
                        target.LineNum = @LineNum,
                        target.VendorID = @VendorID,
                        target.TransDate = @TransDate,
                        target.PostedDate = @PostedDate,
                        target.NewCost = @NewCost,
                        target.PostedBy = @PostedBy
                WHEN NOT MATCHED THEN
                    INSERT(PurchaseNo, StockCode, LineNum, VendorID, TransDate, PostedDate, NewCost, PostedBy)
                    VALUES(source.PurchaseNo, source.StockCode, source.LineNum, source.VendorID, source.TransDate, source.PostedDate, source.NewCost, source.PostedBy); ";


    var parameters = new
    {
      purchaseCostChange.PurchaseNo,
      purchaseCostChange.StockCode,
      purchaseCostChange.LineNum,
      purchaseCostChange.VendorID,
      purchaseCostChange.TransDate,
      purchaseCostChange.PostedDate,
      purchaseCostChange.NewCost,
      purchaseCostChange.PostedBy,
    };

    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateHeaderFileNameAsync(string headerFile, long purchaseNo)
  {
    string query = $@"UPDATE [CSCTrans].[dbo].[PurchaseHeader]
                        SET
                            HeaderFileName = @HeaderFile
                        WHERE
                            Purchaseno = @PurchaseNo";

    var parameters = new { HeaderFile = headerFile, PurchaseNo = purchaseNo };

    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<PurchaseHeader>> GetPurchaseHeaderByHeaderFileAsync(string headerFileName)
  {
    string query = $@"SELECT 
                            PurchaseNo,
                            AccountNo,
                            InvoiceNo,
							              ReceivedDate,
							              VendorName,
							              SurchargeTotal,
							              TotalGST,
							              PurchaseTotal,
							              PayType,
							              InvoiceNo
                        FROM 
                            [CSCTrans].[dbo].[PurchaseHeader] 
                        WHERE 
                            HeaderFileName = @HeaderFileName
                        ORDER BY
                            posteddate, purchaseno";

    var parameters = new { HeaderFileName = headerFileName };
    IEnumerable<PurchaseHeader> purchaseHeaders = await Connection.QueryAsync<PurchaseHeader>(query, parameters, Transaction);
    return (purchaseHeaders);
  }
}
