-- All necessary database scripts for Head Office of Infonet CSCR project will be written in this file


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

-- Create SP SProc_UpdateOrInsertStockPrices | ICSCR-110 Price Change List Tab
USE [CSCMaster]
GO

IF OBJECT_ID('SProc_UpdateOrInsertStockPrices', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [SProc_UpdateOrInsertStockPrices]
END
GO

CREATE PROCEDURE SProc_UpdateOrInsertStockPrices 
    @Stock_Code VARCHAR(16),
    @Price_Number SMALLINT,
    @Price MONEY,
    @StoreID VARCHAR(6),
    @VendorID VARCHAR(10),
    @StartDate SMALLDATETIME = NULL,
    @EndDate SMALLDATETIME = NULL,
    @OrderNo SMALLINT,
    @Margin REAL
AS
BEGIN
    -- Check if record exists
    DECLARE @RecordCount INT
    SELECT  @RecordCount = COUNT(*) FROM Stock_Prices 
    WHERE   Stock_Code = @Stock_Code
    AND     StoreID = @StoreID 
    AND     Price_Number= @Price_Number 
    AND     OrderNO = @OrderNo 
    AND     VendorID = @VendorID

    -- If record exists, update it
    IF @RecordCount > 0 
    BEGIN
        UPDATE Stock_Prices
        SET 
            Price = @Price,
            Margin = @Margin,
            StartDate = CASE WHEN @OrderNo <> 0 THEN @StartDate ELSE NULL END,
            EndDate = CASE WHEN @OrderNo <> 0 THEN @EndDate ELSE NULL END
        WHERE 
            Stock_Code = @Stock_Code
            AND StoreID = @StoreID 
            AND Price_Number= @Price_Number 
            AND OrderNo = @OrderNo 
            AND VendorID = @VendorID
    END

    -- If record does not exist, insert a new record
    ELSE
    BEGIN
        INSERT INTO Stock_Prices 
        (
            Stock_Code,
            Price_Number,
            Price,
            StoreID,
            VendorID,
            Startdate,
            EndDate,
            OrderNo,
            Margin
        )
        VALUES 
        (
            @Stock_Code,
            @Price_Number,
            @Price,
            @StoreID,
            @VendorID,
            CASE WHEN @OrderNo <> 0 THEN @StartDate ELSE NULL END,
            CASE WHEN @OrderNo <> 0 THEN @EndDate ELSE NULL END,
            @OrderNo,
            @Margin
        )
    END
END
GO

-- Create SP SProc_PerformPriceChangeTransfer | ICSCR-110 Price Change List Tab
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
    WHERE A.Trans_Flag = 'O' AND A.Store_id = 'ALL'; 

    DELETE FROM  PriceChangeTransfer  
    WHERE Trans_Flag = 'O' AND Store_id = 'ALL'; 

    INSERT INTO PriceDefinitionHist   
    SELECT  A.*   
    FROM  PriceDefinition A 
    WHERE A.Trans_Flag = 'O'; 

    DELETE FROM  PriceDefinition  
    WHERE Trans_Flag = 'O';  

    UPDATE PriceChangeTransfer 
    SET Trans_Flag = 'N' 
    WHERE Trans_Flag = 'O'; 
END
GO

-- Create SP SProc_StockCostInfoBatch | ICSCR-213
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
@StoreID AS VARCHAR(10),
@StockCodes AS StockCodeTableType READONLY,
@OrderNo AS INTEGER
AS
BEGIN
  SELECT storeId, Cost, VendorId, LTRIM(RTRIM(Stock_Code)) AS StockCode, OrderNo, ActiveVendor
  FROM StockCosts 
  LEFT JOIN Vendors ON stockcosts.Vendorid = Vendors.Code 
  WHERE stockcosts.stock_code 
  IN ((SELECT StockCode FROM @StockCodes)) 
  AND (activevendor =1 OR Vendorid ='ALL') 
  AND orderno = @OrderNo 
  AND storeid = @StoreID
  ORDER BY activevendor DESC 
END
GO

-- Create SP SProc_PackingInfoBatch | ICSCR-213
USE [CSCMaster]
GO

IF OBJECT_ID('SProc_PackingInfoBatch', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [SProc_PackingInfoBatch]
END
GO

CREATE PROCEDURE SProc_PackingInfoBatch
@StockCodes AS StockCodeTableType READONLY
AS
BEGIN
  SELECT PACKING, STOCK_CODE AS StockCode 
  FROM STOCKMST 
  WHERE STOCK_CODE 
  IN ((SELECT StockCode FROM @StockCodes))
END
GO

-- Create SP SProc_StockPriceInfoBatch | ICSCR-213
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
@StoreID as varchar(10),
@StockPriceTemp  as StockPriceTableType READONLY,
@PriceNum as integer,
@OrderNo as integer
AS
BEGIN
  IF @PriceNum <> 0
	  SELECT Stock_Code AS StockCode, StoreID, Price, Price_Number AS PriceNumber, VendorID, OrderNo, MarginForOrderNo0 Margin
    FROM Stock_prices
		INNER JOIN @StockPriceTemp 
    ON Stock_Code = StockCodeTemp 
    AND VendorID = VendorIDTemp
	--We need to fetch margin data using order no = 0 according to VB code
	INNER JOIN (
		SELECT 
			Stock_Code AS StockCodeForOrderNo0, 
			Price_Number PriceNumberForOrderNo0, 
			StoreID StoreIDForOrderNo0, 
			VendorID VendorIDForOrderNo0, 
			Margin MarginForOrderNo0
		FROM Stock_prices
		WHERE OrderNo = 0
		AND StoreID = @StoreID
		AND price_number = @PriceNum
	) StockPricesForOrderNo0 ON (StockPricesForOrderNo0.StockCodeForOrderNo0 = StockCodeTemp AND StockPricesForOrderNo0.VendorIDForOrderNo0 = VendorIDTemp )
		WHERE price_number = @PriceNum 
    AND orderno = @OrderNo 
    AND StoreID = @StoreID
  ELSE
	  SELECT Stock_Code AS StockCode, StoreID, Price, Price_Number AS PriceNumber, VendorID, OrderNo, MarginForOrderNo0 Margin
    FROM Stock_prices
		INNER JOIN @StockPriceTemp 
    ON Stock_Code = StockCodeTemp 
    AND VendorID = VendorIDTemp
	--We need to fetch margin data using order no = 0 according to VB code
	INNER JOIN (
		SELECT 
			Stock_Code AS StockCodeForOrderNo0, 
			Price_Number PriceNumberForOrderNo0, 
			StoreID StoreIDForOrderNo0, 
			VendorID VendorIDForOrderNo0, 
			Margin MarginForOrderNo0
		FROM Stock_prices
		WHERE OrderNo = 0
		AND StoreID = @StoreID
	) StockPricesForOrderNo0 ON (StockPricesForOrderNo0.StockCodeForOrderNo0 = StockCodeTemp AND StockPricesForOrderNo0.VendorIDForOrderNo0 = VendorIDTemp )
		WHERE orderno = @OrderNo 
    AND StoreID = @StoreID
END
GO

-- Creating indexes for Cost Change Transfer | ICSCR-281
USE [CSCTransfer]
GO
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_Trans_Flag_NewCost_VendorId_OrderNo'
    AND object_id = OBJECT_ID('dbo.[CostChangeTransfer]')
)
BEGIN
    DROP INDEX [Idx_Trans_Flag_NewCost_VendorId_OrderNo] ON [dbo].[CostChangeTransfer];
END
GO
CREATE NONCLUSTERED INDEX [Idx_Trans_Flag_NewCost_VendorId_OrderNo]
ON [dbo].[CostChangeTransfer] ([Trans_Flag])
INCLUDE ([StartDate],[EndDate],[NewCost],[VendorId],[OrderNo])
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_Trans_Flag_ActiveVendor'
    AND object_id = OBJECT_ID('dbo.[CostChangeTransfer]')
)
BEGIN
    DROP INDEX [Idx_Trans_Flag_ActiveVendor] ON [dbo].[CostChangeTransfer];
