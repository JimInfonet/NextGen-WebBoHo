namespace InfonetCSCR.Data.Repositories;

public class PurchaseDetailRepository : BaseRepository, IPurchaseDetailRepository
{
  public PurchaseDetailRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> DeletePurchaseDetailByPurchaseNoAsync(string purchaseNo)
  {
    const string sql = @"DELETE FROM [CSCTrans].[dbo].[PurchaseDetail] WHERE PurchaseNo =  @PurchaseNo";

    var parameters = new { PurchaseNo = purchaseNo };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return true;
  }

  public async Task<double> CalculatPODiscount(string purchaseNo)
  {
    const string sql = $@"SELECT SUM(OrderedQty*CalcDiscount) as ExtDiscount FROM [CSCTrans].[dbo].[PurchaseDetail] 
                            Where Purchaseno = @PurchaseNo";

    var parameters = new { PurchaseNo = purchaseNo };
    double? poDiscount = await Connection.QueryFirstOrDefaultAsync<double?>(sql, parameters, Transaction);
    if (poDiscount == null)
    {
      poDiscount = 0;
    }

    poDiscount = (double?)Math.Round((decimal)poDiscount, 2);
    return (double)poDiscount;
  }

  public async Task<POSubTotalDataDto> GetPOSubTotalDataAsync(string purchaseNo)
  {
    const string sql = $@"SELECT 
	                            SUM(ExtCost) as SubTotal, 
	                            SUM(OrderedQty*CalcDiscount) as ExtDiscount, 
	                            SUM(Charges) AS TotalCharges, 
	                            SUM(GST) As GSTTotal 
                            FROM [CSCTrans].[dbo].[PurchaseDetail] 
                            Where Purchaseno = @PurchaseNo";

    var parameters = new { PurchaseNo = purchaseNo };
    POSubTotalDataDto pOSubTotalData = await Connection.QueryFirstOrDefaultAsync<POSubTotalDataDto>(sql, parameters, Transaction);
    return pOSubTotalData;
  }

  public async Task<IEnumerable<POSubTotalDataDto>> GetCalculatedDiscountsAsync(List<long> purchaseNoList)
  {
    if (purchaseNoList.Count == 0) return new List<POSubTotalDataDto>();
    string encodedPurchaseNo = "'" + string.Join("', '", purchaseNoList) + "'";
    string sql = $@"SELECT 
	                      PurchaseNo,
	                      SUM(OrderedQty*CalcDiscount) as ExtDiscount
                      FROM [CSCTrans].[dbo].[PurchaseDetail] 
                      Where PurchaseNo IN ({encodedPurchaseNo})
                      Group By PurchaseNo";

    IEnumerable<POSubTotalDataDto> pOSubTotalData = await Connection.QueryAsync<POSubTotalDataDto>(sql, null, Transaction);
    return pOSubTotalData;
  }

  public async Task<double> CalculatePOGSTTotal(string purchaseNo)
  {
    const string sql = $@"SELECT SUM(GST) as GSTTotal FROM [CSCTrans].[dbo].[PurchaseDetail] Where Purchaseno = @PurchaseNo";

    var parameters = new { PurchaseNo = purchaseNo };
    double? poGSTTotal = await Connection.QueryFirstOrDefaultAsync<double?>(sql, parameters, Transaction);
    if (poGSTTotal == null)
    {
      poGSTTotal = 0;
    }

    return (double)poGSTTotal;
  }

  public async Task<int> GetLinesCountAsync(string purchaseNo)
  {
    const string sql = $@"SELECT count(linenum) as LineCount FROM [CSCTrans].[dbo].[PurchaseDetail] Where Purchaseno = @PurchaseNo";

    var parameters = new { PurchaseNo = purchaseNo };
    int lineCount = await Connection.QueryFirstOrDefaultAsync<int>(sql, parameters, Transaction);
    return lineCount;
  }

  public async Task<int> GetMaxLineNumAsync(string purchaseNo)
  {
    const string sql = $@"SELECT MAX(linenum) AS MaxLineNumber FROM [CSCTrans].[dbo].[PurchaseDetail] Where Purchaseno = @purchaseNo";
    int maxLineNumber = await Connection.ExecuteScalarAsync<int>(sql, new { purchaseNo }, Transaction);
    return maxLineNumber + 1;
  }

  public async Task<bool> UpdatePurchaseHeaderLineCount(string purchaseNo)
  {
    const string sql = $@"UPDATE [CSCTrans].[dbo].[PurchaseHeader]
                            SET [Lines] = (SELECT COUNT(LineNum) AS LineCount 
                                          FROM [CSCTrans].[dbo].[PurchaseDetail] 
                                          WHERE PurchaseNo = @purchaseNo)
                            WHERE [PurchaseNo] = @purchaseNo ";

    int affectedRows = await Connection.ExecuteAsync(sql, new { purchaseNo }, Transaction);
    return affectedRows != 0;
  }

