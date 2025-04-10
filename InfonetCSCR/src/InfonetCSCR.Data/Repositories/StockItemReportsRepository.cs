namespace InfonetCSCR.Data.Repositories;

//CSCTrans Connection
internal class StockItemReportsRepository : BaseRepository, IStockItemReportsRepository
{
  public StockItemReportsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<SalesAnalysisByProduct>?> GetSalesAnalysisByProductAsync(string stockCode, string startDate, string endDate)
  {
    bool validStartDate = DateTime.TryParse(startDate, out DateTime dateStartDate);
    bool validEndDate = DateTime.TryParse(endDate, out DateTime dateEndDate);

    string query = @"SELECT p.SALE_NO AS SaleNo,
                              P.ShiftDate,
                              P.T_TYPE AS TransactionType,
                              P.CLIENT AS Client,
                              Q.QUANTITY AS Quantity,
                              Q.PRICE AS Price,
                              Q.COST AS Cost,
                              P.SALE_DATE AS SaleDate
	                      ,(Q.AMOUNT-Q.DISCOUNT-Q.DISC_ADJ)
			                      AS TotalAmount
	                      ,ABS(Q.QUANTITY*Q.COST) AS TotalCost
	                      ,(Q.AMOUNT-Q.DISCOUNT-Q.DISC_ADJ-ABS(Q.QUANTITY*Q.COST))
			                      AS GrossProfit
	                      ,CASE 
			                      WHEN Q.QUANTITY*Q.COST!=0
			                      THEN (Q.AMOUNT-Q.DISCOUNT-Q.DISC_ADJ-ABS(Q.QUANTITY*Q.COST))/ABS(Q.QUANTITY*Q.COST)*100
			                      ELSE 100
			                      END AS Margin
	                      ,DATENAME(MONTH, P.ShiftDate) + ' ' + DATENAME(YEAR, P.ShiftDate) AS MonthName
	                      FROM [CSCTrans].[dbo].[SALEHEAD] AS P INNER JOIN [CSCTrans].[dbo].[SALELINE] AS Q ON P.SALE_NO = Q.SALE_NO
	                      WHERE Q.STOCK_CODE = @StockCode
	                      AND P.ShiftDate BETWEEN @StartDate AND @EndDate
	                      ORDER BY P.ShiftDate";

    var parameters = new
    {
      StockCode = stockCode,
      StartDate = validStartDate ? dateStartDate : DateTime.Today,
      EndDate = validEndDate ? dateEndDate : DateTime.Today,
    };

    IEnumerable<SalesAnalysisByProduct> resultSalesAnalysisByProduct =
      await Connection.QueryAsync<SalesAnalysisByProduct>(query, parameters, Transaction);
    return resultSalesAnalysisByProduct;
  }

  public async Task<IEnumerable<StockItemHistory>> GetStockItemHistoryAsync(string stockCode, string startDate, string endDate)
  {
    bool validStartDate = DateTime.TryParse(startDate, out DateTime dateStartDate);
    bool validEndDate = DateTime.TryParse(endDate, out DateTime dateEndDate);

    var parameters = new
    {
      StockCode = stockCode,
      StartDate = validStartDate ? dateStartDate : DateTime.Today,
      EndDate = validEndDate ? dateEndDate : DateTime.Today
    };

    IEnumerable<StockItemHistory> resultStockItemsHistory = await Connection.QueryAsync<StockItemHistory>("Sproc_GetStockItemHistoryReportData", parameters, Transaction, commandType: CommandType.StoredProcedure);
    return resultStockItemsHistory;
  }

