namespace InfonetCSCR.Data.Repositories;

public class ElectronicJournalRepository : BaseRepository, IElectronicJournalRepository
{
  private readonly IApplicationConfig _config;

  public ElectronicJournalRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  public async Task<IEnumerable<ElectronicJournalReportData>?> GetElectronicJournalReportDataAsync(string saleNo, bool isFromTills = false)
  {
    string dbSource = isFromTills ? "CSCTills" : "CSCTrans";
    string sql = $@"SELECT 
        SALEHEAD.SALE_NO AS SaleNo,
        SALEHEAD.TILL AS Till,
        SALEHEAD.[USER] AS Cashier,
        SALEHEAD.CLIENT AS Customer,
        SALEHEAD.SALE_DATE AS SaleDate,
        SALEHEAD.SALE_TIME AS SaleTime,
        SALEHEAD.ShiftDate AS ShiftDate,
        SALEHEAD.INVC_DISC,
        SALEHEAD.T_TYPE AS Type,
        SALELINE.LINE_NUM AS ID,
        SALELINE.STOCK_CODE AS Code,
        SALELINE.DESCRIPT AS Description,
        SALELINE.QUANTITY AS Qty,
        SALELINE.PRICE AS Price,
        SALELINE.DISCOUNT AS Disc,
        SALELINE.AMOUNT AS Amount,
        SALELINE.DEPT,
        SALELINE.PumpID AS PumpID,
        DelLines.Del_Seq AS DelSeq,
        DelLines.STOCK_CODE AS DelLines_STOCK_CODE,
        DelLines.DESCRIPT AS DelLines_DESCRIPT,
        DelLines.QUANTITY AS DelLines_QUANTITY,
        DelLines.PRICE AS DelLines_PRICE,
        DelLines.DISCOUNT AS DelLines_DISCOUNT,
        DelLines.AMOUNT AS DelLines_AMOUNT
    FROM 
        {dbSource}.dbo.SALEHEAD AS SALEHEAD
    LEFT OUTER JOIN 
        {dbSource}.dbo.SALELINE AS SALELINE ON SALEHEAD.SALE_NO = SALELINE.SALE_NO
    LEFT OUTER JOIN 
        {dbSource}.dbo.DelLines AS DelLines ON SALEHEAD.SALE_NO = DelLines.SALE_NO 
        AND SALEHEAD.T_TYPE = 'VOID'
    WHERE 
        SALEHEAD.SALE_NO = @SaleNo
    ORDER BY 
        SALEHEAD.SALE_NO,
        SALELINE.LINE_NUM";

    IEnumerable<ElectronicJournalReportData> ejReportData = await Connection.QueryAsync<ElectronicJournalReportData>(
        sql,
        new { SaleNo = saleNo, DBSource = dbSource },
        transaction: Transaction
    );

    return ejReportData;
  }

  public async Task<IEnumerable<ElectronicJournalSubreport?>> GetEJSubreportDataAsync(string saleNo, bool isFromTills = false)
  {
    string dbSource = isFromTills ? "CSCTills" : "CSCTrans";

    string sql = $@"SELECT 
                    SH.SALE_NO AS SaleNo,
                    SH.ASSOC_AMT AS AssocAmt,
                    SH.TEND_AMT AS TendAmt,
                    SH.SALE_AMT AS SaleAmt,
                    SH.CHANGE AS ChangeAmt,
                    SH.OverPayment AS OverPaymentAmt,
                    SH.INVC_DISC AS InvoiceDiscount,
                    ST.TENDNAME AS TendName,
                    ST.AMTTEND AS AmtTend,
                    ST.AMTUSED AS AmtUsed,
                    SCT.Card_Number AS CardNumber,
                    SST.Tax_Name AS TaxName,
                    SST.Tax_Code AS TaxCode,
                    SST.Tax_Added_Amount AS TaxAddedAmount,
                    SST.Tax_Included_Amount AS TaxIncludedAmount,
                    IsHidden = 0 
                  FROM 
                    {dbSource}.dbo.SALEHEAD AS SH
                  LEFT OUTER JOIN 
                    {dbSource}.dbo.S_SaleTax AS SST ON SH.SALE_NO = SST.Sale_No
                  INNER JOIN 
                    {dbSource}.dbo.SALETEND AS ST ON SH.SALE_NO = ST.SALE_NO
                  LEFT OUTER JOIN 
                    {dbSource}.dbo.Selected_Sale_CardTender AS SCT
                  ON 
                    ST.SALE_NO = SCT.Sale_No 
                    AND ST.TENDNAME = SCT.TENDNAME
                  WHERE 
                    SH.SALE_NO = @SaleNo;";
    
    IEnumerable<ElectronicJournalSubreport?> ejReportSubreportData = await Connection.QueryAsync<ElectronicJournalSubreport?>(
        sql,
        new { SaleNo = saleNo, DBSource = dbSource },
        transaction: Transaction
    );

    return ejReportSubreportData;
  }

