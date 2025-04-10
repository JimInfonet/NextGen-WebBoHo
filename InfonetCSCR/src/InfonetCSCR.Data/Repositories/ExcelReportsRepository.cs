namespace InfonetCSCR.Data.Repositories;

public class ExcelReportsRepository : BaseRepository, IExcelReportsRepository
{
  public ExcelReportsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<SummaryFuelProductsReportDto>?> GetSummaryFuelProductsReportDataAsync(ExcelReportGenerator excelReportGenerator)
  {
    string sql = @$"SELECT 
                      MONTH(B.Sale_Date) AS Month, 
                      YEAR(B.Sale_Date) AS Year, 
                      A.Stock_Code AS StockCode, 
                      A.Descript AS Description, 
                      SUM(A.QUANTITY) AS Quantity, 
                      SUM(A.AMOUNT - A.DISCOUNT) AS Amount, 
                      SUM(A.DISCOUNT) AS Rebate, 
                      SUM(D.Tax_Included_Amount) AS SalesLineIncludedTax,
                      SUM(E.Tax_Included_Amount) AS TaxCreditLineIncludedTax, 
                      GroupID, 
                      C.TaxExempt, 
                      D.Tax_Name AS SalesLineTaxName, 
                      E.Tax_Name AS TaxCreditLineTaxName 
                  FROM 
                      SaleLine AS A 
                      INNER JOIN SaleHead AS B ON A.Sale_No = B.Sale_No 
                      LEFT OUTER JOIN CSCMaster.dbo.Client AS C ON B.Client = C.CL_Code 
                      LEFT OUTER JOIN S_LineTax AS D ON D.Sale_No = A.Sale_No AND D.Line_No = A.Line_Num 
                      LEFT OUTER JOIN TaxCreditLine AS E ON E.Sale_No = A.Sale_No AND E.Line_No = A.Line_Num 
                  WHERE 
                      B.Sale_Date >= @StartDate 
                      AND B.Sale_Date <= @EndDate 
                      AND A.Stock_Code IN (
                          SELECT Stock_Code 
                          FROM [CSCPump].[dbo].[Grade] 
                          WHERE FuelType = 'G' OR FuelType = 'D'        -- Hard Coded in VB
                      ) 
                  GROUP BY  
                      MONTH(B.Sale_Date), 
                      YEAR(B.Sale_Date), 
                      A.Stock_Code, 
                      A.Descript, 
                      GroupID, 
                      C.TaxExempt, 
                      D.Tax_Name, 
                      E.Tax_Name 
                  ORDER BY  
                      MONTH(B.Sale_Date), 
                      YEAR(B.Sale_Date), 
                      A.Stock_Code, 
                      A.Descript, 
                      GroupID, 
                      C.TaxExempt, 
                      D.Tax_Name, 
                      E.Tax_Name";

    IList<SummaryFuelProductsReportDto>? saleData = (IList<SummaryFuelProductsReportDto>?)await Connection.QueryAsync<SummaryFuelProductsReportDto>(sql, excelReportGenerator, Transaction);
    return saleData;
  }
  
  public async Task<IList<FleetCardReportDto>?> GetNorth60OrYTGFleetCardReportDataAsync(ExcelReportGenerator excelReportGenerator, bool isYTG)
  {
    string tender_Name = (isYTG ? "Yukon Territorial Go" : "North of 60 Fleet"); // hard coded in VB
    string sql = @$"SELECT 
                        RIGHT(C.Card_Number, LEN(C.Card_Number) - 6) AS CardNumber,
                        A.Sale_No AS SaleNo,
                        A.Sale_Date AS SaleDate,
                        B.Quantity,
                        B.Price,
                        B.Descript,
                        B.Amount AS SaleLineAmount,
                        B.Discount,
                        B.Amount - B.Discount AS NetAmount,
                        'Other items' AS Misc,                    -- hard coded in VB
                        A.Sale_Amt AS SaleHeadAmount,
                        B.Amount - B.Discount AS FinalAmount
                    FROM 
                        [CSCTrans].[dbo].[CardTenders] AS C
                    INNER JOIN 
                        [CSCTrans].[dbo].[SaleLine] AS B 
                    ON 
                        C.Sale_No = B.Sale_No 
                        AND C.TILL_NUM = B.TILL_NUM
                    INNER JOIN 
                        [CSCTrans].[dbo].[SaleHead] AS A 
                    ON 
                        A.Sale_No = B.Sale_No 
                        AND A.Till = B.Till_Num
                    WHERE 
                        A.SALE_DATE >= @StartDate 
                        AND A.SALE_DATE <= @EndDate
                        AND C.Tender_Name = '{tender_Name}'     
                        AND B.DEPT = @Department
                    ORDER BY 
                        A.Sale_Date, 
                        A.Sale_No";

    IList<FleetCardReportDto>? data = (IList<FleetCardReportDto>?)await Connection.QueryAsync<FleetCardReportDto>(sql, excelReportGenerator, Transaction);
    return data;
  }
  
  public async Task<IList<SaleLine>?> GetAmountFromSaleLineAsync(string? fuelDept)
  {
    string sql = @$"SELECT 
                        Amount, 
                        Sale_No AS SaleNo
                    FROM 
                        [CSCTrans].[dbo].[SaleLine] 
                    WHERE 
                         Dept = {fuelDept}";

    IList<SaleLine>? amounts = (IList<SaleLine>?)await Connection.QueryAsync<SaleLine>(sql, null, Transaction);
    return amounts;
  }

  public async Task<bool> CreateFNGTRSalesReportDataAsync(ExcelReportGenerator excelReportGenerator)
  {
    var rowAffected = await Connection.ExecuteAsync
    (sql: "Sproc_FNGTR_Sales",
    param: new
    {
      StartDate = excelReportGenerator.StartDate,
      Enddate = excelReportGenerator.EndDate
    },
    Transaction, commandType: CommandType.StoredProcedure);
    return (rowAffected > 0);
  }

  public async Task<IList<ReportData>?> GetAllReportDataAsync()
  {
    string sql = @$"SELECT 
                         Sale_Date AS SaleDate, 
                         Sale_No AS SaleNo, 
                         TreatyNo, 
                         Product, 
                         Description,
                         Price, 
                         Tax_Exempt_Price AS TaxExemptedPrice, 
                         Quantity, 
                         TotalExemptedTax, 
                         Sale_Total AS SaleTotal,
                         Phone_No AS PhoneNo, 
                         Reason
                    FROM 
                      [CSCTrans].[dbo].[ReportData]";

    IList<ReportData>? reportData = (IList<ReportData>?)await Connection.QueryAsync<ReportData>(sql, null, Transaction);
    return reportData;
  }
}
