-- All necessary database scripts for Back Office of Infonet CSCR project will be written in this file


-- ICSCR-3 - Implement login/logout feature

-- Create SP Sproc_GetUser
USE [CSCAdmin]
GO

IF OBJECT_ID('Sproc_GetUser', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE [Sproc_GetUser]
END
GO

CREATE PROCEDURE [dbo].[Sproc_GetUser]
  @Code VARCHAR(MAX)
AS
BEGIN
  SELECT U_CODE AS Code
        ,U_NAME AS Name
        ,EPW    AS Password
        ,U_ID   AS Id
        ,Type
        ,DOB    AS DateOfBirth
  FROM [USER] 
  WHERE U_CODE = @Code;
END
GO

-- Create Sproc_GetUserGroupDetails
USE [CSCAdmin]
GO

IF OBJECT_ID('Sproc_GetUserGroupDetails', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE [Sproc_GetUserGroupDetails]
END
GO

CREATE PROCEDURE [dbo].[Sproc_GetUserGroupDetails]
  @Code VARCHAR(MAX)
AS
BEGIN
  SELECT ug.UGroup        AS Code
        ,ug.UG_Name       AS Name
        ,ug.SecurityLevel 
  FROM UIG
  INNER JOIN UGroup ug 
    ON UIG.UGroup = ug.UGroup
  WHERE uig.[User] = @Code;
END
GO

-- Create RefreshToken Table if not exists
USE [CSCAdmin]

IF OBJECT_ID(N'dbo.RefreshToken', N'U') IS NULL
CREATE TABLE RefreshToken(
  Token varchar(MAX),
  ExpiryTime Datetime,
  UserCode varchar(11) not null,
);
GO

-- Create SP Sproc_HasAccessRight
USE [CSCAdmin]
GO

IF OBJECT_ID('Sproc_HasAccessRight', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE [Sproc_HasAccessRight]
END
GO

CREATE PROCEDURE [dbo].[Sproc_HasAccessRight]
   @USERNAME VARCHAR(MAX),
   @P_NAME VARCHAR(MAX)
AS
BEGIN
  IF EXISTS( SELECT TOP(1) P_NAME FROM P_COMP WHERE P_NAME = @P_NAME AND P_ACTIVE = 1)
  BEGIN
   IF EXISTS(
    SELECT TOP(1) P_NAME FROM P_SET WHERE P_NAME = @P_NAME AND P_SET = 'YES'
    AND
    ((P_LEVEL = 'USER' AND P_VALUE = @USERNAME) OR
    (P_LEVEL = 'UGROUP' AND P_VALUE = (SELECT P_VALUE FROM UIG WHERE[USER] =@USERNAME)))
   )
   BEGIN
    SELECT 'TRUE'
   END
   ELSE SELECT 'FALSE'
  END
  ELSE SELECT 'TRUE'
END
GO

-- Create SP Sproc_GetStockItemHistoryReportData
USE [CSCTrans]
GO

IF OBJECT_ID('Sproc_GetStockItemHistoryReportData', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE [Sproc_GetStockItemHistoryReportData]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Sproc_GetStockItemHistoryReportData] 
  @StockCode VARCHAR(20),
  @StartDate DATETIME,
  @EndDate DATETIME

AS
BEGIN
   --SET NOCOUNT ON added to prevent extra result sets from
   --interfering with SELECT statements.
SET NOCOUNT ON;

SELECT T.SALE_NO AS SaleNo, T.ShiftDate, T.T_Type AS TransactionType, T.STOCK_CODE AS StockCode
  , CASE 
    WHEN T.T_Type = 'SALE' OR T.T_Type = 'PATP_APP' OR T.T_TYPE = 'REFUND' 
    THEN -1 * T.QUANTITY
    ELSE T.QUANTITY
    END AS Quantity
  ,T.COST AS Cost
  , CASE 
    WHEN T.T_Type = 'SALE' OR T.T_Type = 'PATP_APP' OR T.T_TYPE = 'REFUND' 
   	THEN -1 * T.QUANTITY * T.COST
			ELSE T.QUANTITY * T.COST
			END AS TotalCost
	,DATENAME(MONTH, T.ShiftDate) + ' ' + DATENAME(YEAR, T.ShiftDate) AS MonthName
	FROM [CSCTrans].[dbo].[SR0010] AS T
	WHERE T.STOCK_CODE = @StockCode
	AND T.ShiftDate BETWEEN @StartDate AND @EndDate
	ORDER BY T.ShiftDate, T.SALE_NO

END
GO

-- Create SP SProc_StockCostInfoBatch | ICSCR-299
USE [CSCMaster]
GO

IF NOT EXISTS (
    SELECT *
    FROM sys.types
    WHERE is_user_defined = 1
    AND name = 'StockCodeTableType'
)
BEGIN
	CREATE TYPE StockCodeTableType AS TABLE (
    StockCode VARCHAR(16)
  )
END
GO

IF OBJECT_ID('SProc_StockCostInfoBatch', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [SProc_StockCostInfoBatch]
END
GO

CREATE PROCEDURE SProc_StockCostInfoBatch
@StockCodes StockCodeTableType READONLY,
@OrderNo AS INTEGER,
@VendorID as nvarchar(10)= ''
AS
BEGIN
	if len(@Vendorid) <> 0 /* we are bringing particular vendor info */
		SELECT stockcosts.stock_code stockcode, cost,Vendorid,Vendors.Name,activevendor FROM StockCosts 
		LEFT JOIN Vendors ON stockcosts.Vendorid = Vendors.Code 
		WHERE stockcosts.stock_code IN (SELECT StockCode FROM @StockCodes) and 
		stockcosts.vendorid = @VendorID
		and orderno = @OrderNo 
		ORDER BY activevendor DESC
	else /* Active vendor */
		SELECT stockcosts.stock_code stockcode, cost,Vendorid,Vendors.Name,activevendor FROM StockCosts 
		LEFT JOIN Vendors ON stockcosts.Vendorid = Vendors.Code 
		WHERE stockcosts.stock_code IN (SELECT StockCode FROM @StockCodes) and 
		(activevendor =1 or Vendorid ='ALL')
		and orderno = @OrderNo 
		ORDER BY activevendor DESC 
END
GO

-- Create SProc_StockCostInfoByMultipleVendorBatch | ICSCR-309
USE [CSCMaster]
GO

IF OBJECT_ID('SProc_StockCostInfoByMultipleVendorBatch', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE [SProc_StockCostInfoByMultipleVendorBatch]
END
GO

IF NOT EXISTS (
    SELECT *
    FROM sys.types
    WHERE is_user_defined = 1
    AND name = 'StockCodeAndVendorIdTableType'
)

BEGIN
   CREATE TYPE StockCodeAndVendorIdTableType AS TABLE (
    StockCode VARCHAR(16),
    VendorId VARCHAR(16)
  )
END
GO


CREATE PROCEDURE SProc_StockCostInfoByMultipleVendorBatch
@StockCodesAndVendorIds StockCodeAndVendorIdTableType READONLY,
@OrderNo AS INTEGER
AS
BEGIN
		SELECT sc.Stock_Code StockCode
		      ,Cost
		      ,sc.VendorId
		      ,v.Name
		      ,ActiveVendor 
		FROM StockCosts sc
		LEFT JOIN Vendors v
			ON sc.VendorId = v.Code 
		INNER JOIN @StockCodesAndVendorIds scvi
			ON  sc.Stock_Code = scvi.StockCode
			AND sc.VendorId   = scvi.VendorId
		WHERE OrderNo = @OrderNo 
		ORDER BY ActiveVendor DESC
END
GO

-- Create SP LoadDiscountChart - ICSCR-476
USE [CSCMaster]
GO

IF OBJECT_ID('Sproc_LoadDiscountChart', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Sproc_LoadDiscountChart]
END
GO

CREATE PROCEDURE Sproc_LoadDiscountChart
    @GroupID VARCHAR(255),
		@USE_ARINVOICES BIT
AS
BEGIN
    DECLARE @ResultData TABLE (
        Grade NVARCHAR(255),
        FullName NVARCHAR(255),
        DiscountRate NVARCHAR(255),
        GroupID NVARCHAR(255)
    )

    BEGIN
        INSERT INTO @ResultData (Grade, FullName, DiscountRate, GroupID)
        SELECT Grade, FullName, DiscountRate, GroupID
        FROM CSCPump.dbo.Grade AS Grade
        LEFT JOIN CSCMaster.dbo.FuelDiscountChart ON CSCMaster.dbo.FuelDiscountChart.Grade = Grade.ID
        WHERE GroupID = @GroupID
        ORDER BY Grade;

        IF NOT EXISTS (SELECT 1 FROM @ResultData)
        BEGIN
            INSERT INTO @ResultData (Grade, FullName, DiscountRate, GroupID)
            SELECT ID AS Grade, FullName, '' AS DiscountRate, '' AS GroupID
            FROM CSCPump.dbo.Grade AS Grade
            ORDER BY Grade;
        END
    END

    -- Return the result set
    SELECT *
    FROM @ResultData;
END
GO

-- Create SP UpdateOrInsertDiscountChart - ICSCR-476
USE [CSCMaster]
GO

IF OBJECT_ID('Sproc_UpdateOrInsertDiscountChart', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Sproc_UpdateOrInsertDiscountChart]
END
GO

CREATE PROCEDURE [Sproc_UpdateOrInsertDiscountChart]
    @GroupID VARCHAR(255),
    @Grade VARCHAR(255),
    @DiscountRate VARCHAR(255)
AS
BEGIN
    MERGE INTO [CSCMaster].[dbo].[FuelDiscountChart] AS target
    USING (
        SELECT @GroupID AS GroupID, @Grade AS Grade, @DiscountRate AS DiscountRate
    ) AS source
    ON target.GroupID = source.GroupID AND target.Grade = source.Grade
    WHEN MATCHED THEN
        UPDATE SET
            target.DiscountRate = source.DiscountRate
    WHEN NOT MATCHED THEN
        INSERT (GroupID, Grade, DiscountRate)
        VALUES (source.GroupID, source.Grade, source.DiscountRate);
END
GO

-- Create SP RemoveUserGroup - ICSCR-487
USE [CSCAdmin]
GO

IF OBJECT_ID('Sproc_RemoveUserGroup', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Sproc_RemoveUserGroup]
END
GO

CREATE PROCEDURE [Sproc_RemoveUserGroup]
    @Code NVARCHAR(30)
AS
BEGIN
    -- Delete from UGROUP table
    DELETE FROM [CSCAdmin].[dbo].[UGROUP]
    WHERE [UGROUP].[UGROUP] = @Code;

    -- Delete from UIG table
    DELETE FROM [CSCAdmin].[dbo].[UIG]
    WHERE [UIG].[UGROUP] = @Code;

    -- Delete from P_SET table
    DELETE FROM [CSCAdmin].[dbo].[P_SET]
    WHERE [P_SET].[P_LEVEL] = 'UGROUP'
    AND [P_SET].[P_VALUE] = @Code;
END
GO

-- Create SP GetPoliciesByClass - ICSCR-608
USE [CSCMaster]
GO

IF OBJECT_ID('Sproc_GetPoliciesByClass', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Sproc_GetPoliciesByClass]
END
GO

CREATE PROCEDURE [Sproc_GetPoliciesByClass]
    @ClassName NVARCHAR(20)
AS
BEGIN
  SELECT
    PCMP.P_Name    AS [Name],
    PCMP.P_Desc    AS [Description],
    PCMP.P_LEVELS AS [Levels],
    PCMP.P_Choices AS [Choices],
    PCMP.P_Set     AS [Settings],
    PCMP.Implemented,
    PCMP.P_Class   AS [Class],
    CASE 
      WHEN COUNT(CASE WHEN PCNB.P_Name IS NOT NULL THEN 1 END) > 0 THEN CAST(1 AS BIT)
      ELSE CAST(0 AS BIT)
    END AS [MultipleLevelExists]
  FROM [CSCAdmin].[dbo].P_Comp AS PCMP
  LEFT JOIN [CSCAdmin].[dbo].P_Canbe AS PCNB
    ON PCMP.P_Name = PCNB.P_Name AND PCNB.P_Canbe <> 'COMPANY'
  WHERE
    PCMP.P_Class = @ClassName AND 
    PCMP.Implemented = 1
  GROUP BY
    PCMP.P_Name,
    PCMP.P_Desc,
    PCMP.P_LEVELS,
    PCMP.P_Choices,
    PCMP.P_Set,
    PCMP.Implemented,
    PCMP.P_Class,
    PCMP.P_SEQ
  ORDER BY PCMP.P_Seq;
END
GO

-- Create SP Sproc_GetElectronicJournalSummaryData - ICSCR-718

USE [CSCTrans]
GO

IF OBJECT_ID('Sproc_GetElectronicJournalSummaryData', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE Sproc_GetElectronicJournalSummaryData
END
GO

CREATE PROCEDURE [dbo].Sproc_GetElectronicJournalSummaryData
AS
BEGIN
    WITH CombinedSales AS (
    SELECT 
        B.DEPT, 
        Dept.DEPT_NAME,
        B.DISCOUNT,
        B."DISC_ADJ",
        B."AMOUNT",
		A.SALE_NO
    FROM 
        CSCTrans.dbo.SALEHead A
    INNER JOIN 
        CSCTrans.dbo.SALELine AS B ON A.Sale_no = B.Sale_no
    LEFT OUTER JOIN 
        CSCMaster.dbo.Dept Dept ON B."DEPT" = Dept."DEPT"
    WHERE 
        A.T_TYPE IN ('SALE', 'REFUND', 'PATP_APP')
    
    UNION
    
    SELECT 
        B."DEPT", 
        Dept."DEPT_NAME",
        B."DISCOUNT",
        B."DISC_ADJ",
        B."AMOUNT",
		A.SALE_NO
    FROM 
        CSCTills.dbo.SALEHead A 
    INNER JOIN 
        CSCTills.dbo.SALELine AS B ON A.Sale_no = B.Sale_no
    LEFT OUTER JOIN 
        CSCMaster.dbo.Dept Dept ON Dept.Dept = B."DEPT" 
    WHERE 
        A.T_TYPE IN ('SALE', 'REFUND', 'PATP_APP')
    )

    SELECT 
        DEPT AS Department, 
        DEPT_NAME AS Description,
        SUM(DISCOUNT) AS Total_Discount,
        SUM(DISC_ADJ) AS Total_Disc_Adj,
        SUM(AMOUNT) AS Total_Amount,
        ROUND(SUM(AMOUNT) - SUM(DISCOUNT)- SUM(DISC_ADJ), 2) AS Sales
	    FROM 
        CombinedSales
		INNER JOIN CSCTrans.dbo.Selected_Sale_No C ON CombinedSales.Sale_NO = C.Sale_No
    GROUP BY 
        DEPT, DEPT_NAME
    ORDER BY 
        DEPT ASC;

END
GO

-- Create SP Sproc_GetEJSummarySubReportAData - ICSCR-718

USE [CSCTrans]
GO

IF OBJECT_ID('Sproc_GetEJSummarySubReportAData', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE Sproc_GetEJSummarySubReportAData
END
GO

CREATE PROCEDURE [dbo].Sproc_GetEJSummarySubReportAData
AS
BEGIN
    SELECT
      SUM(D.Tax_Added_Amount) AS TaxAddedAmount,
      SUM(A.ASSOC_AMT) AS AssocAmount,
      SUM(B.DISCOUNT) AS Discount,
      SUM(B.DISC_ADJ) AS DiscAdj,
      SUM(B.AMOUNT) AS Amount
    FROM CSCTrans.dbo.SALEHead A
      INNER JOIN CSCTrans.dbo.SALELine AS B ON A.Sale_no = B.Sale_no
      LEFT OUTER JOIN CSCTrans.dbo.S_SaleTax AS D ON A.Sale_No=D.Sale_No
    WHERE ((A.T_TYPE='SALE' OR A.T_TYPE='REFUND' OR A.T_TYPE='PATP_APP')
      AND A.Sale_NO IN (SELECT Sale_No FROM CSCTrans.dbo.Selected_Sale_No))
END
GO

-- Create SP Sproc_GetEJSummarySubReportTData - ICSCR-718

USE [CSCTrans]
GO

IF OBJECT_ID('Sproc_GetEJSummarySubReportTData', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE Sproc_GetEJSummarySubReportTData
END
GO

CREATE PROCEDURE [dbo].Sproc_GetEJSummarySubReportTData
AS
BEGIN
    SELECT C.TENDNAME AS TendName,
           SUM(A.CHANGE) AS TotalChange,
           SUM(C.AMTUSED) AS TotalAmtUsed
    FROM CSCTrans.dbo.SALEHead AS A
    LEFT OUTER JOIN CSCTrans.dbo.SALETEND AS C ON A.Sale_No = C.Sale_No
    WHERE A.T_TYPE IN ('SALE', 'REFUND', 'PATP_APP') 
    AND A.Sale_No IN (SELECT Sale_No FROM CSCTrans.dbo.Selected_Sale_No)
    GROUP BY C.TENDNAME
    UNION ALL
    SELECT C.TENDNAME,
           SUM(A.CHANGE) AS Total_Change,
           SUM(C.AMTUSED) AS Total_AmtUsed
    FROM CSCTills.dbo.SALEHead AS A
    LEFT OUTER JOIN CSCTills.dbo.SALETEND AS C ON A.Sale_No = C.Sale_No
    WHERE A.T_TYPE IN ('SALE', 'REFUND', 'PATP_APP') 
    AND A.Sale_No IN (SELECT Sale_No FROM CSCTrans.dbo.Selected_Sale_No)
    GROUP BY C.TENDNAME;
END
GO

-- Update policy choices format - ICSCR-722

USE [CSCAdmin];
GO

UPDATE [CSCAdmin].[dbo].[P_COMP]
SET [P_CHOICES] = (SELECT [P_CHOICES] FROM [CSCAdmin].[dbo].[P_COMP] WHERE [P_NAME] = 'REWARDS_TpsIp')
WHERE[P_NAME] = 'GiveX_IP';

UPDATE [CSCAdmin].[dbo].[P_COMP]
SET [P_CHOICES] = (SELECT [P_CHOICES] FROM [CSCAdmin].[dbo].[P_COMP] WHERE [P_NAME] = 'GiveXMerchID')
WHERE[P_NAME] IN ('GiveX_Pass', 'GiveX_User');
GO

-- Create SP SProc_StockPriceInfoBatch | ICSCR-666
USE [CSCMaster]
GO

IF NOT EXISTS (
    SELECT *
    FROM sys.types
    WHERE is_user_defined = 1
    AND name = 'StockPriceTableType'
)
BEGIN
	CREATE TYPE StockPriceTableType AS TABLE (
    StockCodeTemp VARCHAR(16),
	  VendorIdTemp VARCHAR(10)
  )
END
GO

IF OBJECT_ID('SProc_StockPriceInfoBatch', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [SProc_StockPriceInfoBatch]
END
GO

CREATE PROCEDURE [dbo].[SProc_StockPriceInfoBatch]
@StockPriceTemp  as StockPriceTableType READONLY,
@PriceNum as integer,
@OrderNo as integer
AS
BEGIN
  IF @PriceNum <> 0
    SELECT Stock_Code AS StockCode, Price, Price_Number AS PriceNumber, VendorID, OrderNo, MarginForOrderNo0 Margin
    FROM Stock_prices
   INNER JOIN @StockPriceTemp 
    ON Stock_Code = StockCodeTemp 
    AND VendorID = VendorIDTemp
  --We need to fetch margin data using order no = 0 according to VB code
  INNER JOIN (
   SELECT 
    Stock_Code AS StockCodeForOrderNo0,
    Price_Number PriceNumberForOrderNo0,
    VendorID VendorIDForOrderNo0,
    Margin MarginForOrderNo0
   FROM Stock_prices
   WHERE OrderNo = 0
   AND price_number = @PriceNum
  ) StockPricesForOrderNo0 ON (StockPricesForOrderNo0.StockCodeForOrderNo0 = StockCodeTemp AND StockPricesForOrderNo0.VendorIDForOrderNo0 = VendorIDTemp )
   WHERE price_number = @PriceNum
    AND orderno = @OrderNo
  ELSE
    SELECT Stock_Code AS StockCode, Price, Price_Number AS PriceNumber, VendorID, OrderNo, MarginForOrderNo0 Margin
    FROM Stock_prices
   INNER JOIN @StockPriceTemp
    ON Stock_Code = StockCodeTemp
    AND VendorID = VendorIDTemp
  --We need to fetch margin data using order no = 0 according to VB code
  INNER JOIN (
   SELECT 
    Stock_Code AS StockCodeForOrderNo0,
    Price_Number PriceNumberForOrderNo0,
    VendorID VendorIDForOrderNo0,
    Margin MarginForOrderNo0
   FROM Stock_prices
   WHERE OrderNo = 0
  ) StockPricesForOrderNo0 ON (StockPricesForOrderNo0.StockCodeForOrderNo0 = StockCodeTemp AND StockPricesForOrderNo0.VendorIDForOrderNo0 = VendorIDTemp )
   WHERE orderno = @OrderNo
END
GO

-- Create SProc SProc_PerformPriceChangeTransferAll | ICSCR-667
USE CSCTransfer;
GO
IF OBJECT_ID('SProc_PerformPriceChangeTransferAll', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE SProc_PerformPriceChangeTransferAll
END
GO
CREATE PROCEDURE [dbo].[SProc_PerformPriceChangeTransferAll] 
@UpdateForVendorAll SMALLINT,
@UserCode VARCHAR,
@NumberOfPrice INT,
@StoreId AS VARCHAR

AS
BEGIN
    --::::::::::::::: Regular Price --- START :::::::::::::::
    --Create or Update Stock Price
    MERGE INTO [CSCMaster].[dbo].stock_prices AS target
    using (SELECT
            t.BatchId, 
            t.ProductId, 
            t.Price_Number, 
            t.VendorId, 
            t.Price, 
            t.orderno, 
            t.margin, 
            t.pr_from, 
            t.pr_to
            FROM 
            [CSCTransfer].[dbo].pricedefinition t 
            INNER JOIN (
              SELECT 
                MAX(BatchId) AS MaxBatchId, 
                ProductId, 
                Price_Number, 
                VendorId 
              FROM 
                [CSCTransfer].[dbo].pricedefinition 
              WHERE 
                trans_flag = 'O' 
                AND pr_type = 'R' 
              GROUP BY 
                ProductId, 
                Price_Number, 
                VendorId
            ) sub ON t.BatchId = sub.MaxBatchId 
            AND t.ProductId = sub.ProductId 
            AND t.Price_Number = sub.Price_Number 
            AND t.VendorId = sub.VendorId 
          WHERE 
            t.trans_flag = 'O' 
            AND t.pr_type = 'R') AS source
    ON ( target.stock_code = source.productid
        AND target.price_number = source.price_number
        AND target.orderno = source.orderno
        AND target.vendorid = source.vendorid )
    WHEN matched THEN
    UPDATE SET target.price = source.price,
                target.margin = source.margin,
                target.startdate = CASE
                                    WHEN source.orderno <> 0 THEN
                                    source.pr_from
                                    ELSE NULL END,
                target.enddate = CASE
                                    WHEN source.orderno <> 0 THEN source.pr_to
                                    ELSE NULL END

    WHEN NOT matched THEN
    INSERT (stock_code,
            price_number,
            price,
            vendorid,
            startdate,
            enddate,
            orderno,
            margin)
    VALUES (source.productid,
            source.price_number,
            source.price,
            source.vendorid,
            CASE
                WHEN source.orderno <> 0 THEN source.pr_from
                ELSE NULL
            END,
            CASE
                WHEN source.orderno <> 0 THEN source.pr_to
                ELSE NULL
            END,
            source.orderno,
            source.margin);

    IF @UpdateForVendorAll > 0
    BEGIN
        MERGE INTO [CSCMaster].[dbo].stock_prices AS target
        using (SELECT
                t.BatchId, 
                t.ProductId, 
                t.Price_Number, 
                t.VendorId, 
                t.Price, 
                t.orderno, 
                t.margin, 
                t.pr_from, 
                t.pr_to
                FROM 
                [CSCTransfer].[dbo].pricedefinition t 
                INNER JOIN (
                  SELECT 
                    MAX(BatchId) AS MaxBatchId, 
                    ProductId, 
                    Price_Number, 
                    VendorId 
                  FROM 
                    [CSCTransfer].[dbo].pricedefinition 
                  WHERE 
                    trans_flag = 'O' 
                    AND pr_type = 'R' 
                  GROUP BY 
                    ProductId, 
                    Price_Number, 
                    VendorId
                ) sub ON t.BatchId = sub.MaxBatchId 
                AND t.ProductId = sub.ProductId 
                AND t.Price_Number = sub.Price_Number 
                AND t.VendorId = sub.VendorId 
              WHERE 
                t.trans_flag = 'O' 
                AND t.pr_type = 'R') AS source
        ON ( target.stock_code = source.productid
                AND target.price_number = source.price_number
                AND target.orderno = source.orderno
                AND target.vendorid = 'ALL' )
        WHEN matched THEN
            UPDATE SET target.price = source.price,
                        target.margin = source.margin,
                        target.startdate = CASE
                                            WHEN source.orderno <> 0 THEN
                                            source.pr_from
                                            ELSE NULL
                                        END,
                        target.enddate = CASE
                                        WHEN source.orderno <> 0 THEN
                                        source.pr_to
                                        ELSE NULL
                                        END
        WHEN NOT matched THEN
            INSERT (stock_code,
                    price_number,
                    price,
                    vendorid,
                    startdate,
                    enddate,
                    orderno,
                    margin)
            VALUES (source.productid,
                    source.price_number,
                    source.price,
                    'ALL',
                    CASE
                    WHEN source.orderno <> 0 THEN source.pr_from
                    ELSE NULL
                    END,
                    CASE
                    WHEN source.orderno <> 0 THEN source.pr_to
                    ELSE NULL
                    END,
                    source.orderno,
                    source.margin);
    END

    --::::::::::::::: Regular Price --- END :::::::::::::::


    --::::::::::::::: Create Cost Records | Common for all price types --- START :::::::::::::::
    DECLARE @BatchId INT;
    SET @BatchId = [CSCTransfer].dbo.GetNewBatchId('cost-change') - 1;

    --Add Cost Change Transfer
    INSERT INTO [CSCTransfer].[dbo].[costchangetransfer]
                (batchid,
                productid,
                description,
                startdate,
                enddate,
                newcost,
                store_id,
                vendorid,
                orderno,
                trans_date,
                trans_user,
                trans_flag,
                activevendor)
    SELECT ( @BatchId
            + Row_number() OVER (ORDER BY productid) ),
            productid,
            descript,
            pr_from,
            pr_to,
            cost,
            @StoreId,
            vendor_id,
            orderno,
            Getdate(),
            @UserCode,
            'N',
            activevendor
    FROM (
      SELECT 
  s.*, 
  ROW_NUMBER() OVER (
    PARTITION BY s.productId, 
    s.vendor_Id,
    s.OrderNo 
    ORDER BY 
    s.OrderNo
  ) AS RowNum 
      FROM 
  [CSCTransfer].[dbo].[vw_StockCostsOfAllPriceDef] s
    ) AS subquery
    WHERE RowNum = 1;

    --Add Stock Cost
    CREATE TABLE #TempStockCosts (
                temp_productid varchar(16),  
                temp_descript varchar(35),
                temp_vendorid varchar(10),
                temp_cost money,
                temp_prfrom datetime,
                temp_prto datetime,
                temp_orderno smallint,
                temp_activevendor int,
                );
    INSERT INTO #TempStockCosts (temp_productid, temp_descript, temp_vendorid, temp_cost, temp_prfrom, temp_prto, temp_orderno, temp_activevendor)
    SELECT ProductId,
                descript,
                vendor_id,
                Cost,
                pr_from,
                pr_to,
                OrderNo,
                activevendor
        FROM (
          SELECT 
      s.*, 
      ROW_NUMBER() OVER (
        PARTITION BY s.productId, 
        s.vendor_Id,
        s.OrderNo 
        ORDER BY 
        s.OrderNo
      ) AS RowNum 
          FROM 
      [CSCTransfer].[dbo].[vw_StockCostsOfAllPriceDef] s
        ) AS subquery
        WHERE RowNum = 1;

    INSERT INTO [CSCMaster].[dbo].[stockcosts]
                (stock_code,
                vendorid,
                cost,
                startdate,
                enddate,
                orderno,
                activevendor)
    SELECT temp_productid, 
                temp_vendorid, 
                temp_cost, 
                temp_prfrom, 
                temp_prto, 
                temp_orderno, 
                temp_activevendor 
    FROM #TempStockCosts 
    DROP TABLE #TempStockCosts;

    --::::::::::::::: Create Cost Records | Common for all price types --- END :::::::::::::::


    --::::::::::::::: Special Pricing --- START :::::::::::::::
    --Delete Special Prices
    DELETE p
    FROM   [CSCMaster].[dbo].[pricel] p
            LEFT JOIN [CSCTransfer].[dbo].pricedefinition pd
                ON p.stock_code = pd.productid
                    AND p.orderno = pd.orderno
                    AND p.vendorid = pd.vendorid
    WHERE  pd.trans_flag = 'O'
            AND pd.pr_type != 'R'

  CREATE TABLE #EntriesWithNoPrice (
  temp_productId VARCHAR(16), 
  temp_vendorId VARCHAR(16), 
  temp_orderNo SMALLINT
  );
  INSERT INTO #EntriesWithNoPrice (temp_productId, temp_vendorId, temp_orderNo)
  SELECT 
    pd.ProductId, 
    pd.VendorId, 
    pd.OrderNo 
  FROM 
    [CSCTransfer].[dbo].PriceDefinition pd 
    LEFT JOIN CSCMaster.[dbo].Stock_prices sp ON sp.OrderNo = pd.OrderNo 
    and sp.stock_code = pd.ProductId 
    and sp.vendorid = pd.VendorId 
  WHERE 
    pd.Trans_Flag = 'O' 
    AND PR_Type != 'R' 
    AND sp.Stock_Code IS NULL 
  GROUP BY 
    pd.ProductId, 
    pd.VendorId, 
    pd.OrderNo

    --Create Price Record
    --Add Stock Price
    INSERT INTO [CSCMaster].[dbo].stock_prices
                (stock_code,
                price_number,
                price,
                margin,
                vendorid,
                startdate,
                enddate,
                orderno)
    SELECT sp.stock_code,
            sp.price_number,
            sp.price,
            sp.margin,
            pd.vendorid,
            pd.pr_from,
            pd.pr_to,
            pd.orderno
    FROM   [CSCTransfer].[dbo].pricedefinition pd
            LEFT JOIN cscmaster.[dbo].stock_prices sp
                ON sp.orderno = 0
                    AND sp.stock_code = pd.productid
                    AND sp.vendorid = pd.vendorid
            LEFT JOIN cscmaster.[dbo].stock_prices spc
                ON spc.stock_code = pd.productid
                    AND spc.orderno = pd.orderno
                    AND spc.vendorid = pd.vendorid
    WHERE  pd.trans_flag = 'O'
            AND pd.pr_type != 'R'
            AND spc.stock_code IS NULL
            AND sp.stock_code IS NOT NULL;

    --Add Stock Price
    INSERT INTO [CSCMaster].[dbo].stock_prices
            (stock_code,
             price_number,
             price,
             margin,
             vendorid,
             startdate,
             enddate,
             orderno)
    SELECT spv.productid,
           nums.Price_Number,
           0,
           0,
           COALESCE(spv.vendorid, 'ALL'),
           NULL,
           NULL,
           spv.orderno
    FROM   [CSCTransfer].[dbo].[vw_StockPricesOfAllPriceDef] spv
    CROSS APPLY (
           SELECT TOP (@NumberOfPrice) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Price_Number
           FROM sys.columns
    ) AS nums

    -- Insert the results into CSCTrans.dbo.PriceChange

    -- The following function is using to track the price change
    -- Processtype PC--> Price check;
    -- SL--> Saleline;
    -- SC--> Stock Screen Cost Change(Price will change) +  Stock Screen Price Change;
    -- VC- Stock Screen Switch Active Vendor;
    -- PR--> Product Category-Change Price;
    -- FP- FuturePrice rollover;
    -- PB- Price book management( Cost Change(Price will change) +   Price Change);
    -- DT- dTS transfer
    -- Nicolette, March 23, 2009 added VendorID; need it only for StockPrice change or DTS transfer

    INSERT INTO CSCTrans.dbo.PriceChange
    (
        STOCK_CODE,
        EmployeeID,
        PriceNum,
        OriginalPrice,
        NewPrice,
        Date,
        Time,
        Process,
        VendorID
    )
    SELECT
        pd.ProductId AS STOCK_CODE,
        @UserCode AS EmployeeID,
        pd.Price_Number AS PriceNum,
        ROUND(sp.price, 2) AS OriginalPrice,
        ROUND(pd.price, 2) AS NewPrice,
        FORMAT(DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0), 'yyyy-MM-dd HH:mm:ss') AS Date,
        FORMAT(CONVERT(DATETIME, CONVERT(VARCHAR(19), GETDATE(), 120)), 'yyyy-MM-dd HH:mm:ss') AS Time,
        'PB' AS Process,
        pd.VendorID AS VendorID

    FROM CSCTransfer.dbo.PriceDefinition pd
        LEFT JOIN CSCMaster.dbo.Stock_Prices sp 
            ON pd.ProductId = sp.Stock_Code 
            AND pd.VendorID = sp.VendorID
            AND pd.orderno = sp.orderno 
            AND pd.Price_Number = sp.Price_Number
            AND pd.Price != sp.Price
        WHERE pd.OrderNo = 0
            AND pd.Pr_Type = 'R'
            AND pd.Price_Number = 1;

    --Create Price Change Record
    DECLARE @PriceBatchId INT;
    SET @PriceBatchId = [CSCTransfer].dbo.GetNewBatchId('price-change') - 1;

    --Add Price Change Transfer
    INSERT INTO [CSCTransfer].[dbo].[pricechangetransfer]
                (batchid,
                productid,
                description,
                pricetype,
                store_id,
                pricegroup_id,
                region_id,
                vendorid,
                orderno,
                trans_date,
                trans_user,
                trans_flag)
    SELECT (@PriceBatchId + Row_number() OVER (ORDER BY productid)),
            pcr.productid,
            pcr.descript,
            'R',
            @StoreId,
            NULL,
            NULL,
            COALESCE(pcr.vendorid, 'ALL'),
            pcr.orderno,
            Getdate(),
            @UserCode,
            'N'
  FROM #EntriesWithNoPrice t1
  JOIN
  (
    SELECT
      productId,
    vendorId,
    orderNo,
    Price_Number,
    Descript,
      ROW_NUMBER() OVER (PARTITION BY productId, vendorId, orderNo ORDER BY orderNo) AS rn
    FROM
      [CSCTransfer].[dbo].[vw_PriceChangeRecords]
  ) pcr ON t1.temp_productId = pcr.ProductId  
  AND t1.temp_vendorId = pcr.VendorID 
  AND t1.temp_orderNo = pcr.OrderNo
  AND pcr.Price_Number = 1
  WHERE
  pcr.rn = 1;

    --Add Price Def
    INSERT INTO [CSCTransfer].[dbo].[pricedefinition]
                (batchid,
                productid,
                pr_type,
                store_id,
                pr_units,
                price_number,
                price,
                margin,
                pr_from_qty,
                pr_to_qty,
                pr_from,
                pr_to,
                orderno,
                vendorid,
                trans_date,
                trans_user,
                trans_flag)
    SELECT (@PriceBatchId + Row_number() OVER (ORDER BY productid)),
            pcr.productid,
            'R',
             @StoreId,
            NULL,
            pcr.price_number,
            pcr.price,
            pcr.margin,
            0,
            0,
            pcr.startdate,
            pcr.enddate,
            pcr.orderno,
            pcr.vendorid,
            Getdate(),
            @UserCode,
            'N'
    FROM #EntriesWithNoPrice t1
  JOIN
  (
  SELECT   productid,
    vendorid,
    orderno,
    price_number,
    descript,
    price,
    margin,
    startdate,
    enddate,
    row_number() over (partition BY productid, vendorid, orderno ORDER BY price_number DESC) AS rn
  FROM     [CSCTransfer].[DBO].[vw_PriceChangeRecords] ) pcr ON t1.temp_productid = pcr.productid
  AND t1.temp_vendorid = pcr.vendorid
  AND t1.temp_orderno = pcr.orderno
  WHERE
  pcr.rn = 1;

  DROP TABLE #EntriesWithNoPrice;

    --AddSpecialPrice
    INSERT INTO [CSCMaster].[dbo].[pricel]
                ([stock_code],
                [pr_f_qty],
                [pr_t_qty],
                [price],
                [pr_type],
                [pr_units],
                [vendorid],
                [startdate],
                [enddate],
                [orderno])
    SELECT productid,
            pr_from_qty,
            pr_to_qty,
            price,
            pr_type,
            pr_units,
            vendorid,
            pr_from,
            pr_to,
            orderno
    FROM   (SELECT
      *,
      ROW_NUMBER() OVER (PARTITION BY productId, orderno, vendorid ORDER BY batchid desc) AS rn
    FROM
      [CSCTransfer].[dbo].[pricedefinition]
  where trans_flag = 'O'
            AND pr_type != 'R') pd
    WHERE  pd.rn = 1;

    --::::::::::::::: Special Pricing --- END :::::::::::::::

    --Update Stock Date
    UPDATE sm
    SET    sm.updatedate = Getdate()
    FROM   [CSCMaster].[dbo].stockmst sm
            INNER JOIN [CSCTransfer].[dbo].[pricedefinition] pd
                    ON sm.stock_code = pd.productid
    WHERE  pd.trans_flag = 'O'
            AND pd.pr_type != 'R'

    --Perform Transfer
    EXEC [CSCTransfer].[dbo].SProc_PerformPriceChangeTransfer;
END 
GO


-- Create SP SProc_PerformPriceChangeTransfer | ICSCR-667 Price Change List Tab
USE [CSCTransfer]
GO

IF OBJECT_ID('SProc_PerformPriceChangeTransfer', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [SProc_PerformPriceChangeTransfer]
END
GO

CREATE PROCEDURE SProc_PerformPriceChangeTransfer
AS
BEGIN
    INSERT INTO PriceChangeTransferHist
    SELECT  A.*
    FROM  PriceChangeTransfer A
    WHERE A.Trans_Flag = 'O';

    UPDATE PriceChangeTransferHist
    SET Trans_Flag = 'Y'
    WHERE Trans_Flag = 'O';

    DELETE FROM  PriceChangeTransfer
    WHERE Trans_Flag = 'O';

    INSERT INTO PriceDefinitionHist
    SELECT  A.*
    FROM  PriceDefinition A
    WHERE A.Trans_Flag = 'O';
    
    UPDATE PriceDefinitionHist
    SET Trans_Flag = 'Y'
    WHERE Trans_Flag = 'O';

    DELETE FROM PriceDefinition
    WHERE Trans_Flag = 'O';
END
GO

-- Create Price Change Transfer View2 | ICSCR-667
USE CSCTransfer;
GO
IF OBJECT_ID('dbo.vw_StockPricesOfAllPriceDef', 'V') IS NOT NULL
  DROP VIEW dbo.vw_StockPricesOfAllPriceDef;
GO
CREATE VIEW [dbo].[vw_StockPricesOfAllPriceDef] 
AS 
SELECT 
  pd.ProductId, 
  pd.VendorId, 
  pd.OrderNo 
FROM 
  [CSCTransfer].[dbo].PriceDefinition pd 
  LEFT JOIN CSCMaster.[dbo].Stock_prices sp ON (
    sp.orderno = 0 
    or sp.OrderNo = pd.OrderNo
  ) 
  and sp.stock_code = pd.ProductId 
  and sp.vendorid = pd.VendorId 
WHERE 
  pd.Trans_Flag = 'O' 
  AND pd.PR_Type != 'R' 
  AND sp.Stock_Code IS NULL
GROUP BY pd.ProductId, 
  pd.VendorId, 
  pd.OrderNo 
GO

-- Create Price Change Transfer View1 | ICSCR-667
USE CSCTransfer;
GO
IF OBJECT_ID('dbo.vw_StockCostsOfAllPriceDef', 'V') IS NOT NULL
  DROP VIEW dbo.vw_StockCostsOfAllPriceDef;
GO
CREATE VIEW [dbo].[vw_StockCostsOfAllPriceDef] 
AS 
SELECT 
  pd.PR_Type, 
  sm.Descript, 
  ProductId, 
  pd.VendorId as vendor_id,
  VendorId = COALESCE(
    s.VendorId, sAll.VendorId, sNetwork.VendorId, 
    'ALL'
  ), 
  Cost = COALESCE(
    s.Cost, sAll.Cost, sNetwork.Cost, 
    0
  ), 
  PR_FROM = CASE WHEN pd.OrderNo = 0 THEN NULL ELSE pd.PR_FROM END, 
  PR_TO = CASE WHEN pd.OrderNo = 0 THEN NULL ELSE pd.PR_TO END, 
  pd.OrderNo, 
  ActiveVendor = CASE WHEN COALESCE(
    s.VendorId, sAll.VendorId, sNetwork.VendorId, 
    'ALL'
  ) = pd.VendorId THEN 1 ELSE 0 END 
FROM 
  [CSCTransfer].[dbo].PriceDefinition AS pd 
  LEFT JOIN (
    SELECT 
      s.*, 
      ROW_NUMBER() OVER (
        PARTITION BY s.Stock_Code, 
        s.OrderNo 
        ORDER BY 
          s.OrderNo
      ) AS RowNum 
    FROM 
      [CSCMaster].[dbo].[StockCosts] s 
    WHERE 
      (
        s.activevendor = 1 
        or s.Vendorid = 'ALL'
      )
  ) AS s ON pd.ProductId = s.Stock_Code 
  AND pd.OrderNo = s.OrderNo 
  AND s.RowNum = 1 
  LEFT JOIN (
    SELECT 
      sAll.*, 
      ROW_NUMBER() OVER (
        PARTITION BY sAll.Stock_Code, 
        sAll.OrderNo 
        ORDER BY 
          sAll.OrderNo
      ) AS RowNum 
    FROM 
      [CSCMaster].[dbo].[StockCosts] sAll 
    WHERE 
    (
        sAll.activevendor = 1 
        or sAll.Vendorid = 'ALL'
      )
  ) AS sAll ON sAll.Stock_Code = pd.ProductId 
  AND sAll.OrderNo = pd.OrderNo 
  AND sAll.RowNum = 1 
  LEFT JOIN (
    SELECT 
      sNetwork.*, 
      ROW_NUMBER() OVER (
        PARTITION BY sNetwork.Stock_Code
        ORDER BY 
          sNetwork.OrderNo
      ) AS RowNum 
    FROM 
      [CSCMaster].[dbo].[StockCosts] sNetwork 
    WHERE 
      sNetwork.OrderNo = 0 
      AND (
        sNetwork.activevendor = 1 
        or sNetwork.Vendorid = 'ALL'
      )
  ) AS sNetwork ON sNetwork.Stock_Code = pd.ProductId 
  AND sNetwork.OrderNo = 0 
  AND sNetwork.RowNum = 1 
  LEFT JOIN [CSCMaster].[dbo].[StockCosts] sc ON pd.ProductId = sc.Stock_Code 
  AND pd.VendorId = sc.VendorID 
  AND pd.OrderNo = sc.OrderNo 
  LEFT JOIN [CSCMaster].[dbo].[Stockmst] sm ON sm.Stock_code = pd.ProductId 
WHERE 
  pd.Trans_Flag = 'O' 
  AND sc.Stock_Code IS NULL;
GO

-- Create Price Change Transfer View3 | ICSCR-667
USE CSCTransfer;
GO
IF OBJECT_ID('dbo.vw_PriceChangeRecords', 'V') IS NOT NULL
  DROP VIEW dbo.vw_PriceChangeRecords;
GO
CREATE VIEW [dbo].[vw_PriceChangeRecords] 
AS 
SELECT 
  BatchId = ROW_NUMBER() OVER (
    ORDER BY 
      ProductId
  ), 
  sm.DESCRIPT,
  pd.ProductId, 
  sp.OrderNo, 
  sp.VendorID, 
  sp.Price_Number, 
  sp.Price, 
  sp.Margin, 
  sp.StartDate, 
  sp.EndDate 
FROM 
  [CSCTransfer].[dbo].PriceDefinition pd 
  LEFT JOIN CSCMaster.[dbo].Stock_prices sp ON sp.OrderNo = pd.OrderNo 
  and sp.stock_code = pd.ProductId 
  and sp.vendorid = pd.VendorId 
  LEFT JOIN [CSCMaster].[dbo].[stockmst] sm
                ON sm.stock_code = pd.productid
WHERE 
  pd.Trans_Flag = 'O' 
  AND pd.PR_Type != 'R' 
  AND sp.Stock_Code IS NOT NULL 
GO


-- Create SProc SProc_PerformCostChangeTransfer | ICSCR-670
IF OBJECT_ID('SProc_PerformCostChangeTransfer', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE SProc_PerformCostChangeTransfer
END
GO

CREATE PROCEDURE SProc_PerformCostChangeTransfer
@policySetVend BIT,
@policyUpdateAllVendor BIT,
@policyMarginCalcCostMkup BIT,
@policyNumPrice INT,
@userCode AS VARCHAR(10)
AS
BEGIN
    -- Update Existing StockCosts
    UPDATE sc
    SET sc.Cost = cct.NewCost,
        sc.StartDate = cct.StartDate,
        sc.EndDate = cct.EndDate,
        UpdateDate = GETDATE()
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct
    JOIN [CSCMaster].[dbo].[StockCosts] sc
        ON  sc.Stock_Code = cct.ProductID
        AND sc.VendorId = iif (EXISTS
        (
          SELECT 1
          FROM [CSCMaster].[dbo].[StockCosts]
          WHERE
              cct.ProductID = [CSCMaster].[dbo].[StockCosts].Stock_Code
              AND cct.VendorId = [CSCMaster].[dbo].[StockCosts].VendorID
              AND cct.OrderNo = [CSCMaster].[dbo].[StockCosts].OrderNo
        ), cct.vendorId, 'ALL')
        AND sc.OrderNo = cct.OrderNo
    WHERE Trans_Flag = 'O';

    IF @policySetVend = 0 AND @policyUpdateAllVendor = 1
    BEGIN
        UPDATE sc
        SET sc.Cost       = cct.NewCost,
            sc.StartDate  = cct.StartDate,
            sc.EndDate    = cct.EndDate,
            sc.UpdateDate = GETDATE()
        FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct
        JOIN [CSCMaster].[dbo].[StockCosts] sc 
            ON  sc.Stock_Code = cct.ProductID
            AND sc.VendorId   = 'ALL'
            AND sc.OrderNo    = cct.OrderNo
        WHERE Trans_Flag = 'O';
    END


    -- Insert Missing StockCosts
    INSERT INTO [CSCMaster].[dbo].[StockCosts] (Stock_Code, VendorID, Cost, StartDate, EndDate, ActiveVendor, OrderNo)
    SELECT cct.ProductID as StockCode
        ,ISNULL(cct.VendorID, 'ALL') AS VendorID
        ,cct.NewCost AS Cost
        ,cct.StartDate
        ,cct.EndDate
        ,cct.ActiveVendor
        ,cct.OrderNo
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct
    JOIN (
        SELECT ProductID, ISNULL(VendorID, 'ALL') AS VendorID, OrderNo, MAX(batchId) AS MaxBatchId
        FROM [CSCTransfer].[dbo].[CostChangeTransfer]
      WHERE Trans_Flag = 'O'
        GROUP BY ProductID, ISNULL(VendorID, 'ALL'), OrderNo
    ) AS LatestRecord
        ON cct.ProductID = LatestRecord.ProductID
        AND ISNULL(cct.VendorID, 'ALL') = LatestRecord.VendorID
        AND cct.OrderNo = LatestRecord.OrderNo
        AND cct.batchId = LatestRecord.MaxBatchId
    LEFT JOIN [CSCMaster].[dbo].[StockCosts] sc
      ON  sc.Stock_Code = cct.ProductID 
      AND sc.VendorId   = iif (EXISTS
         (
          SELECT 1
          FROM [CSCMaster].[dbo].[StockCosts]
          WHERE cct.ProductID = [CSCMaster].[dbo].[StockCosts].Stock_Code
               AND cct.VendorId = [CSCMaster].[dbo].[StockCosts].VendorID
            AND cct.OrderNo = [CSCMaster].[dbo].[StockCosts].OrderNo
         ), cct.vendorId, 'ALL')
      AND sc.OrderNo    = cct.OrderNo
    WHERE Trans_Flag = 'O'
    GROUP BY cct.ProductID, cct.VendorID, cct.NewCost, cct.StartDate, cct.EndDate, cct.OrderNo, cct.ActiveVendor
    HAVING count(sc.cost) = 0;

    IF @policySetVend = 0 AND @policyUpdateAllVendor = 1
    BEGIN
        INSERT INTO [CSCMaster].[dbo].[StockCosts] (Stock_Code, VendorID, Cost, StartDate, EndDate, ActiveVendor, OrderNo)
        SELECT cct.ProductID as StockCode
            ,'ALL' AS VendorID
            ,cct.NewCost AS Cost
            ,cct.StartDate
            ,cct.EndDate
            ,0 AS ActiveVendor
            ,cct.OrderNo
        FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct 
        JOIN (
            SELECT ProductID, 'ALL' AS VendorID, OrderNo, MAX(batchId) AS MaxBatchId
            FROM [CSCTransfer].[dbo].[CostChangeTransfer]
          WHERE Trans_Flag = 'O'
            GROUP BY ProductID, ISNULL(VendorID, 'ALL'), OrderNo
        ) AS LatestRecord
            ON cct.ProductID = LatestRecord.ProductID
            AND LatestRecord.VendorID = 'ALL'
            AND cct.OrderNo = LatestRecord.OrderNo
            AND cct.batchId = LatestRecord.MaxBatchId
        LEFT JOIN [CSCMaster].[dbo].[StockCosts] sc
          ON  sc.Stock_Code = cct.ProductID 
          AND sc.VendorId   = 'ALL'
          AND sc.OrderNo    = cct.OrderNo
        WHERE Trans_Flag = 'O'
        GROUP BY cct.ProductID, cct.VendorID, cct.NewCost, cct.StartDate, cct.EndDate, cct.OrderNo, cct.ActiveVendor
        HAVING count(sc.cost) = 0;
    END

    -- Insert the results into CSCTrans.dbo.PriceChange

    -- The following function is using to track the price change
    -- Processtype PC--> Price check;
    -- SL--> Saleline;
    -- SC--> Stock Screen Cost Change(Price will change) +  Stock Screen Price Change;
    -- VC- Stock Screen Switch Active Vendor;
    -- PR--> Product Category-Change Price;
    -- FP- FuturePrice rollover;
    -- PB- Price book management( Cost Change(Price will change) +   Price Change);
    -- DT- dTS transfer
    -- Nicolette, March 23, 2009 added VendorID; need it only for StockPrice change or DTS transfer

    INSERT INTO CSCTrans.dbo.PriceChange
    (
        STOCK_CODE,
        EmployeeID,
        PriceNum,
        OriginalPrice,
        NewPrice,
        Date,
        Time,
        Process,
        VendorID
    )
    SELECT
        cct.ProductId AS STOCK_CODE,
        @userCode AS EmployeeID,
        sp.Price_Number AS PriceNum,
        ROUND(sp.price, 2) AS OriginalPrice,
        ROUND(
            CASE 
                WHEN ISNULL(cct.PriceChangeWithCost, 0) = 1 
                    THEN [CSCTransfer].dbo.CalculatePriceBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, ISNULL(sp.Margin, 0))
                    ELSE sp.Price
            END, 
        2) AS NewPrice,
        FORMAT(DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0), 'yyyy-MM-dd HH:mm:ss') AS Date,
        FORMAT(CONVERT(DATETIME, CONVERT(VARCHAR(19), GETDATE(), 120)), 'yyyy-MM-dd HH:mm:ss') AS Time,
        'PB' AS Process,
        cct.VendorID AS VendorID

    FROM CSCTransfer.dbo.CostChangeTransfer cct
        LEFT JOIN CSCMaster.dbo.Stock_Prices sp
            ON cct.ProductId = sp.Stock_Code
            AND cct.VendorID = sp.VendorID
            AND cct.orderno = sp.orderno
        WHERE cct.OrderNo = 0
            AND sp.Price_Number = 1
            AND cct.PriceChangeWithCost = 1;


    -- Update Existing StockPrices
    UPDATE sp
    SET sp.Price  = CASE WHEN ISNULL(cct.PriceChangeWithCost, 0) = 1
                         THEN [CSCTransfer].dbo.CalculatePriceBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, ISNULL(sp.Margin, 0))
                         ELSE sp.Price
                    END,
        sp.Margin = CASE WHEN ISNULL(cct.PriceChangeWithCost, 0) = 0
                         THEN [CSCTransfer].[dbo].CalculateMarginBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, sp.Price)
                         ELSE sp.Margin
                    END,
        sp.UpdateDate = GETDATE()
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct
    INNER JOIN [CSCMaster].[dbo].[Stock_Prices] sp
        ON cct.ProductID = sp.Stock_Code
        AND cct.OrderNo = sp.OrderNo
        AND cct.VendorId = sp.VendorID
    WHERE Trans_Flag = 'O';

    -- Update Existing StockPrices | For 'ALL' Vendor
    UPDATE sp
    SET sp.Price  = CASE WHEN ISNULL(cct.PriceChangeWithCost, 0) = 1
                         THEN [CSCTransfer].dbo.CalculatePriceBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, ISNULL(sp.Margin, 0))
                         ELSE sp.Price
                    END,
        sp.Margin = CASE WHEN ISNULL(cct.PriceChangeWithCost, 0) = 0
                         THEN [CSCTransfer].[dbo].CalculateMarginBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, sp.Price)
                         ELSE sp.Margin
                    END,
        sp.UpdateDate = GETDATE()
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct
    INNER JOIN [CSCMaster].[dbo].[Stock_Prices] sp
        ON cct.ProductID = sp.Stock_Code
        AND cct.OrderNo = sp.OrderNo
        AND sp.VendorId = 'ALL'
    WHERE Trans_Flag = 'O';


    -- Insert Missing StockPrices | Current Price exist, but Future Price doesn't
    INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
            Stock_Code
           ,Price_Number
           ,Price
           ,VendorId
           ,StartDate
           ,EndDate
           ,OrderNo
           ,Margin)
    SELECT   cct.ProductID AS Stock_Code
            ,spCurrent.Price_Number
            ,CASE WHEN cct.NewCost = 0 THEN COALESCE(spCurrent.Price, 0)
                  WHEN ISNULL(cct.PriceChangeWithCost, 0) = 1 THEN [CSCTransfer].[dbo].CalculatePriceBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, ISNULL(spCurrent.Margin, 0))
                  ELSE COALESCE(spCurrent.Price, 0)
             END AS Price
            ,ISNULL(cct.VendorID, 'ALL') AS VendorId
            ,cct.StartDate
            ,cct.EndDate
            ,cct.OrderNo
            ,CASE WHEN cct.NewCost = 0 THEN COALESCE(spCurrent.Margin, 0)
                  WHEN ISNULL(cct.PriceChangeWithCost, 0) = 0 THEN [CSCTransfer].[dbo].CalculateMarginBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, spCurrent.Price)
                  ELSE COALESCE(spCurrent.Margin, 0)
             END AS Margin
    FROM [CSCTransfer].[dbo].[vw_CostChangeRecords] cct
    LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] sp
         ON  cct.ProductID = sp.Stock_Code
         AND cct.VendorID  = sp.VendorId
         AND cct.OrderNo   = sp.OrderNo   
    LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] spCurrent
         ON  cct.ProductID     = spCurrent.Stock_Code
         AND cct.VendorID      = spCurrent.VendorId
         AND spCurrent.OrderNo = 0   
    WHERE sp.Price IS NULL 
          AND spCurrent.Price IS NOT NULL;


    -- Insert Missing StockPrices | Current Price exist, but Future Price doesn't | For 'ALL' Vendor
    INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
            Stock_Code
           ,Price_Number
           ,Price
           ,VendorId
           ,StartDate
           ,EndDate
           ,OrderNo
           ,Margin)
    SELECT   cct.ProductID AS Stock_Code
            ,spCurrent.Price_Number
            ,CASE WHEN cct.NewCost = 0 THEN COALESCE(spCurrent.Price, 0)
                  WHEN ISNULL(cct.PriceChangeWithCost, 0) = 1 THEN [CSCTransfer].[dbo].CalculatePriceBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, ISNULL(spCurrent.Margin, 0))
                  ELSE COALESCE(spCurrent.Price, 0)
             END AS Price
            ,'ALL' AS VendorId
            ,cct.StartDate
            ,cct.EndDate
            ,cct.OrderNo
            ,CASE WHEN cct.NewCost = 0 THEN COALESCE(spCurrent.Margin, 0)
                  WHEN ISNULL(cct.PriceChangeWithCost, 0) = 0 THEN [CSCTransfer].[dbo].CalculateMarginBasedOnPolicy(@policyMarginCalcCostMkup, cct.NewCost, spCurrent.Price)
                  ELSE COALESCE(spCurrent.Margin, 0)
             END AS Margin
    FROM [CSCTransfer].[dbo].[vw_CostChangeRecords] cct
    LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] sp
         ON  cct.ProductID = sp.Stock_Code
         AND sp.VendorId   = 'ALL'
         AND cct.OrderNo   = sp.OrderNo   
    LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] spCurrent
         ON  cct.ProductID      = spCurrent.Stock_Code
         AND spCurrent.VendorId = 'ALL'
         AND spCurrent.OrderNo  = 0   
    WHERE sp.Price IS NULL 
          AND spCurrent.Price IS NOT NULL;


    -- Insert Missing StockPrices
    INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
           Stock_Code
          ,Price_Number
          ,Price
          ,VendorId
          ,OrderNo
          ,Margin)
    SELECT ccts.Stock_Code
          ,pn.Price_Number
          ,ccts.Price
          ,ccts.VendorId
          ,ccts.OrderNo
          ,ccts.Margin
    FROM (
        SELECT cct.ProductID AS Stock_Code
              ,cct.NewCost AS Price
              ,ISNULL(cct.VendorID, 'ALL') AS VendorId
              ,cct.OrderNo
              ,0.0 AS Margin
        FROM [CSCTransfer].[dbo].[vw_CostChangeRecords] AS cct
        LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] sp
            ON  cct.ProductID = sp.Stock_Code
            AND cct.VendorId  = sp.VendorId
            AND cct.OrderNo   = sp.OrderNo
        WHERE sp.Price IS NULL
    ) ccts
    CROSS APPLY (
        SELECT TOP (@policyNumPrice) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Price_Number
        FROM sys.columns
    ) AS pn

    -- Insert Missing StockPrices | For 'ALL' Vendor
    INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
           Stock_Code
          ,Price_Number
          ,Price
          ,VendorId
          ,OrderNo
          ,Margin)
    SELECT ccts.Stock_Code
          ,pn.Price_Number
          ,ccts.Price
          ,ccts.VendorId
          ,ccts.OrderNo
          ,ccts.Margin
    FROM (
        SELECT cct.ProductID AS Stock_Code
              ,cct.NewCost AS Price
              ,'ALL' AS VendorId
              ,cct.OrderNo
              ,0.0 AS Margin
        FROM [CSCTransfer].[dbo].[vw_CostChangeRecords] AS cct
        LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] sp
            ON  cct.ProductID = sp.Stock_Code
            AND sp.VendorId   = 'ALL'
            AND cct.OrderNo   = sp.OrderNo
        WHERE sp.Price IS NULL
    ) ccts
    CROSS APPLY (
        SELECT TOP (@policyNumPrice) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Price_Number
        FROM sys.columns
    ) AS pn


    -- Insert Price Change And Price Definition Record 
    DECLARE @dataTransferType VARCHAR(15) = 'price-change';
    DECLARE @newBatchId INT = [CSCTransfer].dbo.GetNewBatchId(@dataTransferType);

    INSERT INTO [CSCTransfer].[dbo].[PriceDefinition] (
           BatchId
          ,ProductID
          ,PR_Type
          ,Store_id
          ,PR_From_Qty
          ,PR_To_Qty
          ,Price_Number
          ,Price
          ,PR_FROM
          ,PR_TO
          ,Trans_Date
          ,Trans_User
          ,Trans_Flag
          ,OrderNo
          ,VendorId
          ,Margin)
    SELECT (@newBatchId + (ROW_NUMBER() OVER (ORDER BY BatchId, cct.ProductID)) - 1) AS BatchId
          ,cct.ProductID
          ,'R' AS PR_Type
          ,ISNULL(cct.Store_Id, 'ALL') AS Store_Id
          ,0 AS PR_From_Qty
          ,0 AS PR_To_Qty
          ,Price_Number
          ,Price
          ,cct.StartDate AS PR_From
          ,cct.EndDate AS PR_To
          ,GETDATE() AS Trans_Date
          ,@userCode AS Trans_User
          ,'N' AS Trans_Flag
          ,cct.OrderNo
          ,ISNULL(cct.VendorId, 'ALL') AS VendorId
          ,ISNULL(Margin, 0) AS Margin
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct 
    JOIN [CSCMaster].[dbo].[Stock_Prices] sp 
        ON  cct.ProductID   = sp.Stock_Code
        AND cct.VendorId    = sp.VendorId
    --    AND cct.Store_ID    = sp.storeId 
        AND cct.OrderNo     = sp.OrderNo
        AND sp.Price_Number = 1
    WHERE Trans_Flag = 'O'
    ORDER BY ProductId;

    
    INSERT INTO [CSCTransfer].[dbo].[PriceChangeTransfer] (
           BatchId
          ,ProductID
          ,Description
          ,PriceType
          ,Store_Id
          ,Trans_Date
          ,Trans_User
          ,Trans_Flag
          ,VendorId
          ,OrderNo)
    SELECT (@newBatchId + (ROW_NUMBER() OVER (ORDER BY BatchId, ProductID, Store_Id)) - 1) AS BatchId
          ,ProductID
          ,(SELECT Descript FROM [CSCMaster].[dbo].[STOCKMST] WHERE STOCK_CODE = ProductID) AS Description
          ,'R' AS PriceType
          ,ISNULL(Store_Id, 'ALL') AS Store_Id
          ,GETDATE() AS Trans_Date
          ,@userCode AS Trans_User
          ,'N' AS Trans_Flag
          ,ISNULL(VendorId, 'ALL') AS VendorId
          ,OrderNo
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] 
    WHERE Trans_Flag = 'O'
    ORDER BY Store_Id, ProductId;

    -- Insert Price Change And Price Definition Record | For 'ALL' Vendor
    SET @newBatchId = [CSCTransfer].dbo.GetNewBatchId(@dataTransferType);
    
    INSERT INTO [CSCTransfer].[dbo].[PriceDefinition] (
            BatchId
          ,ProductID
          ,PR_Type
          ,Store_id
          ,PR_From_Qty
          ,PR_To_Qty
          ,Price_Number
          ,Price
          ,PR_FROM
          ,PR_TO
          ,Trans_Date
          ,Trans_User
          ,Trans_Flag
          ,OrderNo
          ,VendorId
          ,Margin)
    SELECT (@newBatchId + (ROW_NUMBER() OVER (ORDER BY BatchId, cct.ProductID, cct.Store_Id)) - 1) AS BatchId
          ,cct.ProductID
          ,'R' AS PR_Type
          ,ISNULL(cct.Store_Id, 'ALL') AS Store_Id
          ,0 AS PR_From_Qty
          ,0 AS PR_To_Qty
          ,Price_Number
          ,Price
          ,cct.StartDate AS PR_From
          ,cct.EndDate AS PR_To
          ,GETDATE() AS Trans_Date
          ,@userCode AS Trans_User
          ,'N' AS Trans_Flag
          ,cct.OrderNo
          ,'ALL' AS VendorId
          ,ISNULL(Margin, 0) AS Margin
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct 
    JOIN [CSCMaster].[dbo].[Stock_Prices] sp 
        ON  cct.ProductID   = sp.Stock_Code
        AND sp.VendorId     = 'ALL'
        AND cct.OrderNo     = sp.OrderNo
        AND sp.Price_Number = 1
    WHERE Trans_Flag = 'O'
    ORDER BY Store_Id, ProductId;

    
    INSERT INTO [CSCTransfer].[dbo].[PriceChangeTransfer] (
            BatchId
          ,ProductID
          ,Description
          ,PriceType
          ,Store_Id
          ,Trans_Date
          ,Trans_User
          ,Trans_Flag
          ,VendorId
          ,OrderNo)
    SELECT (@newBatchId + (ROW_NUMBER() OVER (ORDER BY BatchId, ProductID, Store_Id)) - 1) AS BatchId
          ,ProductID
          ,(SELECT Descript FROM [CSCMaster].[dbo].[STOCKMST] WHERE STOCK_CODE = ProductID) AS Description
          ,'R' AS PriceType
          ,ISNULL(Store_Id, 'ALL') AS Store_Id
          ,GETDATE() AS Trans_Date
          ,@userCode AS Trans_User
          ,'N' AS Trans_Flag
          ,'ALL' AS VendorId
          ,OrderNo
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] 
    WHERE Trans_Flag = 'O'
    ORDER BY Store_Id, ProductId;

    -- Move Records And Change The Status
    INSERT INTO CostChangeTransferHist 
    SELECT A.*
    FROM CostChangeTransfer A 
    WHERE A.Trans_Flag='O';

    UPDATE CostChangeTransferHist
    SET Trans_Flag = 'Y'
    WHERE Trans_Flag = 'O';

    DELETE FROM CostChangeTransfer 