  public async Task<IEnumerable<string>> GetElectronicJournalOptionsAsync()
  {
    string sql = $@"SELECT Option_Name
                    FROM [CSCAdmin].[dbo].Search_Wizard
                    GROUP BY Option_Name, Display_Seq 
                    ORDER BY Display_Seq;";

    IEnumerable<string> options = await Connection.QueryAsync<string>(sql, null, Transaction);
    return options;
  }

  public async Task<IEnumerable<EJSearchDescriptionDto>> GetElectronicJournalFiltersAsync(string optionName, bool isWareHouse)
  {
    string sql = $@"SELECT
                      Option_Name AS OptionName,
                      Sequence AS Sequence,
                      Display_Seq AS DisplaySeq,
                      A.DescriptionID AS DescriptionId,
                      Table_Name AS TableName,
                      Field_Name AS FieldName,
                      Field_Type AS FieldType,
                      Description AS Description,
                      Type AS Type,
                      SelDBName AS SelDBName,
                      SelTableName AS SelTableName,
                      SelFieldName AS SelFieldName,
                      SelKeyField AS SelKeyField,
                      Active AS Active
                    FROM
                      [CSCAdmin].[dbo].Search_Wizard AS A
                    INNER JOIN
                      [CSCAdmin].[dbo].Search_Desc AS B
                    ON
                      A.DescriptionID = B.DescriptionID 
                    WHERE
                      Option_Name = @OptionName AND Active = 1
                      {(!_config.IsBackOffice && !isWareHouse ? "AND Description != 'Store'" : "")}
                    ORDER BY Sequence";

    IEnumerable<EJSearchDescriptionDto> searchDescriptions = await Connection.QueryAsync<EJSearchDescriptionDto>(sql, new { OptionName = optionName }, Transaction);
    return searchDescriptions;
  }

  public async Task<IEnumerable<string>> GetSearchDropdownValuesAsync(EJSearchDescriptionDto desc)
  {
    string sql = $@"SELECT
                      {desc.SelFieldName} AS FName,
                      {desc.SelKeyField} AS KName
                    FROM
                      [{desc.SelDBName}].[dbo].{desc.SelTableName}
                    ORDER BY FName;";

    IEnumerable<string> options = await Connection.QueryAsync<string>(sql, null, Transaction);
    return options;
  }

  public async Task<IEnumerable<ElectronicJournalReportData>?> GetElectronicJournalGridDataAsync(EJSearchParamDto ejSearchParam, string searchString)
  {
    string sql = PrepareQueryForEJGridData(ejSearchParam, searchString, countOnly: false);
    IEnumerable<ElectronicJournalReportData>? ejGridData = await Connection.QueryAsync<ElectronicJournalReportData>(sql, null, Transaction);
    return ejGridData;
  }

  public async Task<int> GetElectronicJournalGridDataCountAsync(EJSearchParamDto ejSearchParam, string searchString)
  {
    string sql = PrepareQueryForEJGridData(ejSearchParam, searchString, countOnly: true);

    int dataCount = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return dataCount;
  }

