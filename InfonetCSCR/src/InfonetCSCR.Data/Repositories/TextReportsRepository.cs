namespace InfonetCSCR.Data.Repositories;

public class TextReportsRepository : BaseRepository, ITextReportsRepository
{
  private readonly IApplicationConfig _config;

  public TextReportsRepository(IDbTransaction transaction, IApplicationConfig config) : base(transaction)
  {
    _config = config;
  }

  public async Task<IEnumerable<TextReport>> GetAllTextReportsListAsync()
  {
    const string query = @"
      SELECT
        [ReportCode],
        [ReportTitle]
      FROM [CSCAdmin].[dbo].[TextReports]";

    return await Connection.QueryAsync<TextReport>(query, null, Transaction);
  }

  public async Task<IEnumerable<CardDetails>> GetCardDetailsReportDataAsync(TextReportParams textReportParams)
  {
    var query = new StringBuilder();

    query.AppendLine(@"
      SELECT
        C.TransactionDate,
        C.Sale_No AS SaleNo,
        C.Amount,
        C.TerminalID,
        S.T_Type AS TransactionType,
        C.Card_Type AS CardType, 
        C.Card_Name AS CardName, 
        C.Card_Number AS CardNumber, 
        C.Expiry_Date AS ExpiryDate, 
        C.Approval_Code AS ApprovalCode, 
        C.TransactionTime 
      FROM [CSCTrans].[dbo].[CARDTENDERS] AS C
      INNER JOIN [CSCTrans].[dbo].[SALEHEAD] AS S
      ON	C.Sale_No = S.Sale_No 
      AND	C.TILL_NUM = S.TILL");

    if (!string.IsNullOrEmpty(textReportParams.StoreID))
    {
      query.AppendLine("AND C.Store = S.Store_Id");
    }

    query.AppendLine("WHERE S.ShiftDate >= @StartDate AND S.ShiftDate <= @EndDate");

    if (!string.IsNullOrEmpty(textReportParams.TerminalID))
    {
      query.AppendLine("AND C.TerminalID = @TerminalID");
    }

    if (!string.IsNullOrEmpty(textReportParams.StoreID))
    {
      query.AppendLine("AND S.Store_Id = @StoreID");
    }

    query.AppendLine("ORDER BY C.TransactionDate");

    return await Connection.QueryAsync<CardDetails>(query.ToString(), textReportParams, Transaction);
  }

  public async Task<IEnumerable<FleetCardFuel>> GetFleetCardFuelReportDataAsync(TextReportParams textReportParams, string fuelDept)
  {
    var startDate = textReportParams.StartDate?.ToString("yyyyMMdd") ?? string.Empty;
    var endDate = textReportParams.EndDate?.ToString("yyyyMMdd") ?? string.Empty;

    var sql = new StringBuilder();
    sql.AppendLine($@"
        SELECT
          CardTenders.TransactionDate,
          CardTenders.Sale_No AS SaleNo,
          SaleLine.AMOUNT AS Amount,
          CardTenders.Card_Type AS CardType,
          SaleLine.SALE_TYPE AS SaleType,
          CardTenders.Card_Name AS CardName,
          CardTenders.Card_Number AS CardNumber,
          SaleLine.STOCK_CODE AS StockCode,
          SaleLine.QUANTITY AS Quantity,
          SaleLine.DISCOUNT AS Discount,
          CardTenders.TransactionTime
        FROM CSCTrans.dbo.CardTenders
          INNER JOIN CSCTrans.dbo.SaleLine
          ON CardTenders.Sale_No = SaleLine.Sale_No
          AND CardTenders.TILL_NUM = SaleLine.TILL_NUM
        WHERE CardTenders.TransactionDate >= CAST('{startDate}' AS DATE)
          AND CardTenders.TransactionDate <= CAST('{endDate}' AS DATE)
          AND (CardTenders.Card_Type = 'F' OR CardTenders.Card_Type = 'A')
          AND SaleLine.DEPT = '{fuelDept}'");

    if (!string.IsNullOrEmpty(textReportParams.StoreID))
    {
      sql.AppendLine("AND CardTenders.Store = SaleLine.Store_Id");
      sql.AppendLine($"AND SaleLine.Store_Id = '{textReportParams.StoreID}'");
    }

    if (!string.IsNullOrEmpty(textReportParams.StartCard))
    {
      sql.AppendLine($"AND Card_Number >= '{textReportParams.StartCard}'");
    }
    if (!string.IsNullOrEmpty(textReportParams.EndCard))
    {
      sql.AppendLine($"AND Card_Number <= '{textReportParams.EndCard}'");
    }

    sql.AppendLine("ORDER BY CardTenders.Card_Name, CardTenders.Sale_No");

    var sqlQuery = sql.ToString();

    return await Connection.QueryAsync<FleetCardFuel>(sqlQuery.ToString(), null, Transaction);
  }

  public async Task<DiscountDto?> GetCardDiscountFromCardNameAsync(string cardname)
  {
    string query = $@"SELECT
                    tc.DiscountRate AS discountRate, tc.DiscountType AS discountType
                    FROM [CSCMaster].[dbo].Cards c
                    INNER JOIN [CSCMaster].[dbo].TendCard tc ON c.CardCode = tc.CardCode
                    WHERE c.Description = '{cardname}'";

    DiscountDto? discountProperties = await Connection.QueryFirstOrDefaultAsync<DiscountDto>(query, null, Transaction);
    return discountProperties;
  }

  public async Task<IEnumerable<TotalizerReading>?> GetTotalizerReadingReportDataAsync(TextReportParams textReportParams)
  {
    string query = $@"SELECT
                          th.date_time AS Date,
                          g.FullName AS FullName,
                          th.PumpID AS PumpID,
                          th.volume AS Volume,
                          th.Grade AS Grade,
                          th.dollars AS Dollars
                      FROM CSCPump.[dbo].TotalizerHist th
                      LEFT JOIN (SELECT DISTINCT g.ID, g.FullName
                      FROM CSCPump.[dbo].Grade g
                      {(_config.IsBackOffice ? "" : $"where g.Store_Id = '{textReportParams.StoreID}'")})
                      g ON th.Grade = g.ID
                      WHERE th.GroupNumber = (
                                SELECT MAX(GroupNumber)
                                FROM CSCPump.[dbo].TotalizerHist
                                WHERE date_time >= '{textReportParams.ReadingDate?.ToString("yyyyMMdd")}' 
                                AND date_time <= '{textReportParams.ReadingDate?.AddDays(1).ToString("yyyyMMdd")}'
                               {(_config.IsBackOffice ? "" : $"AND Store_Id = '{textReportParams.StoreID}'")}
                                )
                      {(_config.IsBackOffice ? "" : $"AND th.Store_Id = '{textReportParams.StoreID}'")}
                      ORDER BY th.PumpID, th.PositionId;";

    IEnumerable<TotalizerReading>? totalizerReadings = await Connection.QueryAsync<TotalizerReading>(query, null, Transaction);
    return totalizerReadings;
  }

  public async Task<IEnumerable<CreditCardSummary>?> GetCreditCardSummaryReportDataAsync(TextReportParams textReportParams)
  {
    string query = $@"SELECT 
                        Cards.Description, 
                        Cards.CardType, 
                        COUNT(CardTenders.Card_Name) AS [Transactions], 
                        SUM(CardTenders.Amount) AS [TotalAmount], 
                        SaleHead.Till AS [Till],
                        SaleHead.ShiftDate
                    FROM 
                        CSCMaster.dbo.Cards
                    INNER JOIN 
                        CSCTrans.dbo.CardTenders ON Cards.Description = CardTenders.Card_Name
                    INNER JOIN 
                        CSCTrans.dbo.SaleHead ON CardTenders.Sale_No = SaleHead.SALE_NO
                    WHERE 
                        SaleHead.ShiftDate = '{textReportParams.SaleDate?.ToString("yyyyMMdd")}'
                        AND CardTenders.TILL_NUM = SaleHead.TILL
                        AND Cards.CardType IN ('C', 'D')
                       {(_config.IsBackOffice ? "" : $"AND Store_Id = '{textReportParams.StoreID}'")}
                    GROUP BY 
                        Cards.Description, 
                        Cards.CardType, 
                        SaleHead.Till,
                        SaleHead.ShiftDate
                    ORDER BY 
                        SaleHead.Till;";

    IEnumerable<CreditCardSummary>? creditCardSummary = await Connection.QueryAsync<CreditCardSummary>(query, null, Transaction);
    return creditCardSummary;
  }

  public async Task<IEnumerable<ARCustomerSales>?> GetARCustomerSalesReportDataAsync(TextReportParams textReportParams)
  {
    string query = $@"SELECT 
                          SALETEND.Sale_No AS SaleNo,
                          SALETEND.SerNum AS SerNum,
                          SALEHEAD.SALE_DATE AS SaleDate,
                          SALEHEAD.SALE_TIME AS SaleTime,
                          SALELINE.AMOUNT AS Amount,
                          SALETEND.CCARD_NUM AS CardNumber,
                          SALELINE.SALE_TYPE AS SaleType,
                          SALELINE.STOCK_CODE AS StockCode,
                          SALELINE.QUANTITY AS Quantity,
                          SALELINE.DISCOUNT AS Discount,
                          SALETEND.TENDCLAS AS TendClass,
                          CLIENT.CL_CODE AS ClientCode,
                          CLIENT.CL_NAME AS ClientName
                      FROM 
                          CSCMaster.dbo.CLIENT 
                      LEFT JOIN 
                          CSCTrans.dbo.SALEHEAD 
                          ON SALEHEAD.CLIENT = CLIENT.CL_CODE
                          AND SALEHEAD.ShiftDate BETWEEN '{textReportParams.StartDate?.ToString("yyyyMMdd")}' AND '{textReportParams.EndDate?.ToString("yyyyMMdd")}'
                          {(_config.IsBackOffice ? "" : $"AND SaleHead.Store_Id = '{textReportParams.StoreID}'")}
                      LEFT JOIN 
                          CSCTrans.dbo.SALETEND 
                          ON SALETEND.Sale_No = SALEHEAD.Sale_No 
                          AND SALETEND.TILL_NUM = SALEHEAD.TILL 
                          AND SALETEND.TENDCLAS = 'ACCOUNT'
                          {(_config.IsBackOffice ? "" : "AND SALETEND.Store_Id = SaleHead.Store_Id")}
                      LEFT JOIN 
                          CSCTrans.dbo.SALELINE 
                          ON SALEHEAD.Sale_No = SALELINE.Sale_No 
                          AND SALEHEAD.TILL = SALELINE.TILL_NUM 
                          {(_config.IsBackOffice ? "" : "AND SALELINE.Store_Id = SaleHead.Store_Id")}
                      WHERE 
                          CLIENT.CL_ARCUST = 1
                          {(string.IsNullOrEmpty(textReportParams.StartCustomer) ? "" : $"AND CL_CODE>='{textReportParams.StartCustomer}'")}
                          {(string.IsNullOrEmpty(textReportParams.EndCustomer) ? "" : $"AND CL_CODE<='{textReportParams.EndCustomer}'")}
                      ORDER BY 
                          ClientCode";

    IEnumerable<ARCustomerSales?>? creditCardSummary = await Connection.QueryAsync<ARCustomerSales?>(query, null, Transaction);
    return creditCardSummary;
  }

  public async Task<IEnumerable<DailyFuelDiscount>> GetDailyFuelDiscountReportDataAsync(TextReportParams textReportParams, string fuelDept)
  {
    string storeClause = "";
    string gradeClause = "";

    if (!_config.IsBackOffice)
    {
      storeClause = @$"AND SaleLine.Store_Id = SaleHead.Store_Id
                       AND SaleHead.Store_Id = '{textReportParams.StoreID}'";

      gradeClause = $"AND G.Store_Id = {textReportParams.StoreID}";
    }

    var query = $@" SELECT
                      SUM(SaleLine.DISCOUNT) AS Discount,
                      SaleLine.STOCK_CODE AS StockCode,
                      SaleLine.SALE_TYPE AS SaleType
                    FROM CSCPump.[dbo].Grade G
                    JOIN CSCTrans.dbo.SaleLine ON SaleLine.STOCK_CODE = G.Stock_Code
                    JOIN CSCTrans.dbo.SaleHead 
                      ON SaleLine.Sale_No = SaleHead.SALE_NO AND SaleLine.TILL_NUM = SaleHead.TILL
                    WHERE SaleHead.ShiftDate = '{textReportParams.SaleDate?.ToString("yyyyMMdd")}' 
                      AND SaleLine.DEPT = {fuelDept}
                      AND G.Stock_Code = SaleLine.STOCK_CODE
                    {gradeClause}
                    {storeClause}
                    GROUP BY
                        G.Stock_Code,
                        G.StockGradeID,
                        SaleLine.STOCK_CODE,
                        SaleLine.SALE_TYPE
                    ORDER BY G.StockGradeID";

    IEnumerable<DailyFuelDiscount>? dailyFuelDiscounts = await Connection.QueryAsync<DailyFuelDiscount>(query, null, Transaction);
    return dailyFuelDiscounts;
  }

  public async Task<IEnumerable<FuelSaleVariance>> GetSaleVarianceReportDataAsync(TextReportParams textReportParams)
  {
    var query = $@"SELECT 
                        A.PumpID,
                        G.FullName AS GradeName,
                        SUM(SALELINE.QUANTITY) AS SumOfQty, 
                        SUM(SALELINE.AMOUNT - SALELINE.Discount - SALELINE.Disc_Adj) AS SumOfExt,
                        SUM(TH.SalesLiters) AS SumOfSalesLiters,
                        SUM(TH.SalesDollars) AS SumOfSalesDollars
                    FROM 
                        CSCPump.[dbo].Grade G
                    RIGHT JOIN 
                        CSCPump.[dbo].Assignment A ON G.ID = A.GradeID
                    LEFT JOIN 
                        CSCTrans.[dbo].SALEHEAD SALEHEAD ON
                        SALEHEAD.ShiftDate = '{textReportParams.SaleDate?.ToString("yyyyMMdd")}'
                    LEFT JOIN 
                        CSCTrans.[dbo].SALELINE SALELINE ON SALEHEAD.SALE_NO = SALELINE.SALE_NO 
                        AND SALELINE.PumpID = A.PumpID 
                        AND SALELINE.GradeID = G.ID
                    LEFT JOIN 
                        CSCPump.[dbo].TotalizerHist TH ON TH.PumpID = A.PumpID 
                        AND TH.Grade = G.ID 
                        AND TH.ShiftDate = SALEHEAD.ShiftDate
                    GROUP BY 
                        A.PumpID, 
                        G.FullName,
                        G.StockGradeID
                    ORDER BY 
                        A.PumpID, 
                        G.StockGradeID";

    IEnumerable<FuelSaleVariance>? fuelSaleVariance = await Connection.QueryAsync<FuelSaleVariance>(query, null, Transaction);
    return fuelSaleVariance;
  }

  public async Task<IEnumerable<EmployeesAndShifts>?> GetEmployeesAndShiftsAsync(TextReportParams textReportParams)
  {
    StringBuilder query = new();

    query.AppendLine($@"SELECT DISTINCT U_Name AS Employee,
                    [User] AS EmployeeID,
                    ShiftDate AS ShiftDate
                   FROM CSCTrans.dbo.SaleHead
	                  LEFT JOIN
	                  CSCAdmin.dbo.[USER] 
	                  ON [USER].U_Code = SaleHead.[USER]
	                  WHERE ShiftDate >= CAST('{textReportParams.StartDate?.ToString("yyyyMMdd")}' AS DATE) 
                    AND ShiftDate <= CAST('{textReportParams.EndDate?.ToString("yyyyMMdd")}' AS DATE)");

    if (!string.IsNullOrEmpty(textReportParams.StartEmployee))
      query.AppendLine($"AND [User] >= '{textReportParams.StartEmployee}'");

    if (!string.IsNullOrEmpty(textReportParams.EndEmployee))
      query.AppendLine($"AND [User] <= '{textReportParams.EndEmployee}'");
    var x = query.ToString();

    IEnumerable<EmployeesAndShifts>? employeesAndShifts = await Connection.QueryAsync<EmployeesAndShifts>(query.ToString(), null, Transaction);
    return employeesAndShifts;
  }

  public async Task<IEnumerable<ShiftAnalysis>?> GetShiftAnalysisReportDataAsync(IEnumerable<EmployeesAndShifts> employeesAndShifts, TextReportParams textReportParams, string fuelDept)
  {
    List<string> usernames = employeesAndShifts.Select(e => e.EmployeeID!).Distinct().ToList();
    List<DateTime?> shiftDates = employeesAndShifts.Select(e => e.ShiftDate).Distinct().ToList();

    if (!usernames.Any() || !shiftDates.Any())
      return null;

    var userTable = new DataTable();
    userTable.Columns.Add("Username", typeof(string));
    userTable.Columns.Add("Date", typeof(DateTime));

    foreach (var employeesAndShift in employeesAndShifts)
      userTable.Rows.Add(employeesAndShift.EmployeeID, employeesAndShift.ShiftDate);

    var parameters = new
    {
      EmployeesAndShifts = userTable.AsTableValuedParameter("EmployeesAndShiftsTableType"),
      FuelDept = fuelDept
    };

    IEnumerable<ShiftAnalysis>? shiftAnalysisReportData = await Connection.QueryAsync<ShiftAnalysis>(
      "Sproc_GetSaleAnalysisData", parameters, Transaction, commandType: CommandType.StoredProcedure);

    return shiftAnalysisReportData;
  }

  public async Task<IEnumerable<FleetCardSale>?> GetFleetCardSaleReportDataAsync(TextReportParams textReportParams)
  {
    string cardNumberClause = "";
    string storeClause = "";

    if (!string.IsNullOrEmpty(textReportParams.StartCard))
    {
      cardNumberClause += $"AND Card_Number >= '{textReportParams.StartCard}'";
    }

    if (!string.IsNullOrEmpty(textReportParams.EndCard))
    {
      cardNumberClause += $"AND Card_Number <= '{textReportParams.EndCard}'";
    }

    if (!_config.IsBackOffice)
    {
      storeClause = $@"AND CardTenders.Store = SaleHead.Store_Id
                       AND SaleHead.Store_Id = '{textReportParams.StoreID}'";
    }

    string sql = $@"SELECT 
                    CardTenders.TransactionDate,
                    CardTenders.Sale_No AS SaleNo,
                    CardTenders.TILL_NUM AS TillNumber,
                    CardTenders.Card_Type AS CardType,
                    CardTenders.Card_Name AS CardName,
                    CardTenders.Card_Number AS CardNumber,
                    CardTenders.TransactionTime,
                    CardTenders.Amount AS TendAmount,
                    SaleHead.SALE_AMT AS SaleAmount,
                    SaleLine.STOCK_CODE AS StockCode,
                    SaleLine.DESCRIPT AS Description, 
                    SaleHead.T_TYPE AS Type,
                    SaleLine.QUANTITY AS Quantity,
                    SaleLine.PRICE AS Price,
                    SaleLine.DISCOUNT AS Discount,
                    SaleLine.LINE_NUM AS LineNumber,
                    SaleLine.AMOUNT AS Amount,
                    COALESCE(SUM(S_LineTax.Tax_Added_Amount), 0) AS LineTax,
                    COALESCE(SUM(SaleKitChgTax.Tax_Added_Amount), 0) AS SaleKitTax,
                    COALESCE(SUM(ChargeTax.Tax_Added_Amount), 0) AS ChargeTax,
                    COALESCE(SUM(SALECHG.AMOUNT), 0) AS SaleCharge,
                    COALESCE(SUM(SaleKitChg.AMOUNT), 0) AS SaleKitCharge
                FROM
                  CSCTrans.dbo.CardTenders
                INNER JOIN
                  CSCTrans.dbo.SaleHead ON CardTenders.Sale_No = SaleHead.Sale_No 
                    AND SaleHead.TILL = CardTenders.TILL_NUM 
                LEFT JOIN 
                  CSCTrans.dbo.SALELINE ON SALELINE.Sale_No = CardTenders.Sale_No 
                    AND SALELINE.TILL_NUM = CardTenders.TILL_NUM
                LEFT JOIN 
                  CSCTrans.dbo.S_LineTax ON S_LineTax.Sale_No = CardTenders.Sale_No 
                    AND S_LineTax.TILL_NUM = CardTenders.TILL_NUM 
                    AND S_LineTax.Line_No = SALELINE.LINE_NUM
                    {(_config.IsBackOffice ? "" : $"AND S_LineTax.Store='{textReportParams.StoreID}'")}
                LEFT JOIN 
                  CSCTrans.dbo.SaleKitChgTax ON SaleKitChgTax.Sale_No = CardTenders.Sale_No 
                    AND SaleKitChgTax.TILL_NUM = CardTenders.TILL_NUM 
                    AND SaleKitChgTax.Line_No = SALELINE.LINE_NUM
                LEFT JOIN 
                  CSCTrans.dbo.ChargeTax ON ChargeTax.Sale_No = CardTenders.Sale_No 
                    AND ChargeTax.TILL_NUM = CardTenders.TILL_NUM 
                    AND ChargeTax.Line_No = SALELINE.LINE_NUM
                LEFT JOIN 
                  CSCTrans.dbo.SALECHG ON SALECHG.Sale_No = CardTenders.Sale_No 
                    AND SALECHG.TILL_NUM = CardTenders.TILL_NUM 
                    AND SALECHG.Line_No = SALELINE.LINE_NUM
                LEFT JOIN 
                  CSCTrans.dbo.SaleKitChg ON SaleKitChg.Sale_No = CardTenders.Sale_No 
                    AND SaleKitChg.TILL_NUM = CardTenders.TILL_NUM 
                    AND SaleKitChg.Line_No = SALELINE.LINE_NUM
                WHERE 
                    SaleHead.ShiftDate >= CAST('{textReportParams.StartDate?.ToString("yyyyMMdd")}' AS DATE) 
                    AND SaleHead.ShiftDate <= CAST('{textReportParams.EndDate?.ToString("yyyyMMdd")}' AS DATE) 
                    AND (CardTenders.Card_Type = 'F' OR CardTenders.Card_Type = 'A')
                    {cardNumberClause}
                    {storeClause}
                GROUP BY 
                    CardTenders.TransactionDate, 
                    CardTenders.Sale_No, 
                    CardTenders.TILL_NUM, 
                    CardTenders.Card_Type, 
                    CardTenders.Amount, 
                    CardTenders.Card_Name, 
                    CardTenders.Card_Number, 
                    CardTenders.TransactionTime, 
                    SaleHead.SALE_AMT, 
                    SaleHead.T_TYPE, 
                    SALELINE.Sale_No, 
                    SALELINE.TILL_NUM,
                    SaleLine.PRICE,
                    SaleLine.STOCK_CODE,
                    SaleLine.DESCRIPT,
                    SaleLine.QUANTITY,
                    SaleLine.DISCOUNT,
                    SaleLine.AMOUNT,
                    SALELINE.LINE_NUM
                ORDER BY 
                    CardTenders.Sale_No, 
                    CardTenders.Card_Name;";

    IEnumerable<FleetCardSale>? fleetCardSaleReportData = await Connection.QueryAsync<FleetCardSale>(sql, null, Transaction);

    return fleetCardSaleReportData;
  }

  public async Task<IEnumerable<AffdaSaleDetail>?> GetAffdaSaleDetailReportDataAsync(TextReportParams textReportParams)
  {
    string storeClause = "";

    if (!_config.IsBackOffice)
    {
      storeClause = $@"AND A.Store_Id=B.Store_Id
                       AND B.Store_Id='{textReportParams.StoreID}'";
    }

    string sql = $@"Select
                      A.Sale_No AS SaleNo,
                      A.DESCRIPT AS Description,
                      A.QUANTITY AS Quantity,
                      A.PRICE AS Price,
                      A.Rebate AS Rebate,
                      B.SALE_DATE AS TransactionDate,
                      B.SALE_TIME AS TransactionTime,
                      C.CL_CODE AS CustomerCode,
                      C.CL_NAME AS CustomerName
                    From 
                      [CSCTrans].[dbo].SaleLine as A
                      INNER JOIN (
                        [CSCTrans].[dbo].SaleHead as B
                        LEFT JOIN CSCMaster.dbo.CLIENT as C ON B.CLIENT = C.CL_CODE
                      ) ON A.Sale_No = B.Sale_No
                      AND A.TILL_NUM = B.TILL
                    Where 
                      B.ShiftDate >= CAST('{textReportParams.StartDate?.ToString("yyyyMMdd")}' AS DATE)
                      AND B.ShiftDate <= CAST('{textReportParams.EndDate?.ToString("yyyyMMdd")}' AS DATE)
                      AND A.FuelRebateUsed = 1
                      {storeClause}
                    ORDER BY
                      A.Sale_No,
                      A.LINE_NUM";

    IEnumerable<AffdaSaleDetail>? affdaSaleDetailReportData = await Connection.QueryAsync<AffdaSaleDetail>(sql, null, Transaction);
    return affdaSaleDetailReportData;
  }

  public async Task<IEnumerable<AffdaSaleDetail>?> GetAffdaTaxSaleSummaryReportDataAsync(TextReportParams textReportParams)
  {
    string storeClause = "";

    if (!_config.IsBackOffice)
    {
      storeClause = $@"AND A.Store_Id=B.Store_Id
                       AND B.Store_Id='{textReportParams.StoreID}'";
    }

    string sql = $@"SELECT
                    A.STOCK_CODE AS StockCode,
                    A.DESCRIPT AS Description, 
                    SUM(A.QUANTITY) AS Quantity, 
                    SUM(A.Amount) AS Price, 
                    SUM(A.DISCOUNT) AS Rebate, 
                    B.CLIENT as CustomerCode,
                    C.CL_NAME as CustomerName
                    FROM CSCTrans.dbo.SaleLine AS A 
                    INNER JOIN CSCTrans.dbo.SaleHead AS B 
                    ON A.Sale_No = B.Sale_No 
                    AND A.TILL_NUM = B.TILL
                    LEFT JOIN CSCMaster.dbo.CLIENT AS C on C.CL_CODE = ISNULL(B.CLIENT, '')
                    WHERE B.ShiftDate >= CAST('{textReportParams.StartDate?.ToString("yyyyMMdd")}' AS DATE)
                      AND B.ShiftDate <= CAST('{textReportParams.EndDate?.ToString("yyyyMMdd")}' AS DATE)
                      AND A.FuelRebateUsed = 1
                      {storeClause}
                    GROUP BY A.STOCK_CODE, A.DESCRIPT, B.CLIENT, C.CL_NAME, C.CL_CODE
                    ORDER BY C.CL_CODE, C.CL_NAME, A.DESCRIPT";

    IEnumerable<AffdaSaleDetail>? affdaSaleDetailReportData = await Connection.QueryAsync<AffdaSaleDetail>(sql, null, Transaction);
    return affdaSaleDetailReportData;
  }

  public async Task<IEnumerable<TankDip>> GetTankDipReportDataAsync(TextReportParams textReportParams)
  {
    var startDate = textReportParams.StartDate?.ToString("yyyyMMdd") ?? string.Empty;
    var endDate = textReportParams.EndDate?.ToString("yyyyMMdd") ?? string.Empty;

    string storeClause = "";

    if (!_config.IsBackOffice)
    {
      storeClause = $@"AND A.Store_Id='{textReportParams.StoreID}'";
    }

    string query = $@"SELECT *
                  FROM [CSCTrans].[dbo].DipInput AS A
                  WHERE DipDate >= '{startDate}'
                    AND DipDate <= '{endDate}'
                  {storeClause}
                  ORDER BY DipDate, TankID;";

    IEnumerable<TankDip>? tankDips = await Connection.QueryAsync<TankDip>(query, null, Transaction);
    return tankDips;
  }

  public async Task<IEnumerable<AdagioDto>> GetAdagioReportDataAsync(TextReportParams textReportParams)
  {
    var startDate = textReportParams.StartDate?.ToString("yyyyMMdd") ?? string.Empty;
    var endDate = textReportParams.EndDate?.ToString("yyyyMMdd") ?? string.Empty;

    string query = $@"SELECT
                    SALEHEAD.Client,
                    CLIENT.CL_NAME,
                    SALETEND.Sale_No,
                    SALEHEAD.SALE_DATE,
                    SALEHEAD.SALE_TIME,
                    SALETEND.AMTTEND
                FROM
                    [CSCTrans].[dbo].SALETEND
                INNER JOIN
                    [CSCTrans].[dbo].SALEHEAD ON SALETEND.Sale_No = SALEHEAD.Sale_No
                    AND SALETEND.TILL_NUM = SALEHEAD.TILL
                INNER JOIN
                    CSCMASTER.dbo.CLIENT CLIENT ON SALEHEAD.Client = CLIENT.Cl_code
                WHERE
                    SALEHEAD.ShiftDate >= '{startDate}'
                    AND SALEHEAD.ShiftDate <= '{endDate}'
                    AND SALETEND.TENDCLAS = 'ACCOUNT'
                    AND CLIENT.CL_ARCUST = 1
                    {(string.IsNullOrEmpty(textReportParams.StartCustomer) ? "" : $"AND CLIENT.CL_CODE >= '{textReportParams.StartCustomer}'")}
                    {(string.IsNullOrEmpty(textReportParams.EndCustomer) ? "" : $"AND CLIENT.CL_CODE <= '{textReportParams.EndCustomer}'")}
                ORDER BY
                    SALEHEAD.Client, SALEHEAD.Sale_No, SALEHEAD.SALE_DATE;";

    IEnumerable<AdagioDto>? adagioReportData = await Connection.QueryAsync<AdagioDto>(query, null, Transaction);
    return adagioReportData;
  }

  public async Task<IEnumerable<IncompleteTransactionsDto>> GetIncompleteTransactionsReportDataAsync(TextReportParams textReportParams)
  {
    var startDate = textReportParams.StartDate?.ToString("yyyyMMdd") ?? string.Empty;
    var endDate = textReportParams.EndDate?.ToString("yyyyMMdd") ?? string.Empty;

    string storeClause = "", saleClause = "";

    if (!_config.IsBackOffice)
    {
      storeClause = $@"AND SaleHead.Store_Id='{textReportParams.StoreID}'";
      saleClause = $@"AND SaleHead.Store_id = InComplete.Store_Id";
    }

    string query = $@"SELECT 
                    SaleHead.SALE_DATE AS SaleDate,
                    SaleHead.SALE_TIME AS SaleTime,
                    SaleHead.SALE_NO AS SaleNo,
                    CardTenders.Card_Number AS CardNumber,
                    CardTenders.Expiry_Date AS ExpiryDate,
                    CardTenders.Card_Type AS CardType,
                    InComplete.CompletionAmount AS CompletionAmount,
                    InComplete.ApprovalNum AS ApprovalNum
                    FROM [CSCPayPumpHist].[dbo].[InComplete] 
                    INNER JOIN CSCTRANS.dbo.SaleHead SaleHead 
                        ON SaleHead.Sale_No = InComplete.InvoiceNumber
                        {saleClause}
                    INNER JOIN CSCTRANS.dbo.CardTenders CardTenders 
                        ON SaleHead.Sale_No = CardTenders.SALE_NO 
                        AND SaleHead.Till = CardTenders.Till_NUM
                    WHERE SaleHead.ShiftDate >= '{startDate}'
                      AND SaleHead.ShiftDate <= '{endDate}'
                      AND SaleHead.Till = 100 {storeClause};";

    IEnumerable<IncompleteTransactionsDto>? IncompleteTranstionsReportData = await Connection.QueryAsync<IncompleteTransactionsDto>(query, null, Transaction);
    return IncompleteTranstionsReportData;
  }

  public async Task<IEnumerable<EmployeeCommissionDto>> GetEmployeeCommissionReportDataAsync(TextReportParams textReportParams)
  {
    var query = new StringBuilder();

    var startDate = textReportParams.StartDate?.ToString("yyyyMMdd") ?? string.Empty;
    var endDate = textReportParams.EndDate?.ToString("yyyyMMdd") ?? string.Empty;

    query.AppendLine($@"SELECT
                    [SALEHEAD].[USER] AS Code,
                    [USER].[U_NAME] AS Name,
                    [SALELINE].[STOCK_CODE] AS StockCode,
                    [SALELINE].[DESCRIPT] AS Description,
                    [SALEHEAD].[ShiftDate],
                    [SALELINE].[QUANTITY],
                    [SALELINE].[AMOUNT],
                    [SALELINE].[DEPT],
                    [SALELINE].[SUB_DEPT] AS SubDept,
                    [SALELINE].[SUB_Detail] AS SubDetail,
                    ROW_NUMBER() OVER (PARTITION BY [SALEHEAD].[User] ORDER BY [SALEHEAD].[ShiftDate]) AS [RowNumber]
                FROM [CSCTrans].[dbo].[SALEHEAD]
                LEFT JOIN [CSCTrans].[dbo].[SALELINE]
	                ON [SALEHEAD].[TILL] = [SALELINE].[TILL_NUM] AND [SALEHEAD].[SALE_NO] = [SALELINE].[SALE_NO]
                LEFT JOIN [CSCAdmin].[dbo].[USER] ON [SALEHEAD].[User] = [USER].[U_CODE]
                WHERE
                    [SALEHEAD].[ShiftDate] >= '{startDate}'
                    AND [SALEHEAD].[ShiftDate] <= '{endDate}'");

    if (!string.IsNullOrEmpty(textReportParams.StartEmployee))
    {
      query.AppendLine($@"AND[SALEHEAD].[User] >= '{textReportParams.StartEmployee}'");
    }
    if (!string.IsNullOrEmpty(textReportParams.EndEmployee))
    {
      query.AppendLine($@"AND [SALEHEAD].[User] <= '{textReportParams.EndEmployee}'");
    }

    query.AppendLine($@"ORDER BY [SALEHEAD].[User], [SALEHEAD].[ShiftDate];");

    IEnumerable<EmployeeCommissionDto>? employeeCommissionReportData = await Connection.QueryAsync<EmployeeCommissionDto>(query.ToString(), null, Transaction);
    return employeeCommissionReportData;
  }

  public async Task<IEnumerable<EmployeeCommission>> GetEmployeeCommissionTableDataAsync(TextReportParams textReportParams)
  {
    string query = $@"SELECT 
                    [EmployeeID],
                    [StockCode],
                    [Dept],
                    [SubDept],
                    [SubDetail],
                    [Name],
                    [Commission],
                    [CommissionByDol],
                    [ByStockCode],
                    [Apply]
                  FROM [CSCTrans].[dbo].[EmpCommissionData]";

    IEnumerable<EmployeeCommission>? employeeCommissionTableData = await Connection.QueryAsync<EmployeeCommission>(query, null, Transaction);
    return employeeCommissionTableData;
  }

  public async Task<IEnumerable<SalesDto>> GetFinancialReportSalesDeptDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].[Sproc_SalesDept] '{textReportParams.SaleDate}'";

    IEnumerable<SalesDto>? financialReportSalesDeptData = await Connection.QueryAsync<SalesDto>(query, null, Transaction);
    return financialReportSalesDeptData;
  }

  public async Task<IEnumerable<SalesDto>> GetFinancialReportSalesDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_Sales '{textReportParams.SaleDate}','01',{textReportParams.NullDept},{textReportParams.Period}";

    IEnumerable<SalesDto>? financialReportSalesData = await Connection.QueryAsync<SalesDto>(query, null, Transaction);
    return financialReportSalesData;
  }

  public async Task<IEnumerable<TaxesDto>> GetFinancialReportSaleTaxesDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].[Sproc_SaleTaxes] '{textReportParams.SaleDate}'";