END
GO
CREATE NONCLUSTERED INDEX [Idx_Trans_Flag_ActiveVendor]
ON [dbo].[CostChangeTransfer] ([Trans_Flag])
INCLUDE ([StartDate],[EndDate],[NewCost],[VendorId],[OrderNo],[ActiveVendor])
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_Trans_Flag_PriceChangeWithCost'
    AND object_id = OBJECT_ID('dbo.[CostChangeTransfer]')
)
BEGIN
    DROP INDEX [Idx_Trans_Flag_PriceChangeWithCost] ON [dbo].[CostChangeTransfer];
END
GO
CREATE NONCLUSTERED INDEX [Idx_Trans_Flag_PriceChangeWithCost]
ON [dbo].[CostChangeTransfer] ([Trans_Flag])
INCLUDE ([NewCost],[VendorId],[OrderNo],[PriceChangeWithCost])
GO

USE [CSCMaster]
GO
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_Stock_Code_StoreId_VendorId_OrderNo'
    AND object_id = OBJECT_ID('dbo.[Stock_Prices]')
)
BEGIN
    DROP INDEX [Idx_Stock_Code_StoreId_VendorId_OrderNo] ON [dbo].[Stock_Prices];
END
GO
CREATE NONCLUSTERED INDEX [Idx_Stock_Code_StoreId_VendorId_OrderNo]
ON [dbo].[Stock_Prices] ([Stock_Code],[StoreID],[VendorID],[OrderNo])
INCLUDE ([Price],[Margin])
GO


-- Creating indexes for Price Change Transfer | ICSCR-280
USE [CSCMaster]
GO
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_Stock_Prices_Price_Number_StoreID_VendorID'
    AND object_id = OBJECT_ID('dbo.[Stock_Prices]')
)
BEGIN
    DROP INDEX [Idx_Stock_Prices_Price_Number_StoreID_VendorID] ON [dbo].[Stock_Prices];
END
GO
CREATE NONCLUSTERED INDEX [Idx_Stock_Prices_Price_Number_StoreID_VendorID]
ON [dbo].[Stock_Prices] ([Stock_Code],[Price_Number],[StoreID],[VendorID],[OrderNo])
INCLUDE ([UpdateDate])
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_StoreID_Cost_ActiveVendor'
    AND object_id = OBJECT_ID('dbo.[StockCosts]')
)
BEGIN
    DROP INDEX [Idx_StoreID_Cost_ActiveVendor] ON [dbo].[StockCosts];
END
GO
CREATE NONCLUSTERED INDEX [Idx_StoreID_Cost_ActiveVendor]
ON [dbo].[StockCosts] ([StoreID])
INCLUDE ([Cost],[ActiveVendor])
GO


IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_OrderNo_Cost_ActiveVendor'
    AND object_id = OBJECT_ID('dbo.[StockCosts]')
)
BEGIN
    DROP INDEX [Idx_OrderNo_Cost_ActiveVendor] ON [dbo].[StockCosts];
END
GO
CREATE NONCLUSTERED INDEX [Idx_OrderNo_Cost_ActiveVendor]
ON [dbo].[StockCosts] ([OrderNo])
INCLUDE ([Cost],[ActiveVendor])
GO


USE [CSCTransfer]
GO
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_PR_Type_Trans_Flag'
    AND object_id = OBJECT_ID('dbo.[PriceDefinition]')
)
BEGIN
    DROP INDEX [Idx_PR_Type_Trans_Flag] ON [dbo].[PriceDefinition];