  private string PrepareQueryForEJGridData(EJSearchParamDto ejSearchParam, string searchString, bool countOnly)
  {
    StringBuilder sql = new();

    if (countOnly)
      sql.AppendLine($@"SELECT
                    COUNT(*) AS EntryCount
                FROM ( ");

    if (_config.IsBackOffice)
    {
      sql.AppendLine($@"SELECT
                      A.Sale_No AS SaleNo,
                      CONVERT(VARCHAR, A.ShiftDate, 101) AS ShiftDate,
                      A.Till,
                      CAST(A.Sale_Amt AS DECIMAL(15,2)) AS Amount,
                      A.Client AS Customer,
                      A.T_Type AS Type,
                      A.SD AS SDT,
                      1 AS Source
                    FROM 
                      [CSCTills].[dbo].SaleHead AS A
                    LEFT OUTER JOIN
                      [CSCTills].[dbo].SaleLine AS B ON A.Sale_No = B.Sale_No
                    LEFT OUTER JOIN
                      [CSCTills].[dbo].SaleTend AS C ON A.Sale_No = C.Sale_No
                    WHERE
                      {searchString}
                    UNION
                    SELECT
                      A.Sale_No AS SaleNo,
                      CONVERT(VARCHAR, A.ShiftDate, 101) AS ShiftDate, 
                      A.Till,
                      CAST(A.Sale_Amt AS DECIMAL(15,2)) AS Amount, 
                      A.Client AS Customer,
                      A.T_Type AS Type,
                      A.SD AS SDT,
                      0 AS Source
                    FROM 
                      [CSCTrans].[dbo].SaleHead AS A
                    LEFT OUTER JOIN
                      [CSCTrans].[dbo].SaleLine AS B ON A.Sale_No = B.Sale_No
                    LEFT OUTER JOIN
                      [CSCTrans].[dbo].SaleTend AS C ON A.Sale_No = C.Sale_No
                    WHERE 
                      {searchString}
                    GROUP BY
                      A.Sale_No, A.ShiftDate, A.Till, A.Sale_Amt, A.Client, A.T_Type, A.SD 
                   {(countOnly ? ""
                   : @"ORDER BY
                      A.Sale_No DESC ")}");
    }
    else
    {
      if (ejSearchParam.IsWareHouse == false || ejSearchParam.StoreRadioOption == 'C')
      {
        // 'C' = Tills
        if (ejSearchParam.StoreRadioOption == 'C')
        {
          sql.AppendLine($@"SELECT distinct
                      A.Sale_No AS SaleNo,
                      CONVERT(varchar,ShiftDate,101) AS ShiftDate, 
                      Till AS Till, 
                      CAST(Sale_Amt AS DECIMAL (15,2)) AS Amount,
                      Client AS Customer,
                      T_Type AS Type,
                      SD AS SDT,
                      1 AS Source
                      FROM CSCTills.dbo.SaleHead AS A
                      LEFT OUTER JOIN CSCTills.dbo.SaleLine AS B
                      ON A.Sale_No=B.Sale_No LEFT OUTER JOIN CSCTills.dbo.SaleTend AS C ON A.Sale_No=C.Sale_No
                      WHERE {searchString}
                    { (countOnly ? "" : " ORDER BY A.Sale_No DESC ")}"); 
        }
        else
        {
          sql.AppendLine($@"SELECT 
                      A.Sale_No AS SaleNo,
                      CONVERT(varchar,ShiftDate,101) AS ShiftDate, 
                      Till AS Till, 
                      CAST(Sale_Amt AS DECIMAL (15,2)) AS Amount,
                      Client AS Customer,
                      T_Type AS Type,
                      SD AS SDT,
                      0 AS Source
                      FROM CSCTrans.dbo.SaleHead AS A
                      LEFT OUTER JOIN CSCTrans.dbo.SaleLine AS B
                      ON A.Sale_No=B.Sale_No LEFT OUTER JOIN CSCTrans.dbo.SaleTend AS C ON A.Sale_No=C.Sale_No
                      WHERE  {searchString}
                       GROUP BY A.Sale_No, A.ShiftDate, A.Till, A.Sale_Amt, A.Client,
                      A.T_Type, A. SD 
                   {(countOnly ? "" : " ORDER BY A.Sale_No DESC ")}"); 
        }
      }
      else
      {
        sql.AppendLine($@"SELECT
                      DISTINCT A.Store_ID AS StoreId,
                      A.Sale_No AS SaleNo,
                      CONVERT(varchar,ShiftDate,101) AS ShiftDate,
                      Till AS Till,
                      CAST(Sale_Amt AS DECIMAL (15,2)) AS Amount,
                      Client AS Customer,
                      T_Type AS Type,
                      SD AS SDT,
                      0 AS Source
                    FROM
                      CSCTrans.dbo.SaleHead AS A
                    LEFT OUTER JOIN
                      CSCTrans.dbo.SaleLine AS B
                    ON
                      A.Sale_No = B.Sale_No AND A.Store_ID = B.Store_ID
                    LEFT OUTER JOIN
                      CSCTrans.dbo.SaleTend AS C
                    ON
                      A.Sale_No = C.Sale_No AND A.Store_ID = C.Store_ID 
                    WHERE
                      {searchString}
                   {(countOnly ? "" : " ORDER BY A.Sale_No DESC ")}");
      }
    }

    if (ejSearchParam.Length != null && ejSearchParam.Length > 0 && !countOnly)
      sql.AppendLine($@" OFFSET {ejSearchParam.Start} ROWS FETCH NEXT {ejSearchParam.Length} ROWS ONLY; ");

    if (countOnly)
      sql.AppendLine(") AS CombinedData");

    return sql.ToString();
  }

  public async Task<bool> TruncateSelectedSaleNoAsync(bool? isWareHouse, char? storeRadioOption)
  {
    string sql;

    if (_config.IsBackOffice)
    {
      sql = @$"IF EXISTS (SELECT * FROM CSCTrans.dbo.sysobjects
                  WHERE id = object_id(N'[CSCTrans].[dbo].[Selected_Sale_No]') AND
                  OBJECTPROPERTY(id, N'IsUserTable') = 1)
                  TRUNCATE TABLE [CSCTrans].[dbo].[Selected_Sale_No]
                ELSE
                  CREATE TABLE [CSCTrans].[dbo].[Selected_Sale_No]
                  ([Sale_No] [int] NULL) ON [PRIMARY]";
    }
    else
    {
      if (isWareHouse == true && storeRadioOption != 'C')
      {
        // HO DB
        sql = $@"IF EXISTS(SELECT * FROM CSCTrans.dbo.sysobjects 
                WHERE id = object_id(N'[CSCTrans].[dbo].[Selected_Sale_No]') AND 
                OBJECTPROPERTY(id, N'IsUserTable') = 1) 
                TRUNCATE TABLE [CSCTrans].[dbo].[Selected_Sale_No] 
                ELSE 
                CREATE TABLE [CSCTrans].[dbo].[Selected_Sale_No] 
                  ([Store_ID] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS 
                  NOT NULL ,  [Sale_No] [int] NOT NULL) ON [PRIMARY]";
      }
      else if (storeRadioOption == 'C')
      {
        // BO DB
        sql = $@"IF EXISTS(SELECT * FROM CSCTills.dbo.sysobjects 
                WHERE id = object_id(N'[CSCTills].[dbo].[Selected_Sale_No]') AND 
                OBJECTPROPERTY(id, N'IsUserTable') = 1) 
                TRUNCATE TABLE [CSCTills].[dbo].[Selected_Sale_No] 
                ELSE 
                CREATE TABLE [CSCTills].[dbo].[Selected_Sale_No] 
                ([Sale_No] [int] NULL) ON [PRIMARY]";
      }
      else
      {
      // BO DB
        sql = $@"IF EXISTS (SELECT * FROM CSCTrans.dbo.sysobjects
                WHERE id = object_id(N'[CSCTrans].[dbo].[Selected_Sale_No]') AND
                OBJECTPROPERTY(id, N'IsUserTable') = 1)
                TRUNCATE TABLE [CSCTrans].[dbo].[Selected_Sale_No]
                ELSE
                CREATE TABLE [CSCTrans].[dbo].[Selected_Sale_No]
                ([Sale_No] [int] NULL) ON [PRIMARY]";
      }
    }

    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> TruncateSelectedSaleCardTenderAsync(bool? isWareHouse, char? storeRadioOption)
  {
    string sql;

    if (_config.IsBackOffice)
    {
      sql = @$"IF EXISTS (SELECT * FROM CSCTrans.dbo.sysobjects
                  WHERE id = object_id(N'[CSCTrans].[dbo].[Selected_Sale_CardTender]') AND
                  OBJECTPROPERTY(id, N'IsUserTable') = 1)
                  TRUNCATE TABLE [CSCTrans].[dbo].[Selected_Sale_CardTender]
                ELSE
                  CREATE TABLE [CSCTrans].[dbo].[Selected_Sale_CardTender]
                  ([Sale_No] [int] NULL, [TENDNAME] [nvarchar] (20) NULL, [Card_Number] [nvarchar] (50) NULL )";
    }
    else
    {
      if (isWareHouse == true && storeRadioOption != 'C')
      {
        // HO DB
        sql = @$"IF EXISTS (SELECT * FROM CSCTrans.dbo.sysobjects
                  WHERE id = object_id(N'[CSCTrans].[dbo].[Selected_Sale_CardTender]') AND
                  OBJECTPROPERTY(id, N'IsUserTable') = 1)
                  TRUNCATE TABLE [CSCTrans].[dbo].[Selected_Sale_CardTender]
                ELSE
                  CREATE TABLE [CSCTrans].[dbo].[Selected_Sale_CardTender]
                  ([Sale_No] [int] NULL, [TENDNAME] [nvarchar] (20) NULL,
                  [Card_Number] [nvarchar] (50) NULL,
                  [Store_ID] [nvarchar] (10)
                  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL )";
      }
      else
      {
        // BO DB
        string dbSource = storeRadioOption == 'C' ? "CSCTills" : "CSCTrans";
        sql = $@"IF EXISTS (SELECT * FROM {dbSource}.dbo.sysobjects
                WHERE id = object_id(N'[{dbSource}].[dbo].[Selected_Sale_CardTender]') AND
                OBJECTPROPERTY(id, N'IsUserTable') = 1)
                TRUNCATE TABLE [{dbSource}].[dbo].[Selected_Sale_CardTender]
                ELSE
                CREATE TABLE [{dbSource}].[dbo].[Selected_Sale_CardTender]
                ([Sale_No] [int] NULL, [TENDNAME] [nvarchar] (20) NULL, [Card_Number] [nvarchar] (50) NULL )";
      }
    }

    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> InsertSelectedSaleNoAsync(bool? isWareHouse, char? storeRadioOption, string searchString)
  {
    string sql;

    if (_config.IsBackOffice)
    {
      // BO DB
      sql = @$"INSERT INTO [CSCTrans].[dbo].[Selected_Sale_No] SELECT distinct A.Sale_No
                  FROM [CSCTrans].[dbo].[SaleHead] AS A LEFT OUTER JOIN [CSCTrans].[dbo].[SaleLine] AS B
                  ON A.Sale_No = B.Sale_No LEFT OUTER JOIN CSCTrans.dbo.SaleTend AS C ON A.Sale_No = C.Sale_No
                  WHERE {searchString}";
    }
    else
    {
      if (isWareHouse == true && storeRadioOption != 'C')
      {
        // HO DB
        sql = @$"INSERT INTO [CSCTrans].[dbo].[Selected_Sale_No] SELECT A.Store_ID, A.Sale_No
                FROM [CSCTrans].[dbo].[SaleHead] AS A LEFT OUTER JOIN [CSCTrans].[dbo].[SaleLine] AS B
                ON A.Sale_No = B.Sale_No AND A.Store_ID=B.Store_ID LEFT OUTER JOIN CSCTrans.dbo.SaleTend AS C ON A.Sale_No = C.Sale_No AND A.Store_ID=C.Store_ID
                WHERE {searchString}";
      }
      else
      {
        // BO DB
        string dbSource = storeRadioOption == 'C' ? "CSCTills" : "CSCTrans";
        sql = @$"INSERT INTO [{dbSource}].[dbo].[Selected_Sale_No] SELECT distinct A.Sale_No
                  FROM [{dbSource}].[dbo].[SaleHead] AS A LEFT OUTER JOIN [{dbSource}].[dbo].[SaleLine] AS B
                  ON A.Sale_No = B.Sale_No LEFT OUTER JOIN {dbSource}.dbo.SaleTend AS C ON A.Sale_No = C.Sale_No
                  WHERE {searchString}";
      }
    }

    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> InsertSelectedSaleCardTenderAsync(bool? isWareHouse = null, char? storeRadioOption = null, string? saleNo = null)
  {
    string sql;

    if (_config.IsBackOffice)
    {
      // BO DB
      sql = $@"DELETE FROM CSCTrans.dbo.Selected_Sale_CardTender
              INSERT INTO CSCTrans.dbo.Selected_Sale_CardTender(SALE_NO, TENDNAME, Card_Number)
              SELECT SALE_NO, TendName, CCARD_NUM
              FROM CSCTrans.dbo.SALETEND
              WHERE CCARD_NUM<> '' AND CCARD_NUM IS NOT NULL {(string.IsNullOrEmpty(saleNo) ? "": "AND SALE_NO = @SaleNo")};";
    }
    else
    {
      if (isWareHouse == true && storeRadioOption != 'C')
      {
        // HO DB
        sql = @$"DELETE FROM CSCTrans.dbo.Selected_Sale_CardTender
              INSERT INTO CSCTrans.dbo.Selected_Sale_CardTender(SALE_NO, TENDNAME, Card_Number, Store_ID)
              SELECT SALE_NO, TendName, CCARD_NUM, Store_Id
              FROM CSCTrans.dbo.SALETEND
              WHERE CCARD_NUM<> '' AND CCARD_NUM IS NOT NULL AND SALE_NO = @SaleNo";
      }
      else
      {
        // BO DB
        string dbSource = storeRadioOption == 'C' ? "CSCTills" : "CSCTrans";

        sql = $@"DELETE FROM {dbSource}.dbo.Selected_Sale_CardTender
              INSERT INTO {dbSource}.dbo.Selected_Sale_CardTender(SALE_NO, TENDNAME, Card_Number)
              SELECT SALE_NO, TendName, CCARD_NUM
              FROM {dbSource}.dbo.SALETEND
              WHERE CCARD_NUM<> '' AND CCARD_NUM IS NOT NULL AND SALE_NO = @SaleNo;";
      }
    }

    int affectedRows = await Connection.ExecuteAsync(sql, new { SaleNo = saleNo }, Transaction);
    return affectedRows > 0;
  }


  public async Task<IEnumerable<ElectronicJournalSummaryReport>?> GetElectronicJournalSummaryReportDataAsync()
  {
    IEnumerable<ElectronicJournalSummaryReport>? ejSummaryReportData = await Connection.QueryAsync<ElectronicJournalSummaryReport>("Sproc_GetElectronicJournalSummaryData", null, Transaction, commandType: CommandType.StoredProcedure);
    return ejSummaryReportData;
  }

  public async Task<EJSummarySubreportAData> GetEJSummarySubReportADataAsync()
  {
    EJSummarySubreportAData? ejSummaryReportData = await Connection.QueryFirstOrDefaultAsync<EJSummarySubreportAData?>("Sproc_GetEJSummarySubReportAData", null, Transaction, commandType: CommandType.StoredProcedure);
    return ejSummaryReportData;
  }

  public async Task<IEnumerable<EJSummarySubreportTData>?> GetEJSummarySubReportTDataAsync()
  {
    IEnumerable<EJSummarySubreportTData?> ejSummaryReportData = await Connection.QueryAsync<EJSummarySubreportTData?>("Sproc_GetEJSummarySubReportTData", null, Transaction, commandType: CommandType.StoredProcedure);
    return ejSummaryReportData;
  }

  public async Task<IEnumerable<EJDetailsReportData>?> GetEJDetailsReportDataAsync(int? pageNumber = null, int? pageSize = null)
  {
    var tasks = new List<Task<IEnumerable<EJDetailsReportData>?>>
    {
      FetchEJDetailsReportDataAsync("CSCTrans", pageNumber, pageSize) // Fetch data from CSCTrans Database
      //,FetchEJDetailsReportDataAsync("CSCTills", pageNumber, pageSize) // Fetch data from CSCTills Database
    };

    var results = await Task.WhenAll(tasks);

    var combinedResults = results.Where(result => result != null)
                                 .SelectMany(result => result)
                                 .ToList();

    return combinedResults;
  }

  public async Task<string?> GetEJTenderDetailsReportDataAsync(int saleNo)
  {
    string sql = @$"SELECT  Message FROM CSCTrans.dbo.CardTenders where Sale_No = {saleNo}";

    string? message = await Connection.QueryFirstOrDefaultAsync<string?>(sql, null, Transaction);
    return message;
  }

  private async Task<IEnumerable<EJDetailsReportData>?> FetchEJDetailsReportDataAsync(string dbSource, int? pageNumber = null, int? pageSize = null)
  {
    string sql = PrepareQueryForEJReportData(dbSource, pageNumber, pageSize);
    return await Connection.QueryAsync<EJDetailsReportData>(sql, null, Transaction);
  }

  private static string PrepareQueryForEJReportData(string dbSource, int? pageNumber = null, int? pageSize = null)
  {
    var sb = new StringBuilder();
    sb.AppendLine($@"
      SELECT
        SALEHEAD.SALE_NO AS SaleNo,
        SALEHEAD.TILL AS Till,
        SALEHEAD.[USER] AS Cashier,
        SALEHEAD.CLIENT AS Customer,
        SALEHEAD.SALE_DATE AS SaleDate,
        SALEHEAD.SALE_TIME AS SaleTime,
        SALEHEAD.ShiftDate AS ShiftDate,
        SALEHEAD.INVC_DISC AS InvoiceDiscount,
        SALEHEAD.T_TYPE AS Type,
        SALELINE.LINE_NUM AS Id,
        SALELINE.STOCK_CODE AS Code,
        SALELINE.DESCRIPT AS Description,
        SALELINE.QUANTITY AS Qty,
        SALELINE.PRICE AS Price,
        SALELINE.DISCOUNT AS Disc,
        SALELINE.AMOUNT AS Amount,
        SALELINE.DEPT,
        SALELINE.PumpID AS PumpID,
        DelLines.Del_Seq AS DelSeq,
        SH.ASSOC_AMT AS AssocAmt,
        SH.TEND_AMT AS TendAmt,
        SH.SALE_AMT AS SaleAmt,
        SH.CHANGE AS ChangeAmt,
        SH.OverPayment AS OverPaymentAmt,
        ST.TENDNAME AS TendName,
        ST.AMTTEND AS AmtTend,
        ST.AMTUSED AS AmtUsed,
        SCT.Card_Number AS CardNumber,
        SST.Tax_Name AS TaxName,
        SST.Tax_Code AS TaxCode,
        SST.Tax_Added_Amount AS TaxAddedAmount,
        SST.Tax_Included_Amount AS TaxIncludedAmount,
        CASE
          WHEN SALEHEAD.T_TYPE = 'CANCEL' OR SALEHEAD.T_TYPE = 'VOID' OR SALEHEAD.T_TYPE = 'MARKDOWN' OR SALEHEAD.T_TYPE = 'RUNAWAY'
          THEN 1
          ELSE 0
        END AS IsHidden
      FROM
          {dbSource}.dbo.SALEHEAD AS SALEHEAD
      LEFT OUTER JOIN
          {dbSource}.dbo.SALELINE AS SALELINE ON SALEHEAD.SALE_NO = SALELINE.SALE_NO
      LEFT OUTER JOIN
          {dbSource}.dbo.DelLines AS DelLines ON SALEHEAD.SALE_NO = DelLines.SALE_NO
          AND SALEHEAD.T_TYPE = 'VOID'
      LEFT OUTER JOIN
          {dbSource}.dbo.SALEHEAD AS SH ON SALEHEAD.SALE_NO = SH.SALE_NO
      LEFT OUTER JOIN
          {dbSource}.dbo.S_SaleTax AS SST ON SH.SALE_NO = SST.Sale_No
      INNER JOIN
          {dbSource}.dbo.SALETEND AS ST ON SH.SALE_NO = ST.SALE_NO
      LEFT OUTER JOIN
          {dbSource}.dbo.Selected_Sale_CardTender AS SCT ON ST.SALE_NO = SCT.Sale_No
          AND ST.TENDNAME = SCT.TENDNAME
      WHERE
          SALEHEAD.SALE_NO in (SELECT Sale_No FROM {dbSource}.dbo.Selected_Sale_No)
      ORDER BY
          SALEHEAD.SALE_NO,
          SALELINE.LINE_NUM"
    );

    if (pageNumber.HasValue && pageSize.HasValue)
    {
      int offset = (pageNumber.Value - 1) * pageSize.Value;
      sb.AppendLine($"OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY");
    }

    return sb.ToString();
  }
}