  public async Task<bool> AddPurchaseDetailBatchAsync(IList<PurchaseDetail> purchaseDetails)
  {
    string sql = $@"INSERT INTO [CSCTrans].[dbo].[PurchaseDetail]
                    (
                         [PurchaseNo]
                        ,[LineNum]
                        ,[PartNum]
                        ,[Description]
                        ,[UOM]
                        ,[UnitQty]
                        ,[OrderedQty]
                        ,[ReceivedQty]
                        ,[Cost]
                        ,[ExtCost]
                        ,[ApplyGST]
                        ,[GST]
                        ,[StockCode]
                        ,[Dept]
                        ,[SubDept]
                        ,[Posted]
                        ,[CostChanged]
                        ,[CurrentStock]
                        ,[PackqtyChanged]
                        ,[ProductVendor]
                        ,[SubDetail]
                        ,[Charges]
                        ,[CalcDiscount]
                        ,[PoCost]
                    )
                    VALUES
                    (
                         @PurchaseNo
                        ,@LineNumber
                        ,@PartNumber
                        ,@Description
                        ,@UnitOfMeasure
                        ,@UnitQuantity
                        ,@OrderedQuantity
                        ,@ReceivedQuantity
                        ,@Cost
                        ,@ExtCost
                        ,@ApplyGST
                        ,@GST
                        ,@StockCode
                        ,@Department
                        ,@SubDepartment
                        ,@Posted
                        ,@CostChanged
                        ,@CurrentStock
                        ,@PackQuantityChanged
                        ,@ProductVendor
                        ,@SubDetail
                        ,@Charges
                        ,@CalculateDiscount
                        ,@PoCost
                    )";

    int rowsAffected = await Connection.ExecuteAsync(sql, purchaseDetails, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNoAsync(long purchaseNo)
  {
    string query = $@"SELECT
                            {GetPurchaseDetailsColumnNames()}
                        FROM
                            [CSCTrans].[dbo].[PurchaseDetail]
                        WHERE
                            Purchaseno = @PurchaseNo
                        ORDER BY
                            LINENUM";

    IEnumerable<PurchaseDetail> purchaseDetails = await Connection.QueryAsync<PurchaseDetail>(query, new { PurchaseNo = purchaseNo }, Transaction);
    return (purchaseDetails);
  }

  public async Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNoAndOrderByVendorsAsync(long purchaseNo, string vendorSort)
  {
    string query = $@"SELECT
                            LineNum AS LineNumber,
                            Orderedqty OrderedQuantity,
                            UnitQty AS UnitQuantity,
                            UOM AS UnitOfMeasure,
                            Partnum,
                            LTRIM(RTRIM(StockCode)) AS StockCode
                        FROM
                            [CSCTrans].[dbo].[PurchaseDetail]
                        LEFT JOIN
                            cscmaster.dbo.stockmst stockmst ON purchasedetail.stockcode = stockmst.stock_code
                        LEFT JOIN
                            cscmaster.dbo.vendorparts vendorparts ON Vendorparts.Stock_code = Stockmst.Stock_code
                        WHERE
                            PurchaseNo = @PurchaseNo
                        ORDER BY {vendorSort}";

    IEnumerable<PurchaseDetail> purchaseDetails = await Connection.QueryAsync<PurchaseDetail>(query, new { PurchaseNo = purchaseNo }, Transaction);
    return (purchaseDetails);
  }

  public async Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNumberAndVendorSortAsync(long purchaseNo, string? vendorSort)
  {
    string query = $@"SELECT
                            [PurchaseNo] AS PurchaseNo,
                            [LineNum] AS LineNumber,
                            [PartNum] AS PartNumber,
                            [Description],
                            [UOM] AS UnitOfMeasure,
                            [UnitQty] AS UnitQuantity,
                            [OrderedQty] AS OrderedQuantity,
                            [ReceivedQty] AS ReceivedQuantity,
                            [Cost],
                            [ExtCost],
                            [ApplyGST],
                            [GST],
                            LTRIM(RTRIM([StockCode])) AS StockCode,
                            [Posted],
                            [CostChanged],
                            [CurrentStock],
                            [PackqtyChanged] AS PackQuantityChanged,
                            [ProductVendor],
                            [Charges],
                            [CalcDiscount] AS CalculateDiscount,
                            [PoCost]
                        FROM [CSCTrans].[dbo].[PurchaseDetail]";

    if (vendorSort != null && vendorSort != "")
    {
      query += $@"    LEFT JOIN
                            cscmaster.dbo.stockmst stockmst ON purchasedetail.stockcode = stockmst.stock_code
                        LEFT JOIN
                            cscmaster.dbo.vendorparts vendorparts ON Vendorparts.Stock_code = Stockmst.Stock_code  AND VendorParts.VendorID = Stockmst.Vendor
                        WHERE
                            PurchaseNo = @PurchaseNo
                        ORDER BY {vendorSort}";
    }
    else
    {
      query += $@"    WHERE
                            Purchaseno = @PurchaseNo
                        ORDER BY
                            LINENUM";
    }

    IEnumerable<PurchaseDetail> purchaseDetails = await Connection.QueryAsync<PurchaseDetail>(query, new { PurchaseNo = purchaseNo }, Transaction);
    return (purchaseDetails);
  }

  public async Task<bool> UpdatePurchaseDetailAsync(PurchaseDetail purchaseDetail)
  {
    string sql = $@"UPDATE [CSCTrans].[dbo].[PurchaseDetail]
                      SET
                          [Description] = @Description,
                          [UOM] = @UnitOfMeasure,
                          [UnitQty] = @UnitQuantity,
                          [OrderedQty] = @OrderedQuantity,
                          [ReceivedQty] = @ReceivedQuantity,
                          [Cost] = @Cost,
                          [ExtCost] = @ExtCost,
                          [ApplyGST] = @ApplyGST,
                          [GST] = @GST,
                          [StockCode] = @StockCode,
                          [Dept] = @Department,
                          [SubDept] = @SubDepartment,
                          [Posted] = @Posted,
                          [CostChanged] = @CostChanged,
                          [CurrentStock] = @CurrentStock,
                          [PackqtyChanged] = @PackQuantityChanged,
                          [ProductVendor] = @ProductVendor,
                          [SubDetail] = @SubDetail,
                          [Charges] = @Charges,
                          [CalcDiscount] = @CalculateDiscount,
                          [PoCost] = @PoCost
                      WHERE
                          [PurchaseNo] = @PurchaseNo
                          AND [LineNum] = @LineNumber
                          AND [PartNum] = @PartNumber;
                      ";

    int affectedRows = await Connection.ExecuteAsync(sql, purchaseDetail, Transaction);
    return affectedRows > 0;
  }

  public async Task<PurchaseDetail> GetPurchaseDetailByPurchaseNumberAsync(long purchaseNo, int lineNumber, string partNumber)
  {
    string query = $@"SELECT 
                            [PurchaseNo] AS PurchaseNo,
                            [LineNum] AS LineNumber,
                            [PartNum] AS PartNumber,
                            [Description],
                            [UOM] AS UnitOfMeasure,
                            [UnitQty] AS UnitQuantity,
                            [OrderedQty] AS OrderedQuantity,
                            [ReceivedQty] AS ReceivedQuantity,
                            [Cost],
                            [ExtCost],
                            [ApplyGST],
                            [GST],
                            [StockCode],
                            [Dept] AS Department,
                            [SubDept] AS SubDepartment,
                            [Posted],
                            [CostChanged],
                            [CurrentStock],
                            [PackqtyChanged] AS PackQuantityChanged,
                            [ProductVendor],
                            [SubDetail],
                            [Charges],
                            [CalcDiscount] AS CalculateDiscount,
                            [PoCost]
                        FROM [CSCTrans].[dbo].[PurchaseDetail]
                        WHERE
                            [PurchaseNo] = @PurchaseNo
                            AND [LineNum] = @LineNumber
                            AND [PartNum] = @PartNumber";

    var parameter = new { PurchaseNo = purchaseNo, LineNumber = lineNumber, PartNumber = partNumber };
    PurchaseDetail purchaseDetail = await Connection.QueryFirstOrDefaultAsync<PurchaseDetail>(query, parameter, Transaction);
    return purchaseDetail;
  }

  public async Task<bool> DeletePurchaseDetailAsync(
    string purchaseNo,
    string partNumber,
    string lineNumber)
  {
    const string sql = @"DELETE FROM [CSCTrans].[dbo].[PurchaseDetail] 
                            WHERE LineNum = @LineNumber And Purchaseno = @PurchaseNo";

    var parameters = new { PurchaseNo = purchaseNo, LineNumber = lineNumber };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return (rowsAffected > 0);
  }

  public async Task<bool> UpdatePurchaseDetailGSTByPurchaseNOAsync(IEnumerable<PurchaseDetail> purchaseDetails)
  {
    string sql = $@"UPDATE [CSCTrans].[dbo].[PurchaseDetail]
                      SET
                          [ApplyGST] = @ApplyGST,
                          [GST] = @GST
                      WHERE
                          [PurchaseNo] = @PurchaseNo
                          AND [LineNum] = @LineNumber;";

    int affectedRows = await Connection.ExecuteAsync(sql, purchaseDetails, Transaction);
    return affectedRows > 0;
  }

  public async Task<PurchaseDetail> GetPurchaseDetailsByPurchaseNoAndStockCodeAsync(long? purchaseNo, string stockCode)
  {
    string query = $@"SELECT
                            ReceivedQty AS ReceivedQuantity,
                            Cost
                        FROM
                            [CSCTrans].[dbo].[PurchaseDetail]
                        WHERE
                            purchaseno = @PurchaseNo
                        AND StockCode = @StockCode";

    PurchaseDetail? purchaseDetail = await Connection.QueryFirstOrDefaultAsync<PurchaseDetail?>(query, new { PurchaseNo = purchaseNo, StockCode = stockCode }, Transaction);
    return purchaseDetail;
  }

  public async Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsWithStockAsync(long purchaseNo)
  {
    string query = $@"SELECT
                            pd.PurchaseNo,
                            pd.Cost,
                            pd.ReceivedQty,
                            pd.LineNum,
                            pd.OrderedQty,
                            pd.UnitQty,
                            pd.UOM,
                            pd.PartNum,
                            pd.StockCode,
                            pd.Posted,
                            pd.PackqtyChanged,
                            pd.CostChanged,
                            pd.ExtCost,
                            pd.Description,
                            pd.GST,
                            pd.Dept AS Department,
                            pd.SubDept AS SubDepartment,
                            pd.SubDetail,
                            sm.stock_type
                         FROM
                            [CSCTrans].[dbo].[purchasedetail] pd
                            INNER JOIN [cscmaster].[dbo].[stockmst] sm ON pd.stockcode = sm.stock_code
                         WHERE
                            pd.purchaseno = @PurchaseNo
                         ORDER BY
                            sm.stock_type DESC, pd.StockCode";

    var parameters = new { PurchaseNo = purchaseNo };

    IEnumerable<PurchaseDetail> result = await Connection.QueryAsync<PurchaseDetail>(query, parameters, Transaction);

    return result;
  }

  public async Task<bool> UpdatePurchaseDetailAsync(long purchaseNo)
  {
    string query = $@"UPDATE
                          [CSCTrans].[dbo].[purchasedetail]
                        SET
                          Posted = 1,
                          costchanged = 0
                        WHERE
                          PurchaseNo = @PurchaseNo";

    var parameters = new { PurchaseNo = purchaseNo };

    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);

    return rowsAffected > 0;
  }

  public async Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNoListOrderByLinenumAsync(List<long> purchaseNoList)
  {
    if (purchaseNoList.Count == 0) return new List<PurchaseDetail>();
    string encodedPurchaseNo = "'" + string.Join("', '", purchaseNoList) + "'";
    string query = $@"SELECT
                            StockMST.Sub_Detail SubDetail,
                            StockMST.DESCRIPT StockName,
			                      VendorParts.PartNo VendorPart,
                            {GetPurchaseDetailsColumnNames()}
                        FROM
                            PurchaseDetail
                        LEFT JOIN
                            cscmaster.dbo.StockMST ON purchasedetail.stockcode = StockMST.stock_code
                        LEFT JOIN
                            cscmaster.dbo.VendorParts ON VendorParts.Stock_code = StockMST.Stock_code
                        WHERE
                            Purchaseno IN ({encodedPurchaseNo})";

    IEnumerable<PurchaseDetail> purchaseDetails = await Connection.QueryAsync<PurchaseDetail>(query, null, Transaction);
    return purchaseDetails;
  }

  private string GetPurchaseDetailsColumnNames()
  {
    string columns = @" PurchaseNo,
                          LineNum AS LineNumber,
                          PartNum AS PartNumber,
                          Description,
                          UOM  AS UnitOfMeasure,
                          UnitQty  AS UnitQuantity,
                          OrderedQty AS OrderedQuantity,
                          ReceivedQty AS ReceivedQuantity,
                          Cost,
                          ExtCost,
                          ApplyGST,
                          GST,
                          LTRIM(RTRIM(StockCode)) AS StockCode,
                          purchasedetail.Dept AS Department,
                          SubDept AS SubDepartment,
                          Posted,
                          CostChanged,
                          CurrentStock,
                          PackqtyChanged AS PackQuantityChanged,
                          ProductVendor,
                          SubDetail,
                          Charges,
                          CalcDiscount,
                          PoCost";
    return columns;
  }
}