END
GO

-- Create Cost Change Transfer View | ICSCR-670
USE CSCTransfer;
GO

IF OBJECT_ID('dbo.vw_CostChangeRecords', 'V') IS NOT NULL
  DROP VIEW dbo.vw_CostChangeRecords;
GO

CREATE VIEW [dbo].[vw_CostChangeRecords] 
AS 
SELECT MaxBatchId
      ,latest.ProductID
      ,Description
      ,StartDate
      ,EndDate
      ,OldCost
      ,NewCost
      ,latest.Store_Id
      ,PriceGroup_Id
      ,Region_id
      ,latest.VendorId
      ,latest.OrderNo
      ,Trans_Date
      ,Trans_User
      ,Trans_Flag
      ,ActiveVendor
      ,PriceChangeWithCost
FROM [CSCTransfer].[dbo].[CostChangeTransfer] AS cct
INNER JOIN (
        SELECT MAX(BatchId) AS MaxBatchId, ProductId, Store_Id, VendorId, OrderNo
        FROM [CSCTransfer].[dbo].[CostChangeTransfer] 
        WHERE Trans_Flag = 'O'
        GROUP BY ProductId, Store_Id, VendorId, OrderNo 
    ) AS latest 
    ON  cct.BatchId   = latest.MaxBatchId 
    AND cct.ProductId = latest.ProductId 
    AND cct.Store_Id  = latest.Store_Id 
    AND cct.VendorId  = latest.VendorId 
    AND cct.OrderNo   = latest.OrderNo 