END
GO

CREATE NONCLUSTERED INDEX [Idx_PR_Type_Trans_Flag]
ON [dbo].[PriceDefinition] ([PR_Type],[Trans_Flag])
INCLUDE ([PR_FROM],[PR_TO])
GO


IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'Idx_Trans_Flag_Trans_Date_Trans_User_Margin'
    AND object_id = OBJECT_ID('dbo.[PriceDefinition]')
)
BEGIN
    DROP INDEX [Idx_Trans_Flag_Trans_Date_Trans_User_Margin] ON [dbo].[PriceDefinition];
END
GO
CREATE NONCLUSTERED INDEX [Idx_Trans_Flag_Trans_Date_Trans_User_Margin]
ON [dbo].[PriceDefinition] ([Trans_Flag])
INCLUDE ([PR_Type],[PR_Units],[PR_To_Qty],[Price],[PR_FROM],[PR_TO],[Trans_Date],[Trans_User],[Margin])
GO

-- Create Func GetNewBatchId | ICSCR-281
USE CSCTransfer;
GO

IF OBJECT_ID(N'dbo.GetNewBatchId', N'FN') IS NOT NULL
BEGIN
    DROP FUNCTION dbo.GetNewBatchId;
END
GO

CREATE FUNCTION GetNewBatchId(@dataTransferType VARCHAR(15))
RETURNS INT
AS
BEGIN
    DECLARE @newBatchId INT = 1;

    IF @dataTransferType = 'price-change'
    BEGIN
        SET @newBatchId = (
            SELECT MAX(BatchId) + 1
            FROM (
                SELECT BatchId FROM PriceDefinition
                UNION ALL
				SELECT BatchId FROM PriceDefinitionHist
                UNION ALL
				SELECT BatchId FROM PriceChangeTransfer
                UNION ALL
                SELECT BatchId FROM PriceChangeTransferHist
            ) AS CombinedTables
        );
    END
    ELSE IF @dataTransferType = 'stocks'
    BEGIN
        SET @newBatchId = (
            SELECT MAX(BatchId) + 1
            FROM (
                SELECT BatchId FROM StockTransfer
                UNION ALL
                SELECT BatchId FROM StockTransferHist
            ) AS CombinedTables
        );
    END
    ELSE IF @dataTransferType = 'cost-change'
    BEGIN
        SET @newBatchId = (
            SELECT MAX(BatchId) + 1
            FROM (
                SELECT BatchId FROM CostChangeTransfer
                UNION ALL
                SELECT BatchId FROM CostChangeTransferHist
            ) AS CombinedTables
        );
    END
    ELSE IF @dataTransferType = 'commodity'
    BEGIN
        SET @newBatchId = (
            SELECT MAX(BatchId) + 1
            FROM (
                SELECT BatchId FROM CommodityTransfer
                UNION ALL
                SELECT BatchId FROM CommodityTransferHist
            ) AS CombinedTables
        );
    END
    ELSE IF @dataTransferType = 'promotions'
    BEGIN
        SET @newBatchId = (
            SELECT MAX(BatchId) + 1
            FROM (
                SELECT BatchId FROM PromotionTransfer
                UNION ALL
                SELECT BatchId FROM PromotionTransferHist
            ) AS CombinedTables
        );
    END
    ELSE IF @dataTransferType = 'hot-buttons'
    BEGIN
        SET @newBatchId = (
            SELECT MAX(BatchId) + 1
            FROM (
                SELECT BatchId FROM HotButtonTransfer
                UNION ALL
                SELECT BatchId FROM HotButtonTransferHist
            ) AS CombinedTables
        );
    END

    RETURN ISNULL(@newBatchId, 1); 
END;
GO

-- Create Func CalculatePriceBasedOnPolicy | ICSCR-281
USE CSCTransfer;
GO

IF OBJECT_ID(N'dbo.CalculatePriceBasedOnPolicy', N'FN') IS NOT NULL
BEGIN
    DROP FUNCTION dbo.CalculatePriceBasedOnPolicy;
END
GO

CREATE FUNCTION [dbo].CalculatePriceBasedOnPolicy (
    @policyMarginCalcCostMkup BIT,
    @cost MONEY,
    @margin REAL
)
RETURNS MONEY
AS
BEGIN
    DECLARE @price MONEY = 0;

    IF @policyMarginCalcCostMkup = 1
    BEGIN
        SET @price = @cost * (1 + @margin / 100);
    END
    ELSE
    BEGIN
        IF @margin >= 0 AND @margin < 100
        BEGIN
            SET @price = @cost / (1 - @margin / 100);
        END
    END

    RETURN @price;
END;
GO

-- Create Func CalculateMarginBasedOnPolicy | ICSCR-281
USE CSCTransfer;
GO

IF OBJECT_ID(N'dbo.CalculateMarginBasedOnPolicy', N'FN') IS NOT NULL
BEGIN
    DROP FUNCTION dbo.CalculateMarginBasedOnPolicy;
END
GO

CREATE FUNCTION [dbo].CalculateMarginBasedOnPolicy(
    @policyMarginCalcCostMkup BIT,
    @cost MONEY,
    @price MONEY
)
RETURNS REAL
AS
BEGIN
    DECLARE @margin REAL = 0;

    IF @policyMarginCalcCostMkup = 1
    BEGIN
        IF @cost <> 0
        BEGIN
            SET @margin = (@price - @cost) / @cost * 100;
        END
    END
    ELSE
    BEGIN
        IF @price != 0 AND @cost != 0
        BEGIN
            SET @margin = (@price - @cost) / @price * 100;
        END
    END

    RETURN @margin;
END;
GO

-- Create Cost Change Transfer View | ICSCR-281
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