    IEnumerable<TaxesDto>? financialReportSaleTaxesData = await Connection.QueryAsync<TaxesDto>(query, null, Transaction);
    return financialReportSaleTaxesData;
  }

  public async Task<IEnumerable<TaxesDto>> GetFinancialReportTaxesDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_Taxes '{textReportParams.SaleDate}',{textReportParams.Period}";

    IEnumerable<TaxesDto>? financialReportTaxesData = await Connection.QueryAsync<TaxesDto>(query, null, Transaction);
    return financialReportTaxesData;
  }

  public async Task<IEnumerable<ChargeDto>> GetFinancialReportSaleChargesDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_SaleCharges '{textReportParams.SaleDate}'";

    IEnumerable<ChargeDto>? financialReportSaleChargesData = await Connection.QueryAsync<ChargeDto>(query, null, Transaction);
    return financialReportSaleChargesData;
  }

  public async Task<IEnumerable<ChargeDto>> GetFinancialReportChargesDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_Charges '{textReportParams.SaleDate}',{textReportParams.Period}";

    IEnumerable<ChargeDto>? financialReportChargesData = await Connection.QueryAsync<ChargeDto>(query, null, Transaction);
    return financialReportChargesData;
  }

  public async Task<IEnumerable<TendersDto>> GetFinancialReportSaleTendersDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_SaleTenders '{textReportParams.SaleDate}'";

    IEnumerable<TendersDto>? financialReportSaleTendersData = await Connection.QueryAsync<TendersDto>(query, null, Transaction);
    return financialReportSaleTendersData;
  }

  public async Task<IEnumerable<TendersDto>> GetFinancialReportTendersDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_Tenders '{textReportParams.SaleDate}',{textReportParams.Period}";

    IEnumerable<TendersDto>? financialReportTendersData = await Connection.QueryAsync<TendersDto>(query, null, Transaction);
    return financialReportTendersData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportARTendersDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_ARTenders '{textReportParams.SaleDate}'";

    IEnumerable<FinancialReportDto>? financialReportARTendersData = await Connection.QueryAsync<FinancialReportDto>(query, null, Transaction);
    return financialReportARTendersData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportARPayDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_ARPay '{textReportParams.SaleDate}',{textReportParams.Period}";

    IEnumerable<FinancialReportDto>? financialReportARPayData = await Connection.QueryAsync<FinancialReportDto>(query, null, Transaction);
    return financialReportARPayData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportPaymntTendersDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_PaymntTenders '{textReportParams.SaleDate}'";

    IEnumerable<FinancialReportDto>? financialReportPaymntTendersData = await Connection.QueryAsync<FinancialReportDto>(query, null, Transaction);
    return financialReportPaymntTendersData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportPaymntDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_Paymnt '{textReportParams.SaleDate}', {textReportParams.Period}";

    IEnumerable<FinancialReportDto>? financialReportPaymntData = await Connection.QueryAsync<FinancialReportDto>(query, null, Transaction);
    return financialReportPaymntData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportPayoutDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_Payout '{textReportParams.SaleDate}', {textReportParams.Period}";

    IEnumerable<FinancialReportDto>? financialReportPayoutData = await Connection.QueryAsync<FinancialReportDto>(query, null, Transaction);
    return financialReportPayoutData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportOverPayDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_OverPay '{textReportParams.SaleDate}', {textReportParams.Period}";

    IEnumerable<FinancialReportDto>? financialReportOverPayData = await Connection.QueryAsync<FinancialReportDto>(query, null, Transaction);
    return financialReportOverPayData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportRefundDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_Refund '{textReportParams.SaleDate}', {textReportParams.Period}";

    IEnumerable<FinancialReportDto>? financialReportRefundData = await Connection.QueryAsync<FinancialReportDto>(query, null, Transaction);
    return financialReportRefundData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportDelVoidDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_DelVoid '{textReportParams.SaleDate}', {textReportParams.Period}";

    IEnumerable<FinancialReportDto>? financialReportDelVoidData = await Connection.QueryAsync<FinancialReportDto>(query, null, Transaction);
    return financialReportDelVoidData;
  }

  public async Task<IEnumerable<CustomerCntDto>> GetFinancialReportCustomerCntDataAsync(TextReportParams textReportParams)
  {
    string query = $@"exec [CSCTrans].[dbo].Sproc_CustomerCnt '{textReportParams.SaleDate}', {textReportParams.Period}";

    IEnumerable<CustomerCntDto>? financialReportCustomerCntData = await Connection.QueryAsync<CustomerCntDto>(query, null, Transaction);
    return financialReportCustomerCntData;
  }

  public async Task<IEnumerable<CustomerSalesDetail>> GetCustomerSalesDetailReportDataAsync(TextReportParams textReportParams)
  {
    var fromClause = "[CSCTrans].[dbo].[SALEHEAD]";
    var cardNumber = "[SALEHEAD].[LoyaltyCard]";

    if (textReportParams.AROnly)
    {
      fromClause = "([CSCTrans].[dbo].[SALETEND] INNER JOIN [CSCTrans].[dbo].[SALEHEAD] ON [SALETEND].[SALE_NO] = [SALEHEAD].[SALE_NO] AND [SALETEND].[TILL_NUM] = [SALEHEAD].[TILL])";
      cardNumber = "[SALETEND].[CCARD_NUM]";
    }

    var query = new StringBuilder();
    query.AppendLine($@"
      SELECT
        [SALEHEAD].[SALE_NO] AS [SaleNumber],
        [SALEHEAD].[CLIENT] AS [ClientCode],
        [CLIENT].[CL_NAME] AS [ClientName],
        {cardNumber} AS [CardNumber],
        [SALEHEAD].[SALE_DATE] AS [SaleDate],
        [SALEHEAD].[SALE_TIME] AS [SaleTime],
        [SALEHEAD].[OverPayment] AS [Overpayment],
        [SALELINE].[STOCK_CODE] AS [StockCode],
        [SALELINE].[DESCRIPT] AS [StockName],
        [SALELINE].[LINE_NUM] AS [LineNumber],
        [SALELINE].[TILL_NUM] AS [TillNumber],
        [SALELINE].[QUANTITY],
        [SALELINE].[AMOUNT],
        [SALELINE].[DISCOUNT],
        [SALELINE].[DISC_ADJ] AS [DiscountAdjustment],
        [SALELINE].[SALE_TYPE] AS [SaleType],
        SUM(ISNULL([S_LineTax].[Tax_Added_Amount], 0)) AS [LineTaxAmount],
        SUM(ISNULL([ChargeTax].[Tax_Added_Amount], 0)) AS [ChargeTaxAmount],
        SUM(ISNULL([SaleKitChgTax].[Tax_Added_Amount], 0)) AS [KitTaxAmount],
        SUM(ISNULL([SALECHG].[AMOUNT], 0)) AS [SaleAmount],
        SUM(ISNULL([SaleKitChg].[Amount], 0)) AS [KitAmount]
      FROM {fromClause}
      INNER JOIN [CSCTrans].[dbo].[SALELINE] 
        ON [SALEHEAD].[SALE_NO] = [SALELINE].[SALE_NO] AND [SALEHEAD].[TILL] = [SALELINE].[TILL_NUM]
      INNER JOIN [CSCMaster].[dbo].[CLIENT]
        ON [SALEHEAD].[CLIENT] = [CLIENT].[CL_CODE]
      LEFT JOIN [CSCTrans].[dbo].[S_LineTax]
        ON [S_LineTax].[Sale_No] = [SALEHEAD].[SALE_NO] 
        AND [S_LineTax].[TILL_NUM] = [SALELINE].[TILL_NUM] 
        AND [S_LineTax].[Line_No] = [SALELINE].[LINE_NUM]
      LEFT JOIN [CSCTrans].[dbo].[ChargeTax]
        ON [ChargeTax].[Sale_No] = [SALEHEAD].[SALE_NO] 
        AND [ChargeTax].[TILL_NUM] = [SALELINE].[TILL_NUM] 
        AND [ChargeTax].[Line_No] = [SALELINE].[LINE_NUM]
      LEFT JOIN [CSCTrans].[dbo].[SaleKitChgTax]
        ON [SaleKitChgTax].[Sale_No] = [SALEHEAD].[SALE_NO] 
        AND [SaleKitChgTax].[TILL_NUM] = [SALELINE].[TILL_NUM] 
        AND [SaleKitChgTax].[Line_No] = [SALELINE].[LINE_NUM] 
        AND [SaleKitChgTax].[Tax_Included] <> 1
      LEFT JOIN [CSCTrans].[dbo].[SALECHG]
        ON [SALECHG].[SALE_NO] = [SALEHEAD].[SALE_NO] 
        AND [SALECHG].[TILL_NUM] = [SALELINE].[TILL_NUM] 
        AND [SALECHG].[LINE_NO] = [SALELINE].[LINE_NUM]
      LEFT JOIN [CSCTrans].[dbo].[SaleKitChg]
        ON [SaleKitChg].[Sale_No] = [SALEHEAD].[SALE_NO] 
        AND [SaleKitChg].[TILL_NUM] = [SALELINE].[TILL_NUM] 
        AND [SaleKitChg].[Line_No] = [SALELINE].[LINE_NUM]
      WHERE [SALEHEAD].[SALE_DATE] BETWEEN @StartDate AND @EndDate");

    // Filter by customers (start/end range)
    if (!string.IsNullOrEmpty(textReportParams.StartCustomer))
    {
      query.AppendLine("AND [CLIENT].[CL_CODE] >= @StartCustomer");
    }

    if (!string.IsNullOrEmpty(textReportParams.EndCustomer))
    {
      query.AppendLine("AND [CLIENT].[CL_CODE] <= @EndCustomer");
    }

    // Filter by ARO customers if needed
    if (textReportParams.AROnly)
    {
      query.AppendLine("AND [CLIENT].[CL_ARCUST] = 1");
      query.AppendLine("AND [SALETEND].[TENDCLAS] = 'ACCOUNT'");
    }

    // Filter by store ID if provided
    if (!string.IsNullOrEmpty(textReportParams.StoreID))
    {
      query.AppendLine("AND [SALETEND].[Store_Id] = @StoreID");
      query.AppendLine("AND [SALELINE].[Store_Id] = @StoreID");
      query.AppendLine("AND [SALEHEAD].[Store_Id] = @StoreID");
    }

    query.AppendLine($"GROUP BY [SALEHEAD].[SALE_NO], [SALEHEAD].[CLIENT], [CLIENT].[CL_NAME], {cardNumber}, [SALEHEAD].[SALE_DATE], [SALEHEAD].[SALE_TIME], [SALEHEAD].[OverPayment], [SALELINE].[STOCK_CODE], [SALELINE].[DESCRIPT], [SALELINE].[LINE_NUM], [SALELINE].[TILL_NUM], [SALELINE].[QUANTITY], [SALELINE].[AMOUNT], [SALELINE].[DISCOUNT], [SALELINE].[DISC_ADJ], [SALELINE].[SALE_TYPE]");
    query.AppendLine("ORDER BY [SALEHEAD].[CLIENT], [SALEHEAD].[SALE_NO], [SALELINE].[LINE_NUM]");

    return await Connection.QueryAsync<CustomerSalesDetail>(query.ToString(), textReportParams, Transaction);
  }
}