  public async Task<IEnumerable<VendorPurchasesByProduct>> GetVendorPurchasesByProductAsync(string stockCode, string startDate, string endDate)
  {
    bool validStartDate = DateTime.TryParse(startDate, out DateTime dateStartDate);
    bool validEndDate = DateTime.TryParse(endDate, out DateTime dateEndDate);

    string query = @"SELECT * 
        FROM PurchaseDetail INNER JOIN PurchaseHeader ON PurchaseDetail.PurchaseNo = PurchaseHeader.PurchaseNo
        WHERE PostedDate BETWEEN '" + (validStartDate ? dateStartDate : DateTime.Today) +
      "' AND '" + (validEndDate ? dateEndDate : DateTime.Today) + "' AND stockcode='" + stockCode + "'";

    IEnumerable<VendorPurchasesByProduct> resultVendorPurchasesByProduct =
      await Connection.QueryAsync<VendorPurchasesByProduct>(query, null, Transaction);
    return resultVendorPurchasesByProduct;
  }

  public async Task<IEnumerable<StockItemReports>> GetPurchasesDataAsync(string stockCode)
  {
    string sql = @"SELECT
                    Month(PostedDate) AS Month, 
                    Year(PostedDate) AS Year, 
                    SUM(ReceivedQty) AS Quantity, 
                    SUM(ExtCost) AS Amount 
                  FROM PurchaseHeader AS A INNER JOIN PurchaseDetail AS B ON A.PurchaseNo = B.PurchaseNo
                  WHERE B.Posted = 1 AND B.StockCode = '" + stockCode + @"' GROUP BY  Year(A.PostedDate) , Month(A.PostedDate)
                  ORDER BY Year(A.PostedDate),Month(A.PostedDate)";

    IEnumerable<StockItemReports> resultPurchasesData = await Connection.QueryAsync<StockItemReports>(sql, null, Transaction);
    return resultPurchasesData;
  }

  public async Task<IEnumerable<StockItemReports>> GetSalesDataAsync(string stockCode)
  {
    string sql = @"SELECT Month(A.ShiftDate) AS Month, 
                    Year(A.ShiftDate) As Year, 
                    ROUND(SUM(B.Quantity), 0) As Quantity,
                    SUM(B.Amount) AS Amount 
                  FROM SaleLine AS B INNER JOIN SaleHead AS A ON A.Sale_No=B.Sale_No
                  WHERE B.Stock_Code='" + stockCode + @"' GROUP BY Month(A.ShiftDate), Year(A.ShiftDate)
                  ORDER BY Year(A.ShiftDate),Month(A.ShiftDate)";

    IEnumerable<StockItemReports> resultSalesData = await Connection.QueryAsync<StockItemReports>(sql, null, Transaction);
    return resultSalesData;
  }

  public async Task<IEnumerable<SaleDetails>> GetSaleDetailsAsync(string stockCode, int year, int month)
  {
    string sql = $@"SELECT CONVERT(varchar,ShiftDate,101) Date, A.Sale_No SaleNo, Quantity, CAST(AMOUNT AS DECIMAL(15,2)) AS Amount 
                    FROM SaleLine AS B INNER JOIN SaleHead AS A ON A.Sale_No=B.Sale_No WHERE B.Stock_Code='{stockCode}' 
                    AND Year(A.ShiftDate)={year} AND Month(A.ShiftDate)={month}";

    IEnumerable<SaleDetails> resultSaleDetails = await Connection.QueryAsync<SaleDetails>(sql, null, Transaction);
    return resultSaleDetails;
  }

  public async Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsAsync(string stockCode, int year, int month)
  {
    string sql = $@"SELECT CONVERT(varchar,PostedDate,101) Date, A.PurchaseNo, ReceivedQty, CAST(ExtCost AS DECIMAL(15,2)) AS Amount
                    FROM PurchaseDetail AS B INNER JOIN PurchaseHeader AS A ON A.PurchaseNo=B.PurchaseNo
                    WHERE B.StockCode='{stockCode}' AND Year(A.PostedDate)={year} AND Month(A.PostedDate)={month}";

    IEnumerable<PurchaseDetail> resultPurchaseDetails = await Connection.QueryAsync<PurchaseDetail>(sql, null, Transaction);
    return resultPurchaseDetails;
  }
}