WHERE Trans_Flag = 'O'
GO

-- Create SP Sproc_GetSaleAnalysisData - ICSCR-694

USE [CSCAdmin];
GO

IF NOT EXISTS (
    SELECT *
    FROM sys.types
    WHERE is_user_defined = 1
    AND name = 'EmployeesAndShiftsTableType'
)

BEGIN
  CREATE TYPE dbo.EmployeesAndShiftsTableType AS TABLE (
      Username NVARCHAR(50),
      ShiftDate DATE
  );
END
GO

IF OBJECT_ID('Sproc_GetSaleAnalysisData', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE Sproc_GetSaleAnalysisData
END
GO

CREATE PROCEDURE [dbo].Sproc_GetSaleAnalysisData
  @EmployeesAndShifts EmployeesAndShiftsTableType READONLY,
  @FuelDept NVARCHAR(10) = ''
AS
BEGIN
         WITH DrawerCount AS (
            SELECT Count(*) AS Cnt, CONVERT(DATE, od.DateOpen) as DateOpen,
            od.UserCode as Username
            FROM  CSCMaster.dbo.OpenDrawer od
            JOIN @EmployeesAndShifts AS u ON od.UserCode = u.Username where CONVERT(DATE, od.DateOpen) = CONVERT(DATE, u.ShiftDate)
            GROUP BY od.UserCode, CONVERT(DATE, od.DateOpen)
        ),
        EmployeesAndShifts AS (select Username as Username, ShiftDate as ShiftDate from @EmployeesAndShifts),
        SalesCount AS (
            SELECT sh.[User] as Username, CONVERT(DATE, sh.ShiftDate) as ShiftDate, Count(*) AS Cnt
            FROM 
            CSCTrans.dbo.SaleHead sh
            JOIN @EmployeesAndShifts AS u ON sh.[User] = u.Username AND CONVERT(DATE, sh.ShiftDate) = CONVERT(DATE, u.ShiftDate)
            GROUP BY sh.[User], CONVERT(DATE, sh.ShiftDate)
        ),
        DeletedLines AS (
            SELECT CONVERT(DATE, Del_Date) as Del_Date, dl.[User] as Username, Count(*) AS Cnt, Sum(AMOUNT) AS DeletedLinesTotalAmount
            FROM CSCTrans.dbo.DelLines dl 
            JOIN @EmployeesAndShifts AS u ON dl.[User] = u.Username AND
            CONVERT(DATE,dl.Del_Date) = CONVERT(DATE, u.ShiftDate) 
            GROUP BY dl.[User], CONVERT(DATE, Del_Date)
        ),
        SaleData AS (
            SELECT 
                sh.[User] as Username,
                CONVERT(DATE, sh.ShiftDate) as ShiftDate,
            Shift as Shift,
                SUM(sl.Amount) AS TotalAmount,
                AVG(sh.Sale_Amt) AS AvgSaleAmount
            FROM CSCTrans.dbo.SaleHead sh 
            INNER JOIN CSCTrans.dbo.SaleLine sl ON sh.Sale_No = sl.Sale_No

            JOIN @EmployeesAndShifts AS u ON sh.[User] = u.Username AND sh.ShiftDate = u.ShiftDate
            where sl.Dept <> @FuelDept
            GROUP BY sh.[User], CONVERT(DATE, sh.ShiftDate), Shift
        ),
        RefundData AS (
            SELECT 
                u.Username,
                sh.Sale_No, 
                sh.ShiftDate, 
                sh.[User], 
                vs.Void_No, 
                sl.Stock_Code, 
                sl.Descript, 
                sl.Dept, 
                sl.Quantity AS Qty, 
                sl.Amount
            FROM @EmployeesAndShifts u
            JOIN CSCTrans.dbo.SaleHead sh ON u.Username = sh.[User]
            INNER JOIN CSCTrans.dbo.SaleLine sl ON sh.Sale_No = sl.Sale_No 
            LEFT JOIN CSCTrans.dbo.VoidSale vs ON sh.Sale_No = vs.Sale_No 
            WHERE (sh.T_Type = 'REFUND' OR vs.Void_No <> 0)
            AND CONVERT(DATE, sh.ShiftDate) = CONVERT(DATE, u.ShiftDate)
        )
        SELECT 
            ES.Username as EmployeeID,
            DC.Cnt AS CashDrawerCount,
            SC.Cnt AS SalesCount,
            DL.Cnt AS DeletedLinesCount,
            DL.DeletedLinesTotalAmount AS DeletedLinesTotalAmount,
            SD.TotalAmount,
            SD.AvgSaleAmount,
            RD.Sale_No AS SaleNo,
            CONVERT(DATE, SD.ShiftDate) AS ShiftDate, 
            RD.Void_No AS VoidNo, 
            RD.Stock_Code as StockCode, 
            RD.Descript as [Description], 
            RD.Dept AS Department, 
            RD.Qty AS Quantity, 
            RD.Amount,
        SD.Shift
         FROM 
         EmployeesAndShifts ES 
         left JOIN 
        RefundData RD on RD.[User] = ES.Username AND RD.ShiftDate = ES.ShiftDate
        left join SaleData SD ON ES.Username = SD.Username AND SD.ShiftDate = ES.ShiftDate
        left JOIN DrawerCount DC ON ES.Username = DC.Username AND DC.DateOpen = ES.ShiftDate
        left JOIN SalesCount SC ON ES.Username = SC.Username AND SC.ShiftDate = ES.ShiftDate
        left JOIN DeletedLines DL ON ES.Username = DL.Username AND DL.Del_Date = ES.ShiftDate
        ORDER BY  ES.Username, RD.Sale_No;
END
GO


-- Create SP SProc_StockPriceInfoBatchForDetailReport | ICSCR-698
USE [CSCMaster]
GO

IF NOT EXISTS (
    SELECT *
    FROM sys.types
    WHERE is_user_defined = 1
    AND name = 'StockCodeAndVendorIDTableType'
)
BEGIN
	CREATE TYPE StockCodeAndVendorIDTableType AS TABLE (
    StockCode VARCHAR(16),
    VendorID VARCHAR(16)
  )
END
GO

IF OBJECT_ID('SProc_StockPriceInfoBatchForDetailReport', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [SProc_StockPriceInfoBatchForDetailReport]
END
GO

CREATE PROCEDURE SProc_StockPriceInfoBatchForDetailReport

@StockCodeList StockCodeAndVendorIDTableType READONLY,
--@VendorID as nvarchar(10)= '',
@PriceNum as integer,
@OrderNo AS INTEGER,
@StoreID as varchar(10)

AS
BEGIN
  if @PriceNum <> 0 /* we are bringing particular  price number info */
    Select * from Stock_prices
    left join @StockCodeList sc on stock_code = sc.StockCode and Stock_prices.vendorid = sc.VendorID
    Where price_number = @PriceNum and orderno = @OrderNo
  else
    Select *  from Stock_prices
    left join @StockCodeList sc on stock_code = sc.StockCode and Stock_prices.vendorid = sc.VendorID
    Where orderno = @OrderNo
END
GO

-- Create SP SProc_StockTaxInfoBatch | ICSCR-698
USE [CSCMaster]
GO

IF NOT EXISTS (
    SELECT *
    FROM sys.types
    WHERE is_user_defined = 1
    AND name = 'StockCodeTableType'
)
BEGIN
	CREATE TYPE StockCodeTableType AS TABLE (
    StockCode VARCHAR(16)
  )
END
GO

IF OBJECT_ID('SProc_StockTaxInfoBatch', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE [SProc_StockTaxInfoBatch]
END
GO

CREATE PROCEDURE SProc_StockTaxInfoBatch
@StockCodeList StockCodeTableType READONLY,
@StoreID as varchar(10)
AS
BEGIN
  select StockTax.stock_code AS StockCode, StockTax.Tax_Name TaxName,StockTax.Tax_Code TaxCode,TAXRATE.TAX_RATE TaxRate
                     from stocktax INNER JOIN (TAXRATE TAXRATE  INNER JOIN TaxMast ON TaxRate.Tax_Name = TaxMast.Tax_Name)  
                     on Stocktax.tax_name = taxrate.tax_name and stocktax.tax_code = taxrate.tax_code 
                     WHERE StockTax.stock_code in (SELECT StockCode FROM @StockCodeList) and  TaxMast.Tax_Active =1  
END
GO

-- Create SP SProc_StockChargeInfoBatch | ICSCR-698
USE [CSCMaster]
GO

-- Create [CSCMaster].[dbo].[FamilyGroup] table
IF NOT EXISTS (
    SELECT * 
    FROM sysobjects 
    WHERE id = OBJECT_ID(N'[dbo].[FamilyGroup]') 
    AND OBJECTPROPERTY(id, N'IsUserTable') = 1
)
BEGIN
    CREATE TABLE [dbo].[FamilyGroup] (
        GroupID NVARCHAR(10) NOT NULL, -- Primary Key, not nullable
        FamilyID NVARCHAR(3) NOT NULL, -- Primary Key, not nullable
        GroupName NVARCHAR(15) NULL,   -- Nullable
        PRIMARY KEY (GroupID, FamilyID) -- Composite primary key
    )
END
GO

IF NOT EXISTS (
    SELECT *
    FROM sys.types
    WHERE is_user_defined = 1
    AND name = 'StockCodeTableType'
)
BEGIN
	CREATE TYPE StockCodeTableType AS TABLE (
    StockCode VARCHAR(16)
  )
END
GO

IF OBJECT_ID('SProc_StockChargeInfoBatch', 'P') IS NOT NULL
BEGIN
  DROP PROCEDURE [SProc_StockChargeInfoBatch]
END
GO

CREATE PROCEDURE SProc_StockChargeInfoBatch
@StockCodeList StockCodeTableType READONLY,
@StoreID as varchar(10)
AS
BEGIN
   SELECT STOCK_AC.STOCK_CODE AS StockCode, STOCK_AC.AS_CODE AS AssocCode,ASSOC.AS_DESC AS AssocDescription, ASSOC.AS_PRICE AS AssocPrice
             From STOCK_AC INNER JOIN ASSOC  ON STOCK_AC.As_Code = ASSOC.As_Code
             Where STOCK_AC.STOCK_CODE in (SELECT StockCode FROM @StockCodeList) order by STOCK_AC.AS_CODE
END
GO

USE [CSCTrans]
GO

IF OBJECT_ID('Sproc_FNGTR_Sales', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Sproc_FNGTR_Sales]
END
GO

CREATE PROCEDURE [dbo].[Sproc_FNGTR_Sales]
	@StartDate smalldatetime,
	@Enddate smalldatetime
AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF OBJECT_ID('dbo.reportdata', 'U') IS NOT NULL 
		DROP TABLE dbo.reportdata;

	SELECT B.Sale_Date + ' ' + convert(time, B.Sale_Time, 112) AS Sale_Date, B.Sale_NO As Sale_No, 
	C.TreatyNo As TreatyNo, A.Stock_Code As Product,
	A.Descript As Description, A.Price As Price, C.TaxFreePrice As Tax_Exempt_Price, (A.QUANTITY) AS Quantity,
	C.TotalTaxSaved AS TotalExemptedTax,  A.AMOUNT AS Sale_Total, C.PHONE AS Phone_No, '' AS Reason
	into CSCTrans.dbo.reportdata
	FROM SaleLine AS A 
	INNER JOIN SaleHead AS B ON A.Sale_No=B.Sale_No 
	LEFT OUTER JOIN CSCTrans.dbo.PurchaseItem AS C ON A.Sale_No=C.Sale_No AND A.LINE_NUM = C.Line_No 
	WHERE B.Sale_Date>=@StartDate AND B.Sale_Date<=@Enddate
	AND A.Stock_Code IN (SELECT Stock_Code FROM CSCPump.dbo.Grade WHERE FuelType='G' OR FuelType='D') 
	ORDER BY B.Sale_Date desc, B.SALE_NO desc
END
GO