-- Create SProc SProc_PerformCostChangeTransfer | ICSCR-281
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
              AND cct.store_ID = [CSCMaster].[dbo].[StockCosts].StoreID
              AND cct.OrderNo = [CSCMaster].[dbo].[StockCosts].OrderNo
        ), cct.vendorId, 'ALL')
        AND sc.StoreID = cct.store_ID
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
            AND sc.StoreID    = cct.store_ID
            AND sc.OrderNo    = cct.OrderNo
        WHERE Trans_Flag = 'O';
    END




    -- Insert Missing StockCosts
    INSERT INTO [CSCMaster].[dbo].[StockCosts] (Stock_Code, StoreID, VendorID, Cost, StartDate, EndDate, ActiveVendor, OrderNo)
    SELECT cct.ProductID as StockCode
	      ,ISNULL(cct.Store_ID, 'ALL') AS StoreID
	      ,ISNULL(cct.VendorID, 'ALL') AS VendorID
	      ,cct.NewCost AS Cost
	      ,cct.StartDate
	      ,cct.EndDate
	      ,cct.ActiveVendor
	      ,cct.OrderNo
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct
    JOIN (
        SELECT ProductID, ISNULL(Store_ID, 'ALL') AS StoreID, ISNULL(VendorID, 'ALL') AS VendorID, OrderNo, MAX(batchId) AS MaxBatchId
        FROM [CSCTransfer].[dbo].[CostChangeTransfer]
	    WHERE Trans_Flag = 'O'
        GROUP BY ProductID, ISNULL(Store_ID, 'ALL'), ISNULL(VendorID, 'ALL'), OrderNo
    ) AS LatestRecord
        ON cct.ProductID = LatestRecord.ProductID
        AND ISNULL(cct.Store_ID, 'ALL') = LatestRecord.StoreID
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
		            AND cct.store_ID = [CSCMaster].[dbo].[StockCosts].StoreID
			        AND cct.OrderNo = [CSCMaster].[dbo].[StockCosts].OrderNo
		      ), cct.vendorId, 'ALL')
	    AND sc.StoreID    = cct.store_ID 
      AND sc.OrderNo    = cct.OrderNo
    WHERE Trans_Flag = 'O'
    GROUP BY cct.ProductID, cct.VendorID, cct.NewCost, cct.StartDate, cct.EndDate, cct.Store_ID, cct.OrderNo, cct.ActiveVendor
    HAVING count(sc.cost) = 0;

    IF @policySetVend = 0 AND @policyUpdateAllVendor = 1
    BEGIN
        INSERT INTO [CSCMaster].[dbo].[StockCosts] (Stock_Code, StoreID, VendorID, Cost, StartDate, EndDate, ActiveVendor, OrderNo)
        SELECT cct.ProductID as StockCode
	          ,ISNULL(cct.Store_ID, 'ALL') AS StoreID
	          ,'ALL' AS VendorID
	          ,cct.NewCost AS Cost
	          ,cct.StartDate
	          ,cct.EndDate
	          ,0 AS ActiveVendor
	          ,cct.OrderNo
        FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct 
        JOIN (
            SELECT ProductID, ISNULL(Store_ID, 'ALL') AS StoreID, 'ALL' AS VendorID, OrderNo, MAX(batchId) AS MaxBatchId
            FROM [CSCTransfer].[dbo].[CostChangeTransfer]
	        WHERE Trans_Flag = 'O'
            GROUP BY ProductID, ISNULL(Store_ID, 'ALL'), ISNULL(VendorID, 'ALL'), OrderNo
        ) AS LatestRecord
            ON cct.ProductID = LatestRecord.ProductID
            AND ISNULL(cct.Store_ID, 'ALL') = LatestRecord.StoreID
            AND LatestRecord.VendorID = 'ALL'
            AND cct.OrderNo = LatestRecord.OrderNo
            AND cct.batchId = LatestRecord.MaxBatchId
        LEFT JOIN [CSCMaster].[dbo].[StockCosts] sc
	        ON  sc.Stock_Code = cct.ProductID 
	        AND sc.VendorId   = 'ALL'
	        AND sc.StoreID    = cct.store_ID 
          AND sc.OrderNo    = cct.OrderNo
        WHERE Trans_Flag = 'O'
        GROUP BY cct.ProductID, cct.VendorID, cct.NewCost, cct.StartDate, cct.EndDate, cct.Store_ID, cct.OrderNo, cct.ActiveVendor
        HAVING count(sc.cost) = 0;
    END



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
        AND cct.Store_id = sp.StoreID
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
        AND cct.Store_id = sp.StoreID
        AND cct.OrderNo = sp.OrderNo
        AND sp.VendorId = 'ALL'
    WHERE Trans_Flag = 'O';



    -- Insert Missing StockPrices | Current Price exist, but Future Price doesn't
    INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
		         Stock_Code
		        ,Price_Number
		        ,Price
		        ,StoreId
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
            ,ISNULL(cct.Store_Id, 'ALL') AS StoreId
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
	       AND cct.Store_Id  = sp.StoreId
	       AND cct.OrderNo   = sp.OrderNo   
    LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] spCurrent
	       ON  cct.ProductID     = spCurrent.Stock_Code
	       AND cct.VendorID      = spCurrent.VendorId
	       AND cct.Store_Id      = spCurrent.StoreId
	       AND spCurrent.OrderNo = 0   
    WHERE sp.Price IS NULL 
          AND spCurrent.Price IS NOT NULL;


    -- Insert Missing StockPrices | Current Price exist, but Future Price doesn't | For 'ALL' Vendor
    INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
		         Stock_Code
		        ,Price_Number
		        ,Price
		        ,StoreId
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
            ,ISNULL(cct.Store_Id, 'ALL') AS StoreId
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
	       AND cct.Store_Id  = sp.StoreId
	       AND cct.OrderNo   = sp.OrderNo   
    LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] spCurrent
	       ON  cct.ProductID      = spCurrent.Stock_Code
	       AND spCurrent.VendorId = 'ALL'
	       AND cct.Store_Id       = spCurrent.StoreId
	       AND spCurrent.OrderNo  = 0   
    WHERE sp.Price IS NULL 
          AND spCurrent.Price IS NOT NULL;


    -- Insert Missing StockPrices
    INSERT INTO [CSCMaster].[dbo].[Stock_Prices] (
           Stock_Code
          ,Price_Number
          ,Price
          ,StoreId
          ,VendorId
          ,OrderNo
          ,Margin)
    SELECT ccts.Stock_Code
	        ,pn.Price_Number
          ,ccts.Price
          ,ccts.StoreId
          ,ccts.VendorId
          ,ccts.OrderNo
          ,ccts.Margin
    FROM (
        SELECT cct.ProductID AS Stock_Code
              ,cct.NewCost AS Price
              ,ISNULL(cct.Store_Id, 'ALL') AS StoreId
              ,ISNULL(cct.VendorID, 'ALL') AS VendorId
              ,cct.OrderNo
              ,0.0 AS Margin
        FROM [CSCTransfer].[dbo].[vw_CostChangeRecords] AS cct
        LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] sp
			        ON  cct.ProductID = sp.Stock_Code
			        AND cct.VendorId  = sp.VendorId
			        AND cct.Store_Id  = sp.StoreId
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
          ,StoreId
          ,VendorId
          ,OrderNo
          ,Margin)
    SELECT ccts.Stock_Code
	        ,pn.Price_Number
          ,ccts.Price
          ,ccts.StoreId
          ,ccts.VendorId
          ,ccts.OrderNo
          ,ccts.Margin
    FROM (
        SELECT cct.ProductID AS Stock_Code
              ,cct.NewCost AS Price
              ,ISNULL(cct.Store_Id, 'ALL') AS StoreId
              ,'ALL' AS VendorId
              ,cct.OrderNo
              ,0.0 AS Margin
        FROM [CSCTransfer].[dbo].[vw_CostChangeRecords] AS cct
        LEFT JOIN [CSCMaster].[dbo].[Stock_Prices] sp
			        ON  cct.ProductID = sp.Stock_Code
			        AND sp.VendorId   = 'ALL'
			        AND cct.Store_Id  = sp.StoreId
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
          ,ISNULL(cct.VendorId, 'ALL') AS VendorId
          ,ISNULL(Margin, 0) AS Margin
    FROM [CSCTransfer].[dbo].[CostChangeTransfer] cct 
    JOIN [CSCMaster].[dbo].[Stock_Prices] sp 
        ON  cct.ProductID   = sp.Stock_Code
        AND cct.VendorId    = sp.VendorId
        AND cct.Store_ID    = sp.storeId 
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
        AND cct.Store_Id    = sp.storeId
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
    WHERE A.Trans_Flag='O' AND (Store_id = 'ALL');
                    
    UPDATE CostChangeTransfer 
    SET Trans_Flag = 'N' 
    WHERE Trans_Flag ='O';

END
GO

-- Create Price Change Transfer View1 | ICSCR-280
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
  Store_Id,
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
        s.StoreID, 
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
  AND pd.Store_Id = s.StoreID 
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
      sAll.StoreID = 'ALL' 
      AND (
        sAll.activevendor = 1 
        or sAll.Vendorid = 'ALL'
      )
  ) AS sAll ON sAll.StoreID = 'ALL' 
  AND sAll.Stock_Code = pd.ProductId 
  AND sAll.OrderNo = pd.OrderNo 
  AND sAll.RowNum = 1 
  LEFT JOIN (
    SELECT 
      sNetwork.*, 
      ROW_NUMBER() OVER (
        PARTITION BY sNetwork.Stock_Code, 
        sNetwork.StoreID 
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
  ) AS sNetwork ON sNetwork.StoreID = pd.Store_Id 
  AND sNetwork.Stock_Code = pd.ProductId 
  AND sNetwork.OrderNo = 0 
  AND sNetwork.RowNum = 1 
  LEFT JOIN [CSCMaster].[dbo].[StockCosts] sc ON pd.ProductId = sc.Stock_Code 
  AND pd.Store_Id = sc.StoreID 
  AND pd.VendorId = sc.VendorID 
  AND pd.OrderNo = sc.OrderNo 
  LEFT JOIN [CSCMaster].[dbo].[Stockmst] sm ON sm.Stock_code = pd.ProductId 
WHERE 
  pd.Trans_Flag = 'O' 
  AND sc.Stock_Code IS NULL;
GO

-- Create Price Change Transfer View2 | ICSCR-280
USE CSCTransfer;
GO
IF OBJECT_ID('dbo.vw_StockPricesOfAllPriceDef', 'V') IS NOT NULL
	DROP VIEW dbo.vw_StockPricesOfAllPriceDef;
GO
CREATE VIEW [dbo].[vw_StockPricesOfAllPriceDef] 
AS 
SELECT 
  pd.ProductId, 
  pd.Store_Id, 
  pd.VendorId, 
  pd.OrderNo 
FROM 
  [CSCTransfer].[dbo].PriceDefinition pd 
  LEFT JOIN CSCMaster.[dbo].Stock_prices sp ON (
    sp.orderno = 0 
    or sp.OrderNo = pd.OrderNo
  ) 
  and sp.stock_code = pd.ProductId 
  and sp.StoreID = pd.Store_Id 
  and sp.vendorid = pd.VendorId 
WHERE 
  pd.Trans_Flag = 'O' 
  AND pd.PR_Type != 'R' 
  AND sp.Stock_Code IS NULL
GROUP BY pd.ProductId, 
  pd.Store_Id, 
  pd.VendorId, 
  pd.OrderNo 
GO

-- Create Price Change Transfer View3 | ICSCR-280
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
  pd.Store_Id, 
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
  and sp.StoreID = pd.Store_Id 
  and sp.vendorid = pd.VendorId 
  LEFT JOIN [CSCMaster].[dbo].[stockmst] sm
                ON sm.stock_code = pd.productid
WHERE 
  pd.Trans_Flag = 'O' 
  AND pd.PR_Type != 'R' 
  AND sp.Stock_Code IS NOT NULL 
GO

-- Create SProc SProc_PerformPriceChangeTransferAll | ICSCR-280
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
@NumberOfPrice INT
AS
BEGIN
    --::::::::::::::: Regular Price --- START :::::::::::::::
    --Create or Update Stock Price
    MERGE INTO [CSCMaster].[dbo].stock_prices AS target
    using (SELECT
            t.BatchId, 
            t.ProductId, 
            t.Price_Number, 
            t.Store_Id, 
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
                Store_Id, 
                VendorId 
              FROM 
                [CSCTransfer].[dbo].pricedefinition 
              WHERE 
                trans_flag = 'O' 
                AND pr_type = 'R' 
              GROUP BY 
                ProductId, 
                Price_Number, 
                Store_Id, 
                VendorId
            ) sub ON t.BatchId = sub.MaxBatchId 
            AND t.ProductId = sub.ProductId 
            AND t.Price_Number = sub.Price_Number 
            AND t.Store_Id = sub.Store_Id 
            AND t.VendorId = sub.VendorId 
          WHERE 
            t.trans_flag = 'O' 
            AND t.pr_type = 'R') AS source
    ON ( target.stock_code = source.productid
        AND target.storeid = source.store_id
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
            storeid,
            vendorid,
            startdate,
            enddate,
            orderno,
            margin)
    VALUES (source.productid,
            source.price_number,
            source.price,
            source.store_id,
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
                t.Store_Id, 
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
                    Store_Id, 
                    VendorId 
                  FROM 
                    [CSCTransfer].[dbo].pricedefinition 
                  WHERE 
                    trans_flag = 'O' 
                    AND pr_type = 'R' 
                  GROUP BY 
                    ProductId, 
                    Price_Number, 
                    Store_Id, 
                    VendorId
                ) sub ON t.BatchId = sub.MaxBatchId 
                AND t.ProductId = sub.ProductId 
                AND t.Price_Number = sub.Price_Number 
                AND t.Store_Id = sub.Store_Id 
                AND t.VendorId = sub.VendorId 
              WHERE 
                t.trans_flag = 'O' 
                AND t.pr_type = 'R') AS source
        ON ( target.stock_code = source.productid
                AND target.storeid = source.store_id
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
                    storeid,
                    vendorid,
                    startdate,
                    enddate,
                    orderno,
                    margin)
            VALUES (source.productid,
                    source.price_number,
                    source.price,
                    source.store_id,
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
            store_id,
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
				  s.Store_Id,
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
                temp_storeId varchar(6),
                temp_vendorid varchar(10),
                temp_cost money,
                temp_prfrom datetime,
                temp_prto datetime,
                temp_orderno smallint,
                temp_activevendor int,
                );
    INSERT INTO #TempStockCosts (temp_productid, temp_descript, temp_storeId, temp_vendorid, temp_cost, temp_prfrom, temp_prto, temp_orderno, temp_activevendor)
    SELECT ProductId,
                descript,
                Store_Id,
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
				      s.Store_Id,
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
                storeid,
                vendorid,
                cost,
                startdate,
                enddate,
                orderno,
                activevendor)
    SELECT temp_productid, 
                temp_storeId, 
                temp_vendorid, 
                temp_cost, 
                temp_prfrom, 
                temp_prto, 
                temp_orderno, 
                temp_activevendor 
    FROM 	#TempStockCosts	
    DROP TABLE #TempStockCosts;

    --::::::::::::::: Create Cost Records | Common for all price types --- END :::::::::::::::


    --::::::::::::::: Special Pricing --- START :::::::::::::::
    --Delete Special Prices
    DELETE p
    FROM   [CSCMaster].[dbo].[pricel] p
            LEFT JOIN [CSCTransfer].[dbo].pricedefinition pd
                ON p.stock_code = pd.productid
                    AND p.storeid = pd.store_id
                    AND p.orderno = pd.orderno
                    AND p.vendorid = pd.vendorid
    WHERE  pd.trans_flag = 'O'
            AND pd.pr_type != 'R'

	CREATE TABLE #EntriesWithNoPrice (
	temp_productId VARCHAR(16), 
	temp_storeId VARCHAR(16), 
	temp_vendorId VARCHAR(16), 
	temp_orderNo SMALLINT
	);
	INSERT INTO #EntriesWithNoPrice (temp_productId ,temp_storeId ,temp_vendorId, temp_orderNo)
	SELECT 
	  pd.ProductId, 
	  pd.Store_Id, 
	  pd.VendorId, 
	  pd.OrderNo 
	FROM 
	  [CSCTransfer].[dbo].PriceDefinition pd 
	  LEFT JOIN CSCMaster.[dbo].Stock_prices sp ON sp.OrderNo = pd.OrderNo 
	  and sp.stock_code = pd.ProductId 
	  and sp.StoreID = pd.Store_Id 
	  and sp.vendorid = pd.VendorId 
	WHERE 
	  pd.Trans_Flag = 'O' 
	  AND PR_Type != 'R' 
	  AND sp.Stock_Code IS NULL 
	GROUP BY 
	  pd.ProductId, 
	  pd.VendorId, 
	  pd.Store_Id, 
	  pd.OrderNo


    --Create Price Record
    --Add Stock Price
    INSERT INTO [CSCMaster].[dbo].stock_prices
                (stock_code,
                price_number,
                price,
                margin,
                storeid,
                vendorid,
                startdate,
                enddate,
                orderno)
    SELECT sp.stock_code,
            sp.price_number,
            sp.price,
            sp.margin,
            pd.store_id,
            pd.vendorid,
            pd.pr_from,
            pd.pr_to,
            pd.orderno
    FROM   [CSCTransfer].[dbo].pricedefinition pd
            LEFT JOIN cscmaster.[dbo].stock_prices sp
                ON sp.orderno = 0
                    AND sp.stock_code = pd.productid
                    AND sp.storeid = pd.store_id
                    AND sp.vendorid = pd.vendorid
            LEFT JOIN cscmaster.[dbo].stock_prices spc
                ON spc.stock_code = pd.productid
                    AND spc.storeid = pd.store_id
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
             storeid,
             vendorid,
             startdate,
             enddate,
             orderno)
    SELECT spv.productid,
           nums.Price_Number,
           0,
           0,
           COALESCE(spv.store_id, 'ALL'),
           COALESCE(spv.vendorid, 'ALL'),
           NULL,
           NULL,
           spv.orderno
    FROM   [CSCTransfer].[dbo].[vw_StockPricesOfAllPriceDef] spv
    CROSS APPLY (
           SELECT TOP (@NumberOfPrice) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Price_Number
           FROM sys.columns
    ) AS nums


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
            pcr.store_id,
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
      store_id,
	  vendorId,
	  orderNo,
	  Price_Number,
	  Descript,
      ROW_NUMBER() OVER (PARTITION BY productId, store_id, vendorId, orderNo ORDER BY orderNo) AS rn
    FROM
      [CSCTransfer].[dbo].[vw_PriceChangeRecords]
  ) pcr ON t1.temp_productId = pcr.ProductId  
  AND t1.temp_storeId = pcr.store_id 
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
            pcr.store_id,
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
			store_id,
			vendorid,
			orderno,
			price_number,
			descript,
			price,
			margin,
			startdate,
			enddate,
			row_number() over (partition BY productid, store_id, vendorid, orderno ORDER BY price_number DESC) AS rn
	FROM     [CSCTransfer].[DBO].[vw_PriceChangeRecords] ) pcr ON t1.temp_productid = pcr.productid
	AND t1.temp_storeid = pcr.store_id
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
                [storeid],
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
            store_id,
            vendorid,
            pr_from,
            pr_to,
            orderno
    FROM   (SELECT
      *,
      ROW_NUMBER() OVER (PARTITION BY productId, store_id, orderno, vendorid ORDER BY batchid desc) AS rn
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
    EXEC [CSCTransfer].[dbo].Sproc_performpricechangetransfer;
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
        IF @USE_ARINVOICES = 1
        BEGIN
            INSERT INTO @ResultData (Grade, FullName, DiscountRate, GroupID)
            SELECT Grade, FullName, DiscountRate, GroupID
            FROM CSCPump.dbo.Grade AS Grade
            LEFT JOIN FuelDiscountChart ON FuelDiscountChart.Grade = Grade.ID
            WHERE GroupID = @GroupID AND Store_ID = 'KEYLCK'
            ORDER BY Grade;

            IF NOT EXISTS (SELECT 1 FROM @ResultData)
            BEGIN
                INSERT INTO @ResultData (Grade, FullName, DiscountRate, GroupID)
                SELECT ID AS Grade, FullName, '' AS DiscountRate, '' AS GroupID
                FROM CSCPump.dbo.Grade AS Grade
                WHERE Store_ID = 'KEYLCK'
                ORDER BY Grade;
            END
        END
        ELSE
        BEGIN
            INSERT INTO @ResultData (Grade, FullName, DiscountRate, GroupID)
            SELECT Grade, FullName, DiscountRate, GroupID
            FROM CSCPump.dbo.Grade AS Grade
            LEFT JOIN FuelDiscountChart ON FuelDiscountChart.Grade = Grade.ID
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

-- Update Restriction column (25 to 75 chars) in the Restrictoin Table - ICSCR-500
USE [CSCMaster]
GO

IF (COL_LENGTH('dbo.Restrictions', 'Restriction') <> 75)
BEGIN
    ALTER TABLE dbo.[Restrictions]
    ALTER COLUMN [Restriction] nvarchar(75) null
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

-- Create Function SplitString | ICSCR-659
USE [CSCMaster];
GO

IF OBJECT_ID(N'dbo.SplitString', N'TF') IS NOT NULL
BEGIN
  DROP FUNCTION dbo.SplitString;
END
GO

CREATE FUNCTION dbo.SplitString (
  @String NVARCHAR(MAX),
  @Delimiter CHAR(1)
)
RETURNS @Result TABLE (Value NVARCHAR(MAX))
AS
BEGIN
  DECLARE @Start INT = 1, 
          @End INT

  WHILE @Start <= LEN(@String)
  BEGIN
    SET @End = CHARINDEX(@Delimiter, @String, @Start)
  
    IF @End = 0 
    BEGIN
      SET @End = LEN(@String) + 1
    END
  
    INSERT INTO @Result (Value)
    VALUES (SUBSTRING(@String, @Start, @End - @Start))
    SET @Start = @End + 1
  END
  
  RETURN
END
GO

-- Create SP CheckCreateTables | ICSCR-659
USE [CSCMaster];
GO

IF OBJECT_ID(N'Sproc_CheckCreateTables', N'P') IS NOT NULL
BEGIN
  DROP PROCEDURE [Sproc_CheckCreateTables];
END
GO

CREATE PROCEDURE [Sproc_CheckCreateTables]
  @DatabaseName NVARCHAR(128),
  @MasterTableName NVARCHAR(128),
  @RelatedTables NVARCHAR(MAX),
  @Dts BIT
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @Load BIT = 0,
          @SelectionTable NVARCHAR(128) = @MasterTableName,
          @TableName NVARCHAR(128),
          @Query NVARCHAR(MAX),
          @ExpTableName NVARCHAR(128),
          @ArchExpTableName NVARCHAR(128),
          @CreateTableQuery NVARCHAR(MAX);

  -- cursor to iterate through related tables
  DECLARE @TableCursor CURSOR
  SET @TableCursor = CURSOR FOR
  SELECT VALUE FROM dbo.SplitString(@RelatedTables, ',')
  
  OPEN @TableCursor
  FETCH NEXT FROM @TableCursor INTO @TableName
  
  WHILE @@FETCH_STATUS = 0
  BEGIN
    SET @ExpTableName = 'Exp' + @TableName
    SET @ArchExpTableName = 'ArchExp' + @TableName
  
    -- constructing the create table query
    SET @CreateTableQuery = '
      SELECT TOP 0 *,
        CAST(NULL AS NVARCHAR(10)) AS [StoreID]'

    IF UPPER(@MasterTableName) <> 'PRICEL' OR UPPER(@TableName) = 'SPEXPSTOCKMST'
    BEGIN
  	  SET @CreateTableQuery += '
        ,CAST(NULL AS DATETIME) AS [TrDate]
        ,CAST(NULL AS NVARCHAR(10)) AS [UserCode]
        ,CAST(NULL AS NVARCHAR(2)) AS [Flag]'
    END
  
    IF @Dts = 1
    BEGIN
      SET @CreateTableQuery += '
  	    ,CAST(NULL AS BIT) AS [ReadyForTransferTbl]'
    END
  
    SET @CreateTableQuery += '
      INTO [CSCTransfer].[dbo].[#NEW_TABLE_NAME#]
      FROM [' + @DatabaseName + '].[dbo].[' + @TableName + '];'
  
    -- check if the Exp table already exists
    IF NOT EXISTS (SELECT 1 FROM CSCTransfer.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @ExpTableName)
    BEGIN
      SET @Query = REPLACE(@CreateTableQuery, '#NEW_TABLE_NAME#', @ExpTableName)
      EXEC sp_executesql @Query
    END
    ELSE
    BEGIN
      -- logic for existing ExpTable
  	  SET @Query = 'IF EXISTS (SELECT 1 FROM [CSCTransfer].[dbo].[' + @ExpTableName + '] #WHERE_CONDITION#) SELECT @Load = 1, @SelectionTable = ''' + @ExpTableName + '''';
  
      IF @Dts = 1 AND UPPER(@TableName) = UPPER(@MasterTableName)
      BEGIN
  	  	SET @Query = REPLACE(@Query, '#WHERE_CONDITION#', 'WHERE [ReadyForTransferTbl] IS NULL');
      END
      ELSE IF @Dts = 0
      BEGIN
        SET @Query = REPLACE(@Query, '#WHERE_CONDITION#', '');
      END
  
  	  EXEC sp_executesql @Query, N'@Load BIT OUTPUT, @SelectionTable NVARCHAR(128) OUTPUT', @Load OUTPUT, @SelectionTable OUTPUT
    END
  
    -- check if the ArchExp table already exists
    IF NOT EXISTS (SELECT 1 FROM CSCTransfer.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @ArchExpTableName)
    BEGIN
      SET @Query = REPLACE(@CreateTableQuery, '#NEW_TABLE_NAME#', @ArchExpTableName)
      EXEC sp_executesql @Query
    END
  
    FETCH NEXT FROM @TableCursor INTO @TableName
  END
  
  CLOSE @TableCursor
  DEALLOCATE @TableCursor

  -- returning results
  SELECT @Load AS [Load], @SelectionTable AS [SelectionTable]
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
    Where price_number = @PriceNum and orderno = @OrderNo and StoreID = @StoreID
  else
    Select *  from Stock_prices
    left join @StockCodeList sc on stock_code = sc.StockCode and Stock_prices.vendorid = sc.VendorID
    Where orderno = @OrderNo and StoreID = @StoreID
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
                     WHERE StockTax.stock_code in (SELECT StockCode FROM @StockCodeList) and TaxMast.Tax_Active = 1 and Storeid = @StoreID
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
             Where STOCK_AC.STOCK_CODE in (SELECT StockCode FROM @StockCodeList) and Storeid = @StoreID order by STOCK_AC.AS_CODE
END
GO

-- Add TransmissionID column
USE [CSCPump]

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[FuelPrice_HO]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
AND NOT EXISTS (SELECT * FROM  syscolumns WHERE  (name = 'TransmissionID') AND (id = OBJECT_ID(N'[dbo].[FuelPrice_HO]')))
BEGIN
  ALTER TABLE [dbo].[FuelPrice_HO] ADD [TransmissionID] [varchar](25) NULL 
END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[FuelPrice]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
AND NOT EXISTS (SELECT * FROM  syscolumns WHERE  (name = 'TransmissionID') AND (id = OBJECT_ID(N'[dbo].[FuelPrice]')))
BEGIN
	ALTER TABLE [dbo].[FuelPrice] ADD [TransmissionID] [varchar](25) NULL 
END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[FuelPriceHist]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
AND NOT EXISTS (SELECT * FROM  syscolumns WHERE  (name = 'TransmissionID') AND (id = OBJECT_ID(N'[dbo].[FuelPriceHist]')))
BEGIN
	ALTER TABLE [dbo].[FuelPriceHist] ADD [TransmissionID] [varchar](25) NULL 
END

GO