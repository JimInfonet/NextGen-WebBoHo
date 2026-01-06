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

--Jim added on Nov.13,2025
USE [CSCAdmin]
GO

--Adding GID(Group ID) to REPGROUP
UPDATE  RG
SET RG.GID = GN.GID
FROM [dbo].[REPGROUP] RG
JOIN (
SELECT ROW_NUMBER() OVER (ORDER BY [GROUP_NAME] ASC) AS GID, [GROUP_NAME]
FROM [REPGROUP]) GN on RG.GROUP_NAME = GN.GROUP_NAME 
GO

--Adding GID(Group ID) to Reports_in_Groups
UPDATE  RG
SET RG.GID = RG.GID
FROM [dbo].[Reports_in_Groups] RG
JOIN (SELECT ROW_NUMBER() OVER (ORDER BY [GROUP_NAME] ASC) AS GID, [GROUP_NAME]
FROM [REPGROUP]) GN on RG.GROUP_NAME = GN.GROUP_NAME 
GO

--Adding Web to Reports
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = N'Web'
      AND Object_ID = Object_ID(N'dbo.REPORTS')
)
BEGIN
    ALTER TABLE [dbo].[REPORTS]
    ADD [Web] bit NOT NULL DEFAULT(0);
END

--Adding Reports and Reports_in_Groups
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'AC2019')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'AC2019', N'Tax Sales Detail', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'AC4040')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'AC4040', N'Credit Card Reconciliation', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'AC5010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'AC5010', N'Third Party Card', 0, 1, N'ThCardPayments', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'AR0050')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'AR0050', N'AR Customer Information', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'AR0120')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'AR0120', N'AR Payment History', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'FL0100')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'FL0100', N'Fuel Price Change', 0, 0, N'', 0, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'FL0100GR')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'FL0100GR', N'Fuel Price Change - Chart', 0, 2, N'', 0, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'PL1010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'PL1010', N'Tender Definitions', 0, 1, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'PL1020')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'PL1020', N'Policy Settings', 0, 1, N'', 0, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'PT0100')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'PT0100', N'Price Change Tracking', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA1030')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA1030', N'Special Pricing', 0, 1, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA1040')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA1040', N'Payout Journal', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA1050')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA1050', N'Cash Draw', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA2319')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA2319', N'Refunds By Customer', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA2379')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA2379', N'Markdown Items', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7010', N'Daily Sales Report', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SA7010')
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SA7010', 1, N'SaleHead', N'ShiftDate', N'Shift Date', N'=', N'D', NULL, NULL)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7012')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7012', N'Margin By Stock Item & Selling Price', 0, 2, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7013')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7013', N'Department Sales By Date (Detail)', 0, 0, N'', 0, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7016')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7016 ', N'Department Sales By Date (Gross & Net)', 0, 2, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7016GR')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7016GR', N'Department Sales By Date (Gross & Net)  - Chart', 0, 2, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7019')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7019', N'Up Sell Sales Report', 0, 0, NULL, 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7062')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7062', N'Contribution Margin By Department', 1, 2, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7062GR')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7062GR', N'Contribution Margin By Department - Graphical', 1, 1, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SA7070')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7070', N'Voided & Deleted Sales', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK1010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK1010', N'Department Listing', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK1011')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK1011 ', N'Department / Sub-Dept Listing', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK1070')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK1070', N'Tax Codes & Rates', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK2010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK2010', N'Stock Master Listing', 0, 2, N'', 0, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK2011')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK2011', N'Inventory Levels', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK2012')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK2012', N'Reorder Levels', 0, 1, NULL, 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK2015')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK2015', N'Stock Items Detail Report', 0, 1, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK2020')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK2020', N'Kit Components', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK2030')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK2030', N'Promotional Items', 0, 2, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK3010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK3010', N'Standard Price List(1 Column)', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK3011')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK3011', N'Standard Price List( Multiple Columns)', 0, 2, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK4010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK4010', N'Stock Count Listing', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK4020')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK4020', N'Stock Movement Summary', 0, 0, NULL, 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK4030')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK4030', N'No Stock Movement Report', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK4040')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK4040', N'Stock- Zero Movement', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK4045')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK4045', N'Stock Valuation by Department', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK5010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK5010', N'Stock Receiving History', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK5011')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK5011', N'Stock Takes History', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK5020')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK5020', N'Historical Inventory Level Report', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK5030')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK5030', N'ShelfLabelEAN13L', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SK5040')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK5040', N'ShelfLabelCode39', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SL0200')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SL0200 ', N'Top Sellers - Total $ Amount', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SL0200GR')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SL0200GR', N'Top Sellers - Total $ Amount - Chart', 0, 2, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SL0202')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SL0202', N'Top Sellers - Margin $', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SL0202GR')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SL0202GR', N'Top Sellers - Margin $ - Chart', 0, 2, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SL0204')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SL0204', N'Quantity Leaders Sales By Date', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SL0205')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SL0205', N'Hourly Sales Report', 1, 2, NULL, 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SS9010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SS9010', N'User Id Listing', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SS9020')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SS9020', N'User Group Listing', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'SS9050')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SS9050 ', N'User Group / Incumbent Listing', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'TE4045')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'TE4045', N'Stock Valuation by Department', 1, 2, NULL, 1, 1)
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[REPORTS] WHERE [Report_Name] = 'WK0100')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'WK0100 ', N'Sales by Dept - Last 6 Weeks', 0, 0, N'', 1, 1)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AC2019')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AC2019'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'AC2019', N'Tax', 13)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AC4040')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AC4040'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'AC4040', N'Tenders', 14)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AC5010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AC5010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'AC5010', N'Tenders', 14)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AR0050')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AR0050'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'AR0050', N'AR Customers', 1)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AR0120')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'AR0120'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'AR0120', N'AR Customers', 1)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'FL0100')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'FL0100'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'FL0100', N'Pricing', 9)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'FL0100GR')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'FL0100GR'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'FL0100GR', N'Graphical Report', 4)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'PL1010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'PL1010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'PL1010', N'Tenders', 14)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'PL1020')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'PL1020'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'PL1020', N'Policies', 8)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'PT0100')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'PT0100'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'PT0100', N'Pricing', 9)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA1030')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA1030'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA1030', N'Pricing', 9)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA1040')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA1040'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA1040', N'Tenders', 14)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA1050')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA1050'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA1050', N'Tenders', 14)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA2319')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA2319'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA2319', N'Tenders', 14)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA2379')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA2379'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA2379', N'Tenders', 14)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7010', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7012')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7012'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7012', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7013')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7013'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7013', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7016')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7016'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7016 ', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7016GR')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7016GR'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7016GR', N'Graphical Report', 4)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7019')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7019'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7019', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7062')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7062'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7062', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7062GR')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7062GR'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7062GR', N'Graphical Report', 4)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7070')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SA7070'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SA7070', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK1010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK1010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK1010', N'Departments', 2)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK1011')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK1011'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK1011 ', N'Departments', 2)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK1070')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK1070'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK1070', N'Tax', 13)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK2010', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2011')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2011'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK2011', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2012')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2012'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK2012', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2015')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2015'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK2015', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2020')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2020'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK2020', N'Kits', 6)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2030')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK2030'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK2030', N'Kits', 6)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK3010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK3010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK3010', N'Pricing', 9)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK3011')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK3011'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK3011', N'Pricing', 9)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK4010', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4020')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4020'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK4020', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4030')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4030'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK4030', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4040')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4040'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK4040', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4045')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK4045'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK4045', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK5010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK5010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK5010', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK5011')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK5011'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK5011', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK5020')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK5020'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK5020', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK5030')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SK5030'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SK5030', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'Sk5040')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'Sk5040'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'Sk5040', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0200')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0200'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SL0200 ', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0200GR')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0200GR'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SL0200GR', N'Graphical Report', 4)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0202')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0202'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SL0202', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0202GR')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0202GR'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SL0202GR', N'Graphical Report', 4)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0204')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0204'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SL0204', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0205')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SL0205'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SL0205', N'Sales Information', 11)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SS9010')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SS9010'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SS9010', N'Security', 12)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SS9020')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SS9020'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SS9020', N'Security', 12)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SS9050')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'SS9050'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'SS9050 ', N'Security', 12)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'TE4045')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'TE4045'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'TE4045', N'Inventory', 5)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'WK0100')
DELETE FROM [dbo].[Reports_in_Groups] WHERE [Report_Name] = 'WK0100'
INSERT [dbo].[Reports_in_Groups] ([Report_Name], [Group_Name], [GID]) VALUES (N'WK0100 ', N'Sales Information', 11)

--Adding report filter
IF EXISTS (SELECT 1 FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK4020')
DELETE FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK4020'
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK4020', 1, N'SaleHead', N'Sale_Date', N'Sale_Date', N'Range', N'D', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK4020', 2, N'StockMst', N'Dept', N'Department', N'Range', N'T', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK4020', 3, N'StockMst', N'sub_Dept', N'Sub Department', N'Range', N'T', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK4020', 4, N'StockMst', N'Sub_Detail', N'Sub Detail', N'Range', N'T', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK4020', 5, N'StockMst', N'Stock_Code', N'Stock Code', N'Range', N'T', NULL, NULL)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK4030')
DELETE FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK4030'
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK4030', 1, N'SaleHead', N'Sale_Date', N'Sale_Date', N'Range', N'D', NULL, NULL)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK5010')
DELETE FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK5010'
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5010', 1, N'StockHist', N'BatchDate', N'Batch Date', N'Range', N'D', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5010', 2, N'StockHist', N'StockCode', N'Stock Code', N'Range', N'T', NULL, NULL)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK5020')
DELETE FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK5020'
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5020', 1, N'StockHist', N'BatchDate', N'Batch Date', N'=', N'D', NULL, NULL)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK5030')
DELETE FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK5030'
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5030', 1, N'StockMst', N'Dept', N'Department', N'=', N'T', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5030', 2, N'StockMst', N'Sub_Dept', N'Sub Department', N'=', N'T', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5030', 3, N'StockMst', N'Vendor', N'Vendor', N'=', N'T', NULL, NULL)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK5040')
DELETE FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'SK5040'
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5040', 1, N'StockMst', N'Dept', N'Department', N'=', N'T', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5040', 2, N'StockMst', N'Sub_Dept', N'Sub Department', N'=', N'T', NULL, NULL)
GO
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'SK5040', 3, N'StockMst', N'Vendor', N'Vendor', N'=', N'T', NULL, NULL)
GO
IF EXISTS (SELECT 1 FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'AC4040')
DELETE FROM [dbo].[Report_Filter] WHERE [Report_Name] = 'AC4040'
INSERT [dbo].[Report_Filter] ([Report_Name], [Filter_Seq], [Table_name], [Field], [Field_Desc], [Type], [Field_Type], [Forced], [GroupBy]) VALUES (N'AC4040', 1, N'SaleHead', N'Sale_Date', N'Sale Date', N'Range', N'D', NULL, NULL)
GO


--Adding Web to Reports
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[Reports]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
AND NOT EXISTS (SELECT  * FROM  syscolumns
WHERE  (name = 'Web') AND (id = OBJECT_ID(N'[dbo].[Reports]')))
BEGIN
	ALTER  TABLE [dbo].[Reports] ADD [Web] [bit] NULL
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[webReports]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[webReports]
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[webReports]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
CREATE TABLE [dbo].[webReports] (
	[Report_Name] [nvarchar](12) NOT NULL
) ON [PRIMARY]
GO

INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'AC2019')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'AC4040')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'AC5010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'AR0050')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'AR0120')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'FL0100')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'FL0100GR')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'PL1010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'PL1020')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'PT0100')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA1030')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA1040')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA1050')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA2319')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA2379')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7012')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7013')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7016 ')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7016GR')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7019')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7062')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7062GR')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SA7070')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK1010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK1011 ')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK1070')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK2010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK2011')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK2012')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK2015')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK2020')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK2030')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK3010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK3011')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK4010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK4020')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK4030')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK4040')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK4045')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK5010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK5011')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK5020')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK5030')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SK5040')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SL0200 ')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SL0200GR')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SL0202')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SL0202GR')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SL0204')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SL0205')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SS9010')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SS9020')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'SS9050 ')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'TE4045')
GO
INSERT [dbo].[webReports] ([Report_Name]) VALUES (N'WK0100 ')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[REPORTS] WHERE report_name='SA7010')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SA7010', N'Daily Sales Report', 0, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT * FROM [dbo].[REPORTS] WHERE report_name='SK4020')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK4020', N'Stock Movement Summary', 0, 0, NULL, 1, 1)
GO
IF NOT EXISTS (SELECT * FROM [dbo].[REPORTS] WHERE report_name='SK4030')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK4030', N'No Stock Movement Report', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT * FROM [dbo].[REPORTS] WHERE report_name='SK5020')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK5020', N'Historical Inventory Level Report', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT * FROM [dbo].[REPORTS] WHERE report_name='SK5030')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK5030', N'ShelfLabelEAN13L', 1, 0, N'', 1, 1)
GO
IF NOT EXISTS (SELECT * FROM [dbo].[REPORTS] WHERE report_name='SK5040')
INSERT [dbo].[REPORTS] ([Report_Name], [Report_Title], [Detail_Level], [print_orientation], [SubReport_Name], [EnhancedReport], [Web]) VALUES (N'SK5040', N'ShelfLabelCode39', 1, 0, N'', 1, 1)
GO

-- Update Web column in Reports based on webReports
UPDATE R
Set R.Web = 1
FROM [CSCAdmin].[dbo].[Reports] R, [CSCAdmin].[dbo].[webReports] wR 
WHERE R.Report_Name = wR.Report_Name

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[webReports]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[webReports]

USE [CSCTrans]
GO

/****** Object:  StoredProcedure [dbo].[Sproc_AC2019]    Script Date: 2023-07-31 11:18:28 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_AC2019')
DROP PROCEDURE Sproc_AC2019
go
CREATE PROCEDURE [dbo].[Sproc_AC2019]
	@date1		DATETIME,
	@date2		DATETIME
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	SET NOCOUNT ON;
	
	SELECT S_SaleTax.Tax_Name , SUM(S_SaleTax.Taxable_Amount) as Taxable_Amount,SUM(S_SaleTax.Tax_Added_Amount) AS Tax_Added_Amount , sum(S_SaleTax.Tax_Included_Amount) as Tax_Included_Amount , sum(S_SaleTax.Tax_Added_Amount+S_SaleTax.Tax_Included_Amount) as TotalTax
		FROM 
			[CSCTrans].[dbo].[S_SaleTax]
		INNER JOIN 
			[CSCTrans].[dbo].[SALEHEAD]
		ON 
			SALEHEAD.SALE_NO = S_SaleTax.SALE_NO AND SALEHEAD.TILL = S_SaleTax.TILL_NUM
		WHERE 
			SALEHEAD.T_TYPE in ('PATP_APP', 'REFUND', 'SALE')
			AND SALEHEAD.ShiftDate BETWEEN @date1 AND @date2
		GROUP BY S_SaleTax.Tax_Name , S_SaleTax.Tax_Name
END
GO



-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_AC4040')
DROP PROCEDURE Sproc_AC4040
go
CREATE PROCEDURE [dbo].[Sproc_AC4040]
	@date1		DATETIME,
	@date2		DATETIME
AS

BEGIN
	SET NOCOUNT ON;

	SELECT
		SALETEND."SALE_NO",
		SALETEND."TENDNAME",
		SALETEND."TENDCLAS",
		SALETEND."AMTTEND",
		SALEHEAD."TILL",
		SALEHEAD."SALE_DATE",
		SALEHEAD."T_TYPE"
	FROM
		CSCTrans.dbo.SALETEND SALETEND INNER JOIN CSCTrans.dbo.SALEHEAD SALEHEAD ON
			SALETEND."SALE_NO" = SALEHEAD."SALE_NO"
	WHERE
		(SALETEND."TENDCLAS" = 'CRCARD' OR
		SALETEND."TENDCLAS" = 'DBCARD') AND
		SALEHEAD.ShiftDate Between @date1 and @date2
	ORDER BY
		SALETEND."TENDNAME" ASC	
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_AR0050')
DROP PROCEDURE Sproc_AR0050
go
CREATE PROCEDURE [dbo].[Sproc_AR0050] 
	-- Add the parameters for the stored procedure here
	--<@Param1, sysname, @p1> <Datatype_For_Param1, , int> = <Default_Value_For_Param1, , 0>, 
	--<@Param2, sysname, @p2> <Datatype_For_Param2, , int> = <Default_Value_For_Param2, , 0>
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		CLIENT."CL_CODE",
		CLIENT."CL_NAME",
		CLIENT."CL_ADD1",
		CLIENT."CL_ADD2",
		CLIENT."CL_CITY",
		CLIENT."CL_PROV",
		CLIENT."CL_PHONE",
		CLIENT."CL_FAX",
		CLIENT."CL_CONT1",
		CLIENT."CL_CONT2",
		CLIENT."CL_ARCUST",
		CLIENT."PRICE_CODE",
		CLIENT."CL_LIMIT",
		CLIENT."CL_CURBAL",
		CLIENT."CL_STATUS",
		CLIENT."PARENT_ACCT"
	FROM
		CSCMaster.dbo.CLIENT CLIENT
	WHERE
		CLIENT."CL_ARCUST" = 1
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_AR0120')
DROP PROCEDURE Sproc_AR0120
go
CREATE PROCEDURE [dbo].[Sproc_AR0120] 
	-- Add the parameters for the stored procedure here
	@date1		DATETIME,
	@date2		DATETIME,
	@cust1		NVARCHAR(10),
	@cust2		NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @cust1 = 'All' or @cust2 = 'All'
		SELECT
			CLIENT."CL_NAME",
			CLIENT."CL_CURBAL",
			PAYMENT."PY_CLIENT",
			PAYMENT."PY_NUM",
			PAYMENT."PY_DATE",
			SALEHEAD."SALE_NO",
			SALEHEAD."ShiftDate",
			SALEHEAD."CHANGE",
			SALETEND."TENDNAME",
			SALETEND."AMTUSED"
		FROM
			CSCTrans.dbo.PAYMENT PAYMENT 
			inner join CSCMaster.dbo.CLIENT CLIENT ON CLIENT."CL_CODE" = PAYMENT."PY_CLIENT" 
			inner join CSCTrans.dbo.salehead salehead on salehead.sale_no=payment.sale_num 
			inner join csctrans.dbo.saletend saletend on saletend.sale_no=salehead.sale_no
		WHERE Payment.PY_DATE  BETWEEN @date1 AND @date2
	else
		SELECT
			CLIENT."CL_NAME",
			CLIENT."CL_CURBAL",
			PAYMENT."PY_CLIENT",
			PAYMENT."PY_NUM",
			PAYMENT."PY_DATE",
			SALEHEAD."SALE_NO",
			SALEHEAD."ShiftDate",
			SALEHEAD."CHANGE",
			SALETEND."TENDNAME",
			SALETEND."AMTUSED"
		FROM
			CSCTrans.dbo.PAYMENT PAYMENT 
			inner join CSCMaster.dbo.CLIENT CLIENT ON CLIENT."CL_CODE" = PAYMENT."PY_CLIENT" 
			inner join CSCTrans.dbo.salehead salehead on salehead.sale_no=payment.sale_num 
			inner join csctrans.dbo.saletend saletend on saletend.sale_no=salehead.sale_no
		WHERE Payment.PY_DATE  BETWEEN @date1 AND @date2
			AND CLIENT.CL_NAME BETWEEN @cust1 AND @cust2
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_FL0100')
DROP PROCEDURE Sproc_FL0100
go
CREATE PROCEDURE [dbo].[Sproc_FL0100] 
	-- Add the parameters for the stored procedure here
	@date1 DATETIME,
	@date2 DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	DECLARE @STARTDATE DATETIME
	DECLARE @ENDDATE DATETIME
	SET NOCOUNT ON;
	SET @STARTDATE = CONVERT(DATE, @date1)
	SET @ENDDATE = CONVERT(DATE, @date2)

	SELECT
		FuelPriceHist."CashPrice",
		FuelPriceHist."CreditPrice",
		FuelPriceHist."emplID",
		FuelPriceHist."Date_Time",
		Grade."ShortName",
		ServiceLevel."Description",
		Tier."Description"
	FROM
		((CSCPump.dbo.FuelPriceHist FuelPriceHist INNER JOIN CSCPump.dbo.Grade Grade ON
			FuelPriceHist."gradeID" = Grade."ID")
		INNER JOIN CSCPump.dbo.ServiceLevel ServiceLevel ON
			FuelPriceHist."levelID" = ServiceLevel."ID")
		INNER JOIN CSCPump.dbo.Tier Tier ON
			FuelPriceHist."tierID" = Tier."ID"
	WHERE FuelPriceHist.Date_Time BETWEEN @STARTDATE AND @ENDDATE
	ORDER BY
		FuelPriceHist."Date_Time" DESC,
		FuelPriceHist."emplID" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_PL1010')
DROP PROCEDURE Sproc_PL1010
go
CREATE PROCEDURE [dbo].[Sproc_PL1010] 
	-- Add the parameters for the stored procedure here
	@class1 NVARCHAR(10),
	@class2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @class1 = 'All'	
	SELECT
		TENDMAST."TENDCODE",
		TENDMAST."TENDDESC",
		TENDMAST."TENDCLASS",
		TENDMAST."DISPLAYSEQ",
		TENDMAST."EXACTCHG",
		TENDMAST."GIVECHG",
		TENDMAST."AcceptAsPayment",
		TENDMAST."GIVEASREF",
		TENDMAST."MAXAMOUNT",
		TENDMAST."MINAMOUNT",
		TENDMAST."EXCHANGE",
		TENDMAST."SMALLUNIT",
		TENDMAST."BANKABLE",
		TENDMAST."IncludeInClose",
		TENDMAST."INSTANCES",
		TENDMAST."Put_In_Drawer",
		TENDMAST."PrintCopies"
	FROM
		CSCMaster.dbo.TENDMAST TENDMAST
	ORDER BY
		TENDMAST."TENDCLASS" ASC
else
	SELECT
		TENDMAST."TENDCODE",
		TENDMAST."TENDDESC",
		TENDMAST."TENDCLASS",
		TENDMAST."DISPLAYSEQ",
		TENDMAST."EXACTCHG",
		TENDMAST."GIVECHG",
		TENDMAST."AcceptAsPayment",
		TENDMAST."GIVEASREF",
		TENDMAST."MAXAMOUNT",
		TENDMAST."MINAMOUNT",
		TENDMAST."EXCHANGE",
		TENDMAST."SMALLUNIT",
		TENDMAST."BANKABLE",
		TENDMAST."IncludeInClose",
		TENDMAST."INSTANCES",
		TENDMAST."Put_In_Drawer",
		TENDMAST."PrintCopies"
	FROM
		CSCMaster.dbo.TENDMAST TENDMAST
	WHERE TENDMAST."TENDCLASS" between @class1 and @class2
	ORDER BY
		TENDMAST."TENDCLASS" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_PL1020')
DROP PROCEDURE Sproc_PL1020
go
CREATE PROCEDURE [dbo].[Sproc_PL1020] 
	-- Add the parameters for the stored procedure here
	--@dept1 NVARCHAR(10),
	--@dept2 NVARCHAR(10),
	--@subdept1 NVARCHAR(10),
	--@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	@class1 NVARCHAR(10),
	@class2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


if @class1 = 'All'	
	SELECT
		P_COMP."P_CLASS",
		P_COMP."P_NAME",
		P_COMP."P_DESC",
		P_COMP."P_LEVELS",
		P_COMP."P_CHOICES",
		P_COMP."P_SET",
		P_SET."P_NAME",
		P_SET."P_LEVEL",
		P_SET."P_VALUE",
		P_SET."P_SET",
		P_CLASS."P_CL_NAME"
	FROM
		(CSCAdmin.dbo.P_COMP P_COMP 
		LEFT OUTER JOIN CSCAdmin.dbo.P_SET P_SET ON P_COMP."P_NAME" = P_SET."P_NAME") 
		INNER JOIN CSCAdmin.dbo.P_CLASS P_CLASS ON P_COMP."P_CLASS" = P_CLASS."P_CLASS"
	ORDER BY
		P_COMP."P_CLASS" ASC, P_COMP."P_NAME" ASC
else
	SELECT
		P_COMP."P_CLASS",
		P_COMP."P_NAME",
		P_COMP."P_DESC",
		P_COMP."P_LEVELS",
		P_COMP."P_CHOICES",
		P_COMP."P_SET",
		P_SET."P_NAME",
		P_SET."P_LEVEL",
		P_SET."P_VALUE",
		P_SET."P_SET",
		P_CLASS."P_CL_NAME"
	FROM
		(CSCAdmin.dbo.P_COMP P_COMP 
		LEFT OUTER JOIN CSCAdmin.dbo.P_SET P_SET ON P_COMP."P_NAME" = P_SET."P_NAME") 
		INNER JOIN CSCAdmin.dbo.P_CLASS P_CLASS ON P_COMP."P_CLASS" = P_CLASS."P_CLASS"
		AND P_CLASS."P_CLASS" Between @class1 and @class2
	ORDER BY
		P_COMP."P_CLASS" ASC, P_COMP."P_NAME" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_PT0100')
DROP PROCEDURE Sproc_PT0100
go
CREATE PROCEDURE [dbo].[Sproc_PT0100] 
	-- Add the parameters for the stored procedure here
	@emp1 NVARCHAR(10),
	@emp2 NVARCHAR(10),
	@date1 DATETIME,
	@date2 DATETIME,
	@stock1 NVARCHAR(10),
	@stock2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


if @stock1 = 'All'	
	SELECT
		PriceChange."STOCK_CODE",
		PriceChange."EmployeeID",
		PriceChange."PriceNum",
		PriceChange."OriginalPrice",
		PriceChange."NewPrice",
		PriceChange."Date",
		PriceChange."Time",
		PriceChange."Process",
		PriceChange."Row_ID",
		STOCKMST."DESCRIPT",
		A."U_NAME",
		Vendors."Name"
	FROM
		((CSCTrans.dbo.PriceChange PriceChange 
		LEFT OUTER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON PriceChange."STOCK_CODE" = STOCKMST."STOCK_CODE") 
		LEFT OUTER JOIN CSCMaster.dbo.Vendors Vendors ON PriceChange.VendorID=Vendors.Code) 
		LEFT OUTER JOIN CSCAdmin.dbo.[USER] AS A ON PriceChange."EmployeeID" = A."U_CODE"
	WHERE PriceChange.Date Between @date1 and @date2
	ORDER BY
		PriceChange."EmployeeID" ASC
else
	SELECT
		PriceChange."STOCK_CODE",
		PriceChange."EmployeeID",
		PriceChange."PriceNum",
		PriceChange."OriginalPrice",
		PriceChange."NewPrice",
		PriceChange."Date",
		PriceChange."Time",
		PriceChange."Process",
		PriceChange."Row_ID",
		STOCKMST."DESCRIPT",
		A."U_NAME",
		Vendors."Name"
	FROM
		((CSCTrans.dbo.PriceChange PriceChange 
		LEFT OUTER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON PriceChange."STOCK_CODE" = STOCKMST."STOCK_CODE") 
		LEFT OUTER JOIN CSCMaster.dbo.Vendors Vendors ON PriceChange.VendorID=Vendors.Code) 
		LEFT OUTER JOIN CSCAdmin.dbo.[USER] AS A ON PriceChange."EmployeeID" = A."U_CODE"
	WHERE PriceChange.Date Between @date1 and @date2
	ORDER BY
		PriceChange."EmployeeID" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA1030')
DROP PROCEDURE Sproc_SA1030
go
CREATE PROCEDURE [dbo].[Sproc_SA1030] 
	-- Add the parameters for the stored procedure here
	--@dept1 NVARCHAR(10),
	--@dept2 NVARCHAR(10),
	--@subdept1 NVARCHAR(10),
	--@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	@stock1 NVARCHAR(10),
	@stock2 NVARCHAR(10)
AS
BEGIN
	DECLARE @ordno As Integer
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @stock1 = 'All'	
	SELECT
		STOCKMST."STOCK_CODE",
		STOCKMST."DESCRIPT",
		STOCKMST."Vendor",
		--STOCKMST."PR_TYPE",
		Stock_Prices."Price",
		Stock_Prices."VendorID",
		PRICEL."PR_F_QTY",
		PRICEL."PR_T_QTY",
		PRICEL."PRICE",
		PRICEL."Pr_Type",
		PRICEL."Pr_Units",
		PRICEL."StartDate",
		PRICEL."EndDate",
		Vendors."Name"
	FROM
		((CSCMaster.dbo.STOCKMST STOCKMST 
		INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
		INNER JOIN CSCMaster.dbo.PRICEL PRICEL ON STOCKMST."STOCK_CODE" = PRICEL."STOCK_CODE" and (Stock_prices.vendorid = pricel.vendorid or (stock_prices.vendorid is null and pricel.vendorid is null))) 
		LEFT JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
	WHERE
		stock_prices.price_number = 1 and stockmst.pr_type <> 'R' and PriceL.OrderNo = 0 and Stock_Prices.Orderno = 0
	ORDER BY
		STOCKMST."STOCK_CODE" ASC, Stock_Prices."VendorID" ASC
else
	SELECT
		STOCKMST."STOCK_CODE",
		STOCKMST."DESCRIPT",
		STOCKMST."Vendor",
		--STOCKMST."PR_TYPE",
		Stock_Prices."Price",
		Stock_Prices."VendorID",
		PRICEL."PR_F_QTY",
		PRICEL."PR_T_QTY",
		PRICEL."PRICE",
		PRICEL."Pr_Type",
		PRICEL."Pr_Units",
		PRICEL."StartDate",
		PRICEL."EndDate",
		Vendors."Name"
	FROM
		((CSCMaster.dbo.STOCKMST STOCKMST 
		INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
		INNER JOIN CSCMaster.dbo.PRICEL PRICEL ON STOCKMST."STOCK_CODE" = PRICEL."STOCK_CODE" and (Stock_prices.vendorid = pricel.vendorid or (stock_prices.vendorid is null and pricel.vendorid is null))) 
		LEFT JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
	WHERE
		stock_prices.price_number = 1 and stockmst.pr_type <> 'R' and PriceL.OrderNo = 0 and Stock_Prices.Orderno = 0
		and STOCKMST."STOCK_CODE" between @stock1 and @stock2
	ORDER BY
		STOCKMST."STOCK_CODE" ASC, Stock_Prices."VendorID" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA1040')
DROP PROCEDURE Sproc_SA1040
go
CREATE PROCEDURE [dbo].[Sproc_SA1040] 
	-- Add the parameters for the stored procedure here
	@date1 DATETIME,
	@date2 DATETIME,
	@vendor1 NVARCHAR(10),
	@vendor2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

if @vendor1 = 'All'	
	SELECT
		SALEHEAD."SALE_NO",
		SALEHEAD."USER",
		SALEHEAD."ShiftDate",
		SALEHEAD."T_TYPE",
		SALEHEAD."PayOut",
		SALEHEAD."Reason",
		Reasons."Reason",
		SaleVendors."Vendor",
		S_SaleTax."Tax_Name",
		S_SaleTax."Tax_Included_Amount",
		Vendors."Name"
	FROM
		(((CSCTrans.dbo.SALEHEAD SALEHEAD LEFT JOIN CSCMaster.dbo.Reasons Reasons ON SALEHEAD."Reason" = Reasons."Code" AND SALEHEAD."Reason_Type" = Reasons."Type" ) LEFT JOIN CSCTrans.dbo.SaleVendors SaleVendors ON SALEHEAD."SALE_NO" = SaleVendors."Sale_No") LEFT JOIN CSCTrans.dbo.S_SaleTax S_SaleTax ON SALEHEAD."SALE_NO" = S_SaleTax."Sale_No") LEFT JOIN CSCMaster.dbo.Vendors Vendors ON SaleVendors."Vendor" = Vendors."Code" 
	WHERE SALEHEAD."T_TYPE" = 'PAYOUT'
		AND SALEHEAD.SHIFTDATE between @date1 and @date2

	ORDER BY
		SALEHEAD."ShiftDate" ASC, SaleVendors."Vendor" ASC, SALEHEAD."Reason" ASC, SALEHEAD."SALE_NO" ASC
else
	SELECT
		SALEHEAD."SALE_NO",
		SALEHEAD."USER",
		SALEHEAD."ShiftDate",
		SALEHEAD."T_TYPE",
		SALEHEAD."PayOut",
		SALEHEAD."Reason",
		Reasons."Reason",
		SaleVendors."Vendor",
		S_SaleTax."Tax_Name",
		S_SaleTax."Tax_Included_Amount",
		Vendors."Name"
	FROM
		(((CSCTrans.dbo.SALEHEAD SALEHEAD LEFT JOIN CSCMaster.dbo.Reasons Reasons ON SALEHEAD."Reason" = Reasons."Code" AND SALEHEAD."Reason_Type" = Reasons."Type" ) LEFT JOIN CSCTrans.dbo.SaleVendors SaleVendors ON SALEHEAD."SALE_NO" = SaleVendors."Sale_No") LEFT JOIN CSCTrans.dbo.S_SaleTax S_SaleTax ON SALEHEAD."SALE_NO" = S_SaleTax."Sale_No") LEFT JOIN CSCMaster.dbo.Vendors Vendors ON SaleVendors."Vendor" = Vendors."Code" 
	WHERE SALEHEAD."T_TYPE" = 'PAYOUT' and Vendors.Code Between @vendor1 and @vendor2
		AND SALEHEAD.SHIFTDATE between @date1 and @date2

	ORDER BY
		SALEHEAD."ShiftDate" ASC, SaleVendors."Vendor" ASC, SALEHEAD."Reason" ASC, SALEHEAD."SALE_NO" ASC

END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA1050')
DROP PROCEDURE Sproc_SA1050
go
CREATE PROCEDURE [dbo].[Sproc_SA1050] 
	-- Add the parameters for the stored procedure here
	@date1 DATETIME,
	@date2 DATETIME,
	@till1 NVARCHAR(10),
	@till2 NVARCHAR(10)
AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @till_number1 integer
	DECLARE @till_number2 integer

	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

if @till1 = 'All'	
	SELECT
		CashDraw."Draw_Date",
		CashDraw."User",
		CashDraw."Till",
		CashDraw."Amount",
		CashDraw."Reason",
		Reasons."Reason"
	FROM
		CSCTrans.dbo.CashDraw CashDraw 
		LEFT JOIN CSCMaster.dbo.Reasons Reasons ON CashDraw."Reason" = Reasons."Code" and Reasons.Type = 'W'
	WHERE CashDraw."Draw_Date" between @date1 and @date2
else
	SET @till_number1 = CONVERT(int, @till1)
	SET @till_number2 = CONVERT(int, @till2)

	SELECT
		CashDraw."Draw_Date",
		CashDraw."User",
		CashDraw."Till",
		CashDraw."Amount",
		CashDraw."Reason",
		Reasons."Reason"
	FROM
		CSCTrans.dbo.CashDraw CashDraw 
		LEFT JOIN CSCMaster.dbo.Reasons Reasons ON CashDraw."Reason" = Reasons."Code" and Reasons.Type = 'W'
	WHERE CashDraw."Draw_Date" between @date1 and @date2
		and CashDraw."Till" between @till_number1 and @till_number2
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA2319')
DROP PROCEDURE Sproc_SA2319
go
CREATE PROCEDURE [dbo].[Sproc_SA2319] 
	-- Add the parameters for the stored procedure here
	@date1 DATETIME,
	@date2 DATETIME,
	@cust1 NVARCHAR(10),
	@cust2 NVARCHAR(10)
AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

if @cust1 = 'All'	
	SELECT
		SALEHEAD."SALE_NO",
		SALEHEAD."CLIENT",
		SALEHEAD."SALE_DATE",
		SALELINE."STOCK_CODE",
		SALELINE."DESCRIPT",
		SALELINE."QUANTITY",
		SALELINE."AMOUNT",
		SALELINE."SALE_TYPE",
		CLIENT."CL_NAME",
		CLIENT."CL_POSTAL",
		CLIENT."CL_PHONE"
	FROM
		(CSCTrans.dbo.SALEHEAD SALEHEAD INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."SALE_NO" = SALELINE."SALE_NO") left JOIN CSCMaster.dbo.CLIENT CLIENT ON SALEHEAD."CLIENT" = CLIENT."CL_CODE"
	WHERE SALEHEAD."SALE_DATE" between @date1 and @date2
	ORDER BY
		SALEHEAD."SALE_DATE" ASC, SALEHEAD."CLIENT" ASC
else
	SELECT
		SALEHEAD."SALE_NO",
		SALEHEAD."CLIENT",
		SALEHEAD."SALE_DATE",
		SALELINE."STOCK_CODE",
		SALELINE."DESCRIPT",
		SALELINE."QUANTITY",
		SALELINE."AMOUNT",
		SALELINE."SALE_TYPE",
		CLIENT."CL_NAME",
		CLIENT."CL_POSTAL",
		CLIENT."CL_PHONE"
	FROM
		(CSCTrans.dbo.SALEHEAD SALEHEAD INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."SALE_NO" = SALELINE."SALE_NO") left JOIN CSCMaster.dbo.CLIENT CLIENT ON SALEHEAD."CLIENT" = CLIENT."CL_CODE"
	WHERE SALEHEAD."SALE_DATE" between @date1 and @date2
			and CLIENT.CL_CODE between @cust1 and @cust2
	ORDER BY
		SALEHEAD."SALE_DATE" ASC, SALEHEAD."CLIENT" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA2379')
DROP PROCEDURE Sproc_SA2379
go
CREATE PROCEDURE [dbo].[Sproc_SA2379] 
	-- Add the parameters for the stored procedure here
	@date1 DATETIME,
	@date2 DATETIME
AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	SELECT DISTINCT
		SALEHEAD."USER",
		SALEHEAD."SALE_AMT",
		SALEHEAD."T_TYPE",
		SALEHEAD."Reason",
		SALELINE."STOCK_CODE",
		SALELINE."DESCRIPT",
		SALELINE."QUANTITY",
		SALELINE."COST",
		Reasons."Reason"
	FROM
		(CSCTrans.dbo.SALEHEAD SALEHEAD INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."SALE_NO" = SALELINE."SALE_NO") INNER JOIN CSCMaster.dbo.Reasons Reasons on SALEHEAD.Reason = Reasons.Code AND SALEHEAD.Reason_Type = Reasons.Type 
	WHERE
		SALEHEAD."T_TYPE" = 'MARKDOWN'
		and SALEHEAD."SALE_DATE" between @date1 and @date2
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA7012')
DROP PROCEDURE Sproc_SA7012
go
CREATE PROCEDURE [dbo].[Sproc_SA7012]
	@date1		DATETIME,
	@date2		DATETIME,
    @stock1		NVARCHAR(10),
    @stock2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @stock1 = 'All'
		SELECT
			SALEHEAD."SALE_DATE",
			SALEHEAD."T_TYPE",
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."QUANTITY",
			SALELINE."DISCOUNT",
			SALELINE."DISC_ADJ",
			SALELINE."AMOUNT",
			SALELINE."COST",
			SALELINE."DEPT",
			SALELINE."Rebate"
		FROM
			CSCTrans.dbo.SALEHEAD SALEHEAD INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON
				SALEHEAD."SALE_NO" = SALELINE."SALE_NO"
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
		ORDER BY
			SALEHEAD."SALE_DATE" ASC,
			SALELINE."STOCK_CODE" ASC
	else
		SELECT
			SALEHEAD."SALE_DATE",
			SALEHEAD."T_TYPE",
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."QUANTITY",
			SALELINE."DISCOUNT",
			SALELINE."DISC_ADJ",
			SALELINE."AMOUNT",
			SALELINE."COST",
			SALELINE."DEPT",
			SALELINE."Rebate"
		FROM
			CSCTrans.dbo.SALEHEAD SALEHEAD INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON
				SALEHEAD."SALE_NO" = SALELINE."SALE_NO"
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
			and SALELINE.STOCK_CODE between @stock1 and @stock2
		ORDER BY
			SALEHEAD."SALE_DATE" ASC,
			SALELINE."STOCK_CODE" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA7013')
DROP PROCEDURE Sproc_SA7013
go
CREATE PROCEDURE [dbo].[Sproc_SA7013]
	@date1		DATETIME,
	@date2		DATETIME,
    @dept1		NVARCHAR(10),
    @dept2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @dept1 = 'All'
		SELECT
			SALEHEAD."ShiftDate",
			SALEHEAD."T_TYPE",
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."QUANTITY",
			SALELINE."PRICE",
			SALELINE."DISCOUNT",
			SALELINE."DISC_ADJ",
			SALELINE."AMOUNT",
			SALELINE."COST",
			SALELINE."DEPT",
			Dept."DEPT_NAME"
		FROM
			((CSCTrans.dbo.SALEHEAD SALEHEAD 
			INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."SALE_NO" = SALELINE."SALE_NO") 
			LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON SALELINE."DEPT" = Dept."DEPT")
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
		ORDER BY
			SALEHEAD.SHIFTDATE ASC, SALELINE."DEPT" ASC, SALELINE."STOCK_CODE" ASC
	else
		SELECT
			SALEHEAD."ShiftDate",
			SALEHEAD."T_TYPE",
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."QUANTITY",
			SALELINE."PRICE",
			SALELINE."DISCOUNT",
			SALELINE."DISC_ADJ",
			SALELINE."AMOUNT",
			SALELINE."COST",
			SALELINE."DEPT",
			Dept."DEPT_NAME"
		FROM
			((CSCTrans.dbo.SALEHEAD SALEHEAD 
			INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."SALE_NO" = SALELINE."SALE_NO") 
			LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON SALELINE."DEPT" = Dept."DEPT")
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
			and SALELINE.Dept between @dept1 and @dept2
		ORDER BY
			SALEHEAD.SHIFTDATE ASC, SALELINE."DEPT" ASC, SALELINE."STOCK_CODE" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA7016')
DROP PROCEDURE Sproc_SA7016
go
CREATE PROCEDURE [dbo].[Sproc_SA7016]
	@date1		DATETIME,
	@date2		DATETIME,
    @dept1		NVARCHAR(10),
    @dept2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	SET NOCOUNT ON;
    DECLARE @DEPT		NVARCHAR(10) 
    DECLARE @DEPT_NAME	NVARCHAR(35) 

	DECLARE @OUTPUT TABLE
	(	
		DEPT NVARCHAR(10),
		DEPT_NAME NVARCHAR(35),
		SALE_QTY FLOAT,
		SALE_AMT MONEY,
		REFUND_QTY FLOAT,
		REFUND_AMT MONEY,
		DISCOUNT MONEY,
		NET_AMT MONEY
	)

	IF @dept1 = 'ALL' or @dept2 = 'ALL'
		BEGIN
			DECLARE db_cursor CURSOR FOR 
			SELECT DISTINCT SL.DEPT, DEPT_NAME 
			FROM [CSCTrans].[DBO].[SALELINE] SL
			LEFT JOIN [CSCMASTER].[DBO].[DEPT] D
			ON D.DEPT = SL.DEPT
		END
	ELSE 
		BEGIN
			DECLARE db_cursor CURSOR FOR 
			SELECT DISTINCT SL.DEPT, DEPT_NAME 
			FROM [CSCTrans].[DBO].[SALELINE] SL
			JOIN [CSCMASTER].[DBO].[DEPT] D
			ON D.DEPT = SL.DEPT AND SL.DEPT between @dept1 and @dept2
		END 

	OPEN DB_CURSOR  
	FETCH NEXT FROM DB_CURSOR INTO @DEPT, @DEPT_NAME
	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		DECLARE @SALE_QTY AS FLOAT
		DECLARE @REFUND_QTY AS FLOAT
		DECLARE @SALE_AMT AS MONEY
		DECLARE @REFUND_AMT AS MONEY
		DECLARE @DISCOUNT AS MONEY
		DECLARE @NET_AMT AS MONEY
		
		SELECT @SALE_QTY = ISNULL(SUM(SL.QUANTITY), 0)
			FROM 
				[CSCTrans].[dbo].[SALELINE] SL
			INNER JOIN 
				[CSCTrans].[dbo].[SALEHEAD] SH
			ON 
				SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM
			WHERE 
				SL.DEPT = @DEPT
				AND SL.SALE_TYPE IN ('SALE', 'PATT_APP') 
				AND SH.ShiftDate BETWEEN @date1 AND @date2

		SELECT @REFUND_QTY = ISNULL(SUM(SL.QUANTITY) * -1, 0)
			FROM 
				[CSCTrans].[dbo].[SALELINE] SL
			INNER JOIN 
				[CSCTrans].[dbo].[SALEHEAD] SH
			ON 
				SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM
			WHERE 
				SL.DEPT = @DEPT
				AND SL.SALE_TYPE = 'REFUND'
				AND SH.ShiftDate BETWEEN @date1 AND @date2

		IF @SALE_QTY <> 0 OR @REFUND_QTY <>  0
		BEGIN
			SELECT @SALE_AMT = ISNULL(SUM(SL.AMOUNT), 0)
				FROM 
					[CSCTrans].[dbo].[SALELINE] SL
				INNER JOIN 
					[CSCTrans].[dbo].[SALEHEAD] SH
				ON 
				SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM 
				WHERE 
				SL.DEPT = @DEPT
				AND SH.T_TYPE IN ('SALE', 'PATT_APP') 
				AND SH.ShiftDate BETWEEN @date1 AND @date2

			SELECT @REFUND_AMT = ISNULL(SUM(SL.AMOUNT) * -1, 0)
				FROM 
					[CSCTrans].[dbo].[SALELINE] SL
				INNER JOIN 
					[CSCTrans].[dbo].[SALEHEAD] SH
				ON 
					SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM
				WHERE 
					SL.DEPT = @DEPT
					AND SH.T_TYPE = 'REFUND'
					AND SH.ShiftDate BETWEEN @date1 AND @date2
			
			SELECT @DISCOUNT = 
				ISNULL(SUM(SL.DISCOUNT), 0) + ISNULL(SUM(SL.DISC_ADJ), 0)
				FROM 
					[CSCTrans].[dbo].[SALELINE] SL
				INNER JOIN 
					[CSCTrans].[dbo].[SALEHEAD] SH
				ON 
					SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM 
				WHERE 
					SL.DEPT = @DEPT
					AND SH.T_TYPE IN  ('SALE', 'REFUND', 'PATP_APP')
					AND SH.ShiftDate BETWEEN @date1 AND @date2
	
			SELECT @NET_AMT = @SALE_AMT - @REFUND_AMT - @DISCOUNT

			INSERT INTO @OUTPUT VALUES (
				@DEPT,
				@DEPT_NAME,
				@SALE_QTY,
				@SALE_AMT,
				@REFUND_QTY,
				@REFUND_AMT,
				@DISCOUNT,
				@NET_AMT
			)
		END

		FETCH NEXT FROM DB_CURSOR INTO @DEPT, @DEPT_NAME

	END		
	CLOSE db_cursor  
	DEALLOCATE db_cursor 
	SELECT * FROM @OUTPUT
	ORDER BY DEPT
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA7019')
DROP PROCEDURE Sproc_SA7019
go
CREATE PROCEDURE [dbo].[Sproc_SA7019]
	@date1		DATETIME,
	@date2		DATETIME,
    @emp1		NVARCHAR(10),
    @emp2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @emp1 = 'All'
		SELECT
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."QUANTITY",
			SALELINE."PRICE",
			SALELINE."AMOUNT",
			SALELINE."Upsell",
			SALEHEAD."USER",
			SALEHEAD."ShiftDate",
			TABLE_USER."U_NAME"
		FROM
			((CSCTrans.dbo.SALELINE SALELINE INNER JOIN CSCTrans.dbo.SALEHEAD SALEHEAD ON SALELINE."SALE_NO" = SALEHEAD."SALE_NO") INNER JOIN CSCAdmin.dbo.[USER] [TABLE_USER] ON SALEHEAD."USER" = [TABLE_USER]."U_CODE" INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON SALELINE."STOCK_CODE" = STOCKMST."STOCK_CODE")
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
		ORDER BY
			SALEHEAD."USER" ASC, SALELINE."DESCRIPT" ASC, SALELINE."STOCK_CODE" ASC
	else
		SELECT
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."QUANTITY",
			SALELINE."PRICE",
			SALELINE."AMOUNT",
			SALELINE."Upsell",
			SALEHEAD."USER",
			SALEHEAD."ShiftDate",
			TABLE_USER."U_NAME"
		FROM
			((CSCTrans.dbo.SALELINE SALELINE INNER JOIN CSCTrans.dbo.SALEHEAD SALEHEAD ON SALELINE."SALE_NO" = SALEHEAD."SALE_NO") INNER JOIN CSCAdmin.dbo.[USER] [TABLE_USER] ON SALEHEAD."USER" = [TABLE_USER]."U_CODE" INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON SALELINE."STOCK_CODE" = STOCKMST."STOCK_CODE")
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
			and SALELINE.Dept between @emp1 and @emp2
		ORDER BY
			SALEHEAD."USER" ASC, SALELINE."DESCRIPT" ASC, SALELINE."STOCK_CODE" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA7062')
DROP PROCEDURE Sproc_SA7062
go
CREATE PROCEDURE [dbo].[Sproc_SA7062]
	@date1		DATETIME,
	@date2		DATETIME,
    @dept1		NVARCHAR(10),
    @dept2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @dept1 = 'All'
		SELECT
			SALEHEAD."T_TYPE",
			SALELINE."STOCK_CODE",
			SALELINE."QUANTITY",
			SALELINE."DISCOUNT",
			SALELINE."DISC_ADJ",
			SALELINE."AMOUNT",
			SALELINE."COST",
			SALELINE."DEPT",
			SALELINE."Rebate",
			STOCKMST."DESCRIPT",
			KIT_ITEM."KIT_CODE",
			Dept."DEPT_NAME",
			STOCK_BR."LAST_SALE"
		FROM
			((((CSCTrans.dbo.SALEHEAD SALEHEAD INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."SALE_NO" = SALELINE."SALE_NO") INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON SALELINE."STOCK_CODE" = STOCKMST."STOCK_CODE") LEFT JOIN CSCMaster.dbo.KIT_ITEM KIT_ITEM ON SALELINE."STOCK_CODE" = KIT_ITEM."STOCK_CODE") INNER JOIN CSCMaster.dbo.Dept Dept ON SALELINE."DEPT" = Dept."DEPT") INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE"
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
		ORDER BY
			SALELINE."DEPT" ASC, SALELINE."STOCK_CODE" ASC
	else
		SELECT
			SALEHEAD."T_TYPE",
			SALELINE."STOCK_CODE",
			SALELINE."QUANTITY",
			SALELINE."DISCOUNT",
			SALELINE."DISC_ADJ",
			SALELINE."AMOUNT",
			SALELINE."COST",
			SALELINE."DEPT",
			SALELINE."Rebate",
			STOCKMST."DESCRIPT",
			KIT_ITEM."KIT_CODE",
			Dept."DEPT_NAME",
			STOCK_BR."LAST_SALE"
		FROM
			((((CSCTrans.dbo.SALEHEAD SALEHEAD INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."SALE_NO" = SALELINE."SALE_NO") INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON SALELINE."STOCK_CODE" = STOCKMST."STOCK_CODE") LEFT JOIN CSCMaster.dbo.KIT_ITEM KIT_ITEM ON SALELINE."STOCK_CODE" = KIT_ITEM."STOCK_CODE") INNER JOIN CSCMaster.dbo.Dept Dept ON SALELINE."DEPT" = Dept."DEPT") INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE"
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
			and SALELINE.Dept between @dept1 and @dept2
		ORDER BY
			SALELINE."DEPT" ASC, SALELINE."STOCK_CODE" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SA7070')
DROP PROCEDURE Sproc_SA7070
go
CREATE PROCEDURE [dbo].[Sproc_SA7070]
	@date1		DATETIME,
	@date2		DATETIME,
    @emp1		NVARCHAR(10),
    @emp2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @emp1 = 'All'
		SELECT
			DelLines."SALE_NO",
			DelLines."Del_Date",
			DelLines."Del_Action",
			DelLines."STOCK_CODE",
			DelLines."DESCRIPT",
			DelLines."QUANTITY",
			DelLines."PRICE",
			DelLines."AMOUNT",
			DelLines."USER"
		FROM
			CSCTrans.dbo.DelLines DelLines
		WHERE DelLines."Del_Date" between @date1 and @date2
	else
		SELECT
			DelLines."SALE_NO",
			DelLines."Del_Date",
			DelLines."Del_Action",
			DelLines."STOCK_CODE",
			DelLines."DESCRIPT",
			DelLines."QUANTITY",
			DelLines."PRICE",
			DelLines."AMOUNT",
			DelLines."USER"
		FROM
			CSCTrans.dbo.DelLines DelLines
		WHERE DelLines."Del_Date" between @date1 and @date2
			and DelLines."USER" between @emp1 and @emp2
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK1010')
DROP PROCEDURE Sproc_SK1010
go
CREATE PROCEDURE [dbo].[Sproc_SK1010] 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		Dept."DEPT",
		Dept."DEPT_NAME"
	FROM
		CSCMaster.dbo.Dept Dept
	ORDER BY
		Dept."DEPT" ASC
END

GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK1011')
DROP PROCEDURE Sproc_SK1011
go
CREATE PROCEDURE [dbo].[Sproc_SK1011] 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		Dept."DEPT",
		Dept."DEPT_NAME",
		SubDept."SUB_DEPT",
		SubDept."SUB_NAME",
		SubDetail."SubDetail",
		SubDetail."SubDetail_Name"
	FROM
		(CSCMaster.dbo.Dept Dept 
		LEFT JOIN CSCMaster.dbo.SubDept SubDept ON Dept."DEPT" = SubDept."DEPT") 
		LEFT JOIN CSCMaster.dbo.SubDetail SubDetail ON SubDept."DEPT" = SubDetail."Dept" 
		AND SubDept."SUB_DEPT" = SubDetail."SubDept"
	ORDER BY
		Dept."DEPT", SubDept."SUB_DEPT", SubDetail."SubDetail"
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK1070')
DROP PROCEDURE Sproc_SK1070
go
CREATE PROCEDURE [dbo].[Sproc_SK1070] 
	-- Add the parameters for the stored procedure here
	@tax1 NVARCHAR(10),
	@tax2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @tax1 = 'All'	
	SELECT
		TAXMAST."TAX_NAME",
		TAXMAST."TAX_ACTIVE",
		TAXMAST."TAX_ORD",
		TAXMAST."TAX_DESC",
		TAXMAST."TAX_APPLY",
		TAXMAST."TAX_DEF",
		TAXRATE."TAX_CODE",
		TAXRATE."TAX_RATE",
		TAXRATE."TAX_DESC"
	FROM
		CSCMaster.dbo.TAXMAST TAXMAST INNER JOIN CSCMaster.dbo.TAXRATE TAXRATE ON
			TAXMAST."TAX_NAME" = TAXRATE."TAX_NAME"
	ORDER BY
		TAXMAST."TAX_NAME" ASC
else
	SELECT
		TAXMAST."TAX_NAME",
		TAXMAST."TAX_ACTIVE",
		TAXMAST."TAX_ORD",
		TAXMAST."TAX_DESC",
		TAXMAST."TAX_APPLY",
		TAXMAST."TAX_DEF",
		TAXRATE."TAX_CODE",
		TAXRATE."TAX_RATE",
		TAXRATE."TAX_DESC"
	FROM
		CSCMaster.dbo.TAXMAST TAXMAST INNER JOIN CSCMaster.dbo.TAXRATE TAXRATE ON
			TAXMAST."TAX_NAME" = TAXRATE."TAX_NAME"
	WHERE TAXMAST."TAX_NAME" between @tax1 and @tax2
	ORDER BY
		TAXMAST."TAX_NAME" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK2010')
DROP PROCEDURE Sproc_SK2010
go
CREATE PROCEDURE [dbo].[Sproc_SK2010] 
	-- Add the parameters for the stored procedure here
	@dept1 NVARCHAR(10),
	@dept2 NVARCHAR(10),
	@subdept1 NVARCHAR(10),
	@subdept2 NVARCHAR(10),
	@subdetail1 NVARCHAR(10),
	@subdetail2 NVARCHAR(10),
	@stock1 NVARCHAR(10),
	@stock2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @stock1 = 'All'
	if @dept1 = 'All' 
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			STOCKMST."AVG_COST",
			STOCKMST."DEPT",
			STOCKMST."SUB_DEPT",
			STOCKMST."SERIAL",
			StockTax."Stock_Code",
			StockTax."Tax_Name",
			StockTax."Tax_Code",
			Dept."DEPT_NAME",
			SubDept."SUB_NAME",
			PLUMAST."PLU_CODE",
			PLUMAST."PLU_PRIM"
		FROM
			(((CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM") 
			LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code") 
			LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
			LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."DEPT" = SubDept."DEPT" 
			AND STOCKMST."Sub_DEPT" = SubDept."SUB_DEPT"
		ORDER BY
			STOCKMST."STOCK_CODE" ASC, StockTax."Stock_Code" ASC, StockTax."Tax_Name" ASC, StockTax."Tax_Code" ASC
	else
		if @subdept1 = 'All'
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				STOCKMST."SUB_DEPT",
				STOCKMST."SERIAL",
				StockTax."Stock_Code",
				StockTax."Tax_Name",
				StockTax."Tax_Code",
				Dept."DEPT_NAME",
				SubDept."SUB_NAME",
				PLUMAST."PLU_CODE",
				PLUMAST."PLU_PRIM"
			FROM
				(((CSCMaster.dbo.STOCKMST STOCKMST 
				INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM") 
				LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code") 
				LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
				LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."DEPT" = SubDept."DEPT" 
				AND STOCKMST."Sub_DEPT" = SubDept."SUB_DEPT"
			WHERE STOCKMST."DEPT" between @dept1 and @dept2
			ORDER BY
				STOCKMST."STOCK_CODE" ASC, StockTax."Stock_Code" ASC, StockTax."Tax_Name" ASC, StockTax."Tax_Code" ASC
		else
			if @subdetail1 = 'All'
				SELECT
					STOCKMST."STOCK_CODE",
					STOCKMST."DESCRIPT",
					STOCKMST."STOCK_TYPE",
					STOCKMST."AVG_COST",
					STOCKMST."DEPT",
					STOCKMST."SUB_DEPT",
					STOCKMST."SERIAL",
					StockTax."Stock_Code",
					StockTax."Tax_Name",
					StockTax."Tax_Code",
					Dept."DEPT_NAME",
					SubDept."SUB_NAME",
					PLUMAST."PLU_CODE",
					PLUMAST."PLU_PRIM"
				FROM
					(((CSCMaster.dbo.STOCKMST STOCKMST 
					INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM") 
					LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code") 
					LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
					LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."sub_DEPT" = SubDept."sub_dept" 
				WHERE STOCKMST."DEPT" between @dept1 and @dept2 and STOCKMST."Sub_dept" between @subdept1 and @subdept2 
				ORDER BY
					STOCKMST."STOCK_CODE" ASC, StockTax."Stock_Code" ASC, StockTax."Tax_Name" ASC, StockTax."Tax_Code" ASC
			else
				SELECT
					STOCKMST."STOCK_CODE",
					STOCKMST."DESCRIPT",
					STOCKMST."STOCK_TYPE",
					STOCKMST."AVG_COST",
					STOCKMST."DEPT",
					STOCKMST."SUB_DEPT",
					STOCKMST."SERIAL",
					StockTax."Stock_Code",
					StockTax."Tax_Name",
					StockTax."Tax_Code",
					Dept."DEPT_NAME",
					SubDept."SUB_NAME",
					PLUMAST."PLU_CODE",
					PLUMAST."PLU_PRIM"
				FROM
					(((CSCMaster.dbo.STOCKMST STOCKMST 
					INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM") 
					LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code") 
					LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
					LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."DEPT" = SubDept."DEPT" AND STOCKMST."Sub_DEPT" = SubDept."SUB_DEPT"
					LEFT OUTER JOIN CSCMaster.dbo.SubDetail SubDetail ON STOCKMST."DEPT" = SubDetail."DEPT" AND STOCKMST."Sub_DEPT" = SubDetail."SUBDEPT" AND STOCKMST."Sub_DETAIL" = SubDetail."SUBDetail"
				WHERE STOCKMST."DEPT" between @dept1 and @dept2 and STOCKMST."Sub_dept" between @subdept1 and @subdept2 and STOCKMST."Sub_detail" between @subdetail1 and @subdetail2 
				ORDER BY
					STOCKMST."STOCK_CODE" ASC, StockTax."Stock_Code" ASC, StockTax."Tax_Name" ASC, StockTax."Tax_Code" ASC
else	--for specific stockcode
	if @dept1 = 'All' 
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			STOCKMST."AVG_COST",
			STOCKMST."DEPT",
			STOCKMST."SUB_DEPT",
			STOCKMST."SERIAL",
			StockTax."Stock_Code",
			StockTax."Tax_Name",
			StockTax."Tax_Code",
			Dept."DEPT_NAME",
			SubDept."SUB_NAME",
			PLUMAST."PLU_CODE",
			PLUMAST."PLU_PRIM"
		FROM
			(((CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM") 
			LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code") 
			LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
			LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."DEPT" = SubDept."DEPT" 
			AND STOCKMST."Sub_DEPT" = SubDept."SUB_DEPT"
		WHERE STOCKMST."STOCK_CODE" between @stock1 and @stock2
		ORDER BY
			STOCKMST."STOCK_CODE" ASC, StockTax."Stock_Code" ASC, StockTax."Tax_Name" ASC, StockTax."Tax_Code" ASC
	else
		if @subdept1 = 'All'
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				STOCKMST."SUB_DEPT",
				STOCKMST."SERIAL",
				StockTax."Stock_Code",
				StockTax."Tax_Name",
				StockTax."Tax_Code",
				Dept."DEPT_NAME",
				SubDept."SUB_NAME",
				PLUMAST."PLU_CODE",
				PLUMAST."PLU_PRIM"
			FROM
				(((CSCMaster.dbo.STOCKMST STOCKMST 
				INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM") 
				LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code") 
				LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
				LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."DEPT" = SubDept."DEPT" AND STOCKMST."Sub_DEPT" = SubDept."SUB_DEPT"
			WHERE STOCKMST."DEPT" between @dept1 and @dept2 and STOCKMST."STOCK_CODE" between @stock1 and @stock2

			ORDER BY
				STOCKMST."STOCK_CODE" ASC, StockTax."Stock_Code" ASC, StockTax."Tax_Name" ASC, StockTax."Tax_Code" ASC
		else
			if @subdetail1 = 'All'
				SELECT
					STOCKMST."STOCK_CODE",
					STOCKMST."DESCRIPT",
					STOCKMST."STOCK_TYPE",
					STOCKMST."AVG_COST",
					STOCKMST."DEPT",
					STOCKMST."SUB_DEPT",
					STOCKMST."SERIAL",
					StockTax."Stock_Code",
					StockTax."Tax_Name",
					StockTax."Tax_Code",
					Dept."DEPT_NAME",
					SubDept."SUB_NAME",
					PLUMAST."PLU_CODE",
					PLUMAST."PLU_PRIM"
				FROM
					(((CSCMaster.dbo.STOCKMST STOCKMST 
					INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM") 
					LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code") 
					LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
					LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."sub_DEPT" = SubDept."dept" AND STOCKMST."Sub_DEPT" = SubDept."SUB_DEPT"

				WHERE STOCKMST."DEPT" between @dept1 and @dept2 and STOCKMST."Sub_dept" between @subdept1 and @subdept2 and STOCKMST."STOCK_CODE" between @stock1 and @stock2
				ORDER BY
					STOCKMST."STOCK_CODE" ASC, StockTax."Stock_Code" ASC, StockTax."Tax_Name" ASC, StockTax."Tax_Code" ASC
			else
				SELECT
					STOCKMST."STOCK_CODE",
					STOCKMST."DESCRIPT",
					STOCKMST."STOCK_TYPE",
					STOCKMST."AVG_COST",
					STOCKMST."DEPT",
					STOCKMST."SUB_DEPT",
					STOCKMST."SERIAL",
					StockTax."Stock_Code",
					StockTax."Tax_Name",
					StockTax."Tax_Code",
					Dept."DEPT_NAME",
					SubDept."SUB_NAME",
					PLUMAST."PLU_CODE",
					PLUMAST."PLU_PRIM"
				FROM
					(((CSCMaster.dbo.STOCKMST STOCKMST 
					INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM") 
					LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code") 
					LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
					LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."DEPT" = SubDept."DEPT" AND STOCKMST."Sub_DEPT" = SubDept."SUB_DEPT"
					LEFT OUTER JOIN CSCMaster.dbo.SubDetail SubDetail ON STOCKMST."DEPT" = SubDetail."DEPT" AND STOCKMST."Sub_DEPT" = SubDetail."SUBDEPT" AND STOCKMST."Sub_DETAIL" = SubDetail."SUBDetail"
				WHERE STOCKMST."DEPT" between @dept1 and @dept2 and STOCKMST."Sub_detail" between @subdetail1 and @subdetail2 and STOCKMST."STOCK_CODE" between @stock1 and @stock2
				ORDER BY
					STOCKMST."STOCK_CODE" ASC, StockTax."Stock_Code" ASC, StockTax."Tax_Name" ASC, StockTax."Tax_Code" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK2011')
DROP PROCEDURE Sproc_SK2011
go
CREATE PROCEDURE [dbo].[Sproc_SK2011] 
	-- Add the parameters for the stored procedure here
	@dept1 NVARCHAR(10),
	@dept2 NVARCHAR(10),
	@subdept1 NVARCHAR(10),
	@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	@stock1 NVARCHAR(10),
	@stock2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @dept1 = 'All'	
	SELECT
		STOCK_BR."STOCK_CODE",
		STOCK_BR."IN_STOCK",
		STOCK_BR."AVAIL",
		STOCKMST."DESCRIPT",
		STOCKMST."DEPT",
		STOCKMST."SUB_DEPT",
		Dept."DEPT_NAME",
		SubDept."SUB_NAME"
	FROM
		CSCMaster.dbo.STOCK_BR STOCK_BR 
		INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON STOCK_BR.STOCK_CODE = STOCKMST.STOCK_CODE 
		INNER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST.DEPT = Dept.DEPT AND STOCKMST.DEPT = Dept.DEPT 
		AND STOCKMST.DEPT = Dept.DEPT 
		LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST.DEPT = SubDept.DEPT AND STOCKMST.SUB_DEPT = SubDept.SUB_DEPT 
	WHERE
		(STOCKMST.STOCK_TYPE = 'V')
	ORDER BY
		STOCKMST.DEPT, STOCKMST.SUB_DEPT, STOCKMST.STOCK_CODE
else
	if @subdept1 = 'All'	
		if @stock1 = 'All'
			SELECT
				STOCK_BR."STOCK_CODE",
				STOCK_BR."IN_STOCK",
				STOCK_BR."AVAIL",
				STOCKMST."DESCRIPT",
				STOCKMST."DEPT",
				STOCKMST."SUB_DEPT",
				Dept."DEPT_NAME",
				SubDept."SUB_NAME"
			FROM
				CSCMaster.dbo.STOCK_BR STOCK_BR 
				INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON STOCK_BR.STOCK_CODE = STOCKMST.STOCK_CODE 
				INNER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST.DEPT = Dept.DEPT AND STOCKMST.DEPT = Dept.DEPT AND STOCKMST.DEPT = Dept.DEPT 
				and Dept.DEPT between @dept1 and @dept2
				LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST.DEPT = SubDept.DEPT AND STOCKMST.SUB_DEPT = SubDept.SUB_DEPT 

			WHERE
				(STOCKMST.STOCK_TYPE = 'V')
			ORDER BY
				STOCKMST.DEPT, STOCKMST.SUB_DEPT, STOCKMST.STOCK_CODE
		else
			SELECT
				STOCK_BR."STOCK_CODE",
				STOCK_BR."IN_STOCK",
				STOCK_BR."AVAIL",
				STOCKMST."DESCRIPT",
				STOCKMST."DEPT",
				STOCKMST."SUB_DEPT",
				Dept."DEPT_NAME",
				SubDept."SUB_NAME"
			FROM
				CSCMaster.dbo.STOCK_BR STOCK_BR 
				INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON STOCK_BR.STOCK_CODE = STOCKMST.STOCK_CODE 
				INNER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST.DEPT = Dept.DEPT AND STOCKMST.DEPT = Dept.DEPT AND STOCKMST.DEPT = Dept.DEPT 
				and Dept.DEPT between @dept1 and @dept2
				LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST.DEPT = SubDept.DEPT AND STOCKMST.SUB_DEPT = SubDept.SUB_DEPT 

			WHERE
				(STOCKMST.STOCK_TYPE = 'V') and STOCKMST.STOCK_CODE between @stock1 and @stock2
			ORDER BY
				STOCKMST.DEPT, STOCKMST.SUB_DEPT, STOCKMST.STOCK_CODE
	else
		SELECT
			STOCK_BR."STOCK_CODE",
			STOCK_BR."IN_STOCK",
			STOCK_BR."AVAIL",
			STOCKMST."DESCRIPT",
			STOCKMST."DEPT",
			STOCKMST."SUB_DEPT",
			Dept."DEPT_NAME",
			SubDept."SUB_NAME"
		FROM
			CSCMaster.dbo.STOCK_BR STOCK_BR 
			INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON STOCK_BR.STOCK_CODE = STOCKMST.STOCK_CODE 
			INNER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST.DEPT = Dept.DEPT AND STOCKMST.DEPT = Dept.DEPT AND STOCKMST.DEPT = Dept.DEPT 
			and Dept.DEPT between @dept1 and @dept2
			LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST.DEPT = SubDept.DEPT AND STOCKMST.SUB_DEPT = SubDept.SUB_DEPT 
			and SubDept.SUB_DEPT between @subdept1 and @subdept2

		WHERE
			(STOCKMST.STOCK_TYPE = 'V')
		ORDER BY
			STOCKMST.DEPT, STOCKMST.SUB_DEPT, STOCKMST.STOCK_CODE
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK2012')
DROP PROCEDURE Sproc_SK2012
go
CREATE PROCEDURE [dbo].[Sproc_SK2012] 
	-- Add the parameters for the stored procedure here
	@dept1 NVARCHAR(10),
	@dept2 NVARCHAR(10),
	@subdept1 NVARCHAR(10),
	@subdept2 NVARCHAR(10),
	@vendor1 NVARCHAR(10),
	@vendor2 NVARCHAR(10),
	@stock1 NVARCHAR(10),
	@stock2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	if @vendor1 = 'All'
		if @dept1 = 'All'
			SELECT
				STOCK_BR."STOCK_CODE",
				STOCK_BR."IN_STOCK",
				STOCK_BR."AVAIL",
				STOCK_BR."REORD_QTY",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."Vendor",
				Vendors."Name"
			FROM
				(CSCMaster.dbo.STOCK_BR STOCK_BR INNER 
				JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
					STOCK_BR."STOCK_CODE" = STOCKMST."STOCK_CODE")
				INNER JOIN CSCMaster.dbo.Vendors Vendors ON
					STOCKMST."Vendor" = Vendors."Code"
			WHERE
				STOCKMST."STOCK_TYPE" = 'V'
			ORDER BY
				STOCKMST."Vendor" ASC
		else
			if @subdept1 = 'All'
				SELECT
					STOCK_BR."STOCK_CODE",
					STOCK_BR."IN_STOCK",
					STOCK_BR."AVAIL",
					STOCK_BR."REORD_QTY",
					STOCKMST."DESCRIPT",
					STOCKMST."STOCK_TYPE",
					STOCKMST."Vendor",
					Vendors."Name"
				FROM
					(CSCMaster.dbo.STOCK_BR STOCK_BR INNER 
					JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
						STOCK_BR."STOCK_CODE" = STOCKMST."STOCK_CODE")
					INNER JOIN CSCMaster.dbo.Vendors Vendors ON
						STOCKMST."Vendor" = Vendors."Code"
				WHERE
					STOCKMST."STOCK_TYPE" = 'V'
					and STOCKMST.dept between @dept1 and @dept2
				ORDER BY
					STOCKMST."Vendor" ASC
			else
				if @stock1 = 'All'
					SELECT
						STOCK_BR."STOCK_CODE",
						STOCK_BR."IN_STOCK",
						STOCK_BR."AVAIL",
						STOCK_BR."REORD_QTY",
						STOCKMST."DESCRIPT",
						STOCKMST."STOCK_TYPE",
						STOCKMST."Vendor",
						Vendors."Name"
					FROM
						(CSCMaster.dbo.STOCK_BR STOCK_BR INNER 
						JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
							STOCK_BR."STOCK_CODE" = STOCKMST."STOCK_CODE")
						INNER JOIN CSCMaster.dbo.Vendors Vendors ON
							STOCKMST."Vendor" = Vendors."Code"
					WHERE
						STOCKMST."STOCK_TYPE" = 'V'
						and STOCKMST.dept between @dept1 and @dept2
						and STOCKMST.SUB_DEPT  between @subdept1 and @subdept2
					ORDER BY
						STOCKMST."Vendor" ASC
				else
					SELECT
						STOCK_BR."STOCK_CODE",
						STOCK_BR."IN_STOCK",
						STOCK_BR."AVAIL",
						STOCK_BR."REORD_QTY",
						STOCKMST."DESCRIPT",
						STOCKMST."STOCK_TYPE",
						STOCKMST."Vendor",
						Vendors."Name"
					FROM
						(CSCMaster.dbo.STOCK_BR STOCK_BR INNER 
						JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
							STOCK_BR."STOCK_CODE" = STOCKMST."STOCK_CODE")
						INNER JOIN CSCMaster.dbo.Vendors Vendors ON
							STOCKMST."Vendor" = Vendors."Code"
					WHERE
						STOCKMST."STOCK_TYPE" = 'V'
						and STOCKMST.dept between @dept1 and @dept2
						and STOCKMST.SUB_DEPT  between @subdept1 and @subdept2
						and STOCKMST.STOCK_CODE between @stock1 and @stock2
					ORDER BY
						STOCKMST."Vendor" ASC
	else
	if @dept1 = 'All'
		SELECT
			STOCK_BR."STOCK_CODE",
			STOCK_BR."IN_STOCK",
			STOCK_BR."AVAIL",
			STOCK_BR."REORD_QTY",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			STOCKMST."Vendor",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCK_BR STOCK_BR INNER 
			JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
				STOCK_BR."STOCK_CODE" = STOCKMST."STOCK_CODE")
			INNER JOIN CSCMaster.dbo.Vendors Vendors ON
				STOCKMST."Vendor" = Vendors."Code"
		WHERE
			STOCKMST."STOCK_TYPE" = 'V' and STOCKMST."Vendor" between @vendor1 and @vendor2
		ORDER BY
			STOCKMST."Vendor" ASC
	else
		if @subdept1 = 'All'
			SELECT
				STOCK_BR."STOCK_CODE",
				STOCK_BR."IN_STOCK",
				STOCK_BR."AVAIL",
				STOCK_BR."REORD_QTY",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."Vendor",
				Vendors."Name"
			FROM
				(CSCMaster.dbo.STOCK_BR STOCK_BR INNER 
				JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
					STOCK_BR."STOCK_CODE" = STOCKMST."STOCK_CODE")
				INNER JOIN CSCMaster.dbo.Vendors Vendors ON
					STOCKMST."Vendor" = Vendors."Code"
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' and STOCKMST."Vendor" between @vendor1 and @vendor2
				and STOCKMST.dept between @dept1 and @dept2
			ORDER BY
				STOCKMST."Vendor" ASC
		else
			if @stock1 = 'All'
				SELECT
					STOCK_BR."STOCK_CODE",
					STOCK_BR."IN_STOCK",
					STOCK_BR."AVAIL",
					STOCK_BR."REORD_QTY",
					STOCKMST."DESCRIPT",
					STOCKMST."STOCK_TYPE",
					STOCKMST."Vendor",
					Vendors."Name"
				FROM
					(CSCMaster.dbo.STOCK_BR STOCK_BR INNER 
					JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
						STOCK_BR."STOCK_CODE" = STOCKMST."STOCK_CODE")
					INNER JOIN CSCMaster.dbo.Vendors Vendors ON
						STOCKMST."Vendor" = Vendors."Code"
				WHERE
					STOCKMST."STOCK_TYPE" = 'V' and STOCKMST."Vendor" between @vendor1 and @vendor2
					and STOCKMST.dept between @dept1 and @dept2
					and STOCKMST.SUB_DEPT  between @subdept1 and @subdept2
				ORDER BY
					STOCKMST."Vendor" ASC
			else
				SELECT
					STOCK_BR."STOCK_CODE",
					STOCK_BR."IN_STOCK",
					STOCK_BR."AVAIL",
					STOCK_BR."REORD_QTY",
					STOCKMST."DESCRIPT",
					STOCKMST."STOCK_TYPE",
					STOCKMST."Vendor",
					Vendors."Name"
				FROM
					(CSCMaster.dbo.STOCK_BR STOCK_BR INNER 
					JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
						STOCK_BR."STOCK_CODE" = STOCKMST."STOCK_CODE")
					INNER JOIN CSCMaster.dbo.Vendors Vendors ON
						STOCKMST."Vendor" = Vendors."Code"
				WHERE
					STOCKMST."STOCK_TYPE" = 'V' and STOCKMST."Vendor" between @vendor1 and @vendor2
					and STOCKMST.dept between @dept1 and @dept2
					and STOCKMST.SUB_DEPT  between @subdept1 and @subdept2
					and STOCKMST.STOCK_CODE between @stock1 and @stock2
				ORDER BY
					STOCKMST."Vendor" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK2015')
DROP PROCEDURE Sproc_SK2015
go
CREATE PROCEDURE [dbo].[Sproc_SK2015] 
	-- Add the parameters for the stored procedure here
	--@dept1 NVARCHAR(10),
	--@dept2 NVARCHAR(10),
	--@subdept1 NVARCHAR(10),
	--@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	--@stock1 NVARCHAR(10),
	--@stock2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		STOCKMST."STOCK_CODE",
		STOCKMST."DESCRIPT",
		STOCKMST."DEPT",
		STOCKMST."SUB_DEPT",
		Dept."DEPT_NAME",
		SubDept."SUB_NAME",
		TempAssocTbl."AssocList",
		StockTax."Stock_Code",
		StockTax."Tax_Name",
		StockTax."Tax_Code",
		StockCosts."Cost",
		Stock_Prices."Price",
		TAXRATE."TAX_RATE"
	FROM
		CSCMaster.dbo.STOCKMST STOCKMST INNER JOIN CSCMaster.dbo.PLUMAST PLUMAST ON STOCKMST."STOCK_CODE" = PLUMAST."PLU_PRIM" LEFT OUTER JOIN CSCMaster.dbo.StockCosts ON STOCKMST."STOCK_CODE" = StockCosts."Stock_Code" LEFT OUTER JOIN CSCMaster.dbo.Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code" LEFT OUTER JOIN CSCMaster.dbo.StockTax StockTax ON STOCKMST."STOCK_CODE" = StockTax."Stock_Code" LEFT OUTER JOIN CSCMaster.dbo.TAXRATE ON StockTax.Tax_Name = TAXRATE.TAX_NAME AND StockTax.Tax_Code = TAXRATE.TAX_CODE LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT" LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON STOCKMST."DEPT" = SubDept."DEPT" AND STOCKMST."Sub_DEPT" = SubDept."SUB_DEPT" LEFT OUTER JOIN CSCMaster.dbo.TempAssocTbl ON STOCKMST."STOCK_CODE" = TempAssocTbl."STOCK_CODE" 
	WHERE
		(StockCosts.ActiveVendor =1 AND StockCosts.VendorID = Stock_Prices.VendorID) OR (StockCosts.ActiveVendor =0 AND StockCosts.VendorID = 'ALL' and StockCosts.VendorID = Stock_Prices.VendorID) and STOCKMST.STOCK_CODE not in (SELECT STOCKMST."STOCK_CODE" FROM CSCMaster.dbo.STOCKMST STOCKMST LEFT OUTER JOIN CSCMaster.dbo.StockCosts ON STOCKMST."STOCK_CODE" = StockCosts."Stock_Code" LEFT OUTER JOIN CSCMaster.dbo.Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code" WHERE (StockCosts.ActiveVendor =1 AND StockCosts.VendorID = Stock_Prices.VendorID) )
	ORDER BY
		STOCKMST."STOCK_CODE" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK2020')
DROP PROCEDURE Sproc_SK2020
go
CREATE PROCEDURE [dbo].[Sproc_SK2020] 
	-- Add the parameters for the stored procedure here
	--@dept1 NVARCHAR(10),
	--@dept2 NVARCHAR(10),
	--@subdept1 NVARCHAR(10),
	--@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	--@stock1 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	SELECT
		KIT_MAST."KIT_CODE",
		KIT_MAST."KIT_DESC",
		KIT_ITEM."STOCK_CODE",
		KIT_ITEM."QUANTITY",
		STOCKMST."DESCRIPT",
		STOCKMST."DEPT"
	FROM
		(CSCMaster.dbo.KIT_MAST KIT_MAST INNER JOIN CSCMaster.dbo.KIT_ITEM KIT_ITEM ON
			KIT_MAST."KIT_CODE" = KIT_ITEM."KIT_CODE")
		INNER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON
			KIT_ITEM."STOCK_CODE" = STOCKMST."STOCK_CODE"
	ORDER BY
		KIT_MAST."KIT_CODE" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK2030')
DROP PROCEDURE Sproc_SK2030
go
CREATE PROCEDURE [dbo].[Sproc_SK2030] 
	-- Add the parameters for the stored procedure here
	--@dept1 NVARCHAR(10),
	--@dept2 NVARCHAR(10),
	--@subdept1 NVARCHAR(10),
	--@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	--@stock1 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		PromoDetail."Stock_Code",
		PromoDetail."Dept",
		PromoDetail."SUB_DEPT",
		PromoDetail."SubDetail",
		PromoDetail."Link",
		PromoDetail."Qty",
		STOCKMST."DESCRIPT",
		SubDept."SUB_NAME",
		SubDetail."SubDetail_Name",
		PromoHeader."PromoID",
		PromoHeader."Description",
		PromoHeader."Price",
		PromoHeader."StartDate",
		PromoHeader."EndDate",
		PromoHeader."DiscType",
		PromoHeader."Items",
		Dept."DEPT_NAME"
	FROM
		(CSCMaster.dbo.PromoHeader PromoHeader INNER JOIN CSCMaster.dbo.PromoDetail PromoDetail ON PromoDetail."PromoID" = PromoHeader."PromoID") LEFT OUTER JOIN CSCMaster.dbo.STOCKMST STOCKMST ON PromoDetail."Stock_Code" = STOCKMST."STOCK_CODE" LEFT OUTER JOIN CSCMaster.dbo.SubDept SubDept ON PromoDetail."Dept" = SubDept."DEPT" AND PromoDetail.Sub_dept=subdept.sub_dept LEFT OUTER JOIN CSCMaster.dbo.SubDetail SubDetail ON PromoDetail."Dept" = SubDetail."DEPT" AND PromoDetail.Sub_dept=subdetAIL.subdept AND PromoDetail."subDetail" = SubDetail."subDetail" LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON PromoDetail."Dept" = Dept."DEPT"
	ORDER BY
		PromoHeader."PromoID" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK3010')
DROP PROCEDURE Sproc_SK3010
go
CREATE PROCEDURE [dbo].[Sproc_SK3010] 
	-- Add the parameters for the stored procedure here
	--@dept1 NVARCHAR(10),
	--@dept2 NVARCHAR(10),
	--@subdept1 NVARCHAR(10),
	--@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	@stock1 NVARCHAR(10),
	@stock2 NVARCHAR(10),
	@orderno NVARCHAR(3)
AS
BEGIN
	DECLARE @ord As Integer
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @orderno <> 'All'
		SET @ord = convert(int, @orderno)

if @stock1 = 'All'	
	if @orderno = 'All'
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			Stock_Prices."Price_Number",
			Stock_Prices."Price",
			Stock_Prices."VendorID",
			Stock_Prices."StartDate",
			Stock_Prices."EndDate",
			Stock_Prices."OrderNo",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
		Left JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
		WHERE
			STOCKMST."STOCK_TYPE" = 'V' and stock_prices.price_number= 1 
	else
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			Stock_Prices."Price_Number",
			Stock_Prices."Price",
			Stock_Prices."VendorID",
			Stock_Prices."StartDate",
			Stock_Prices."EndDate",
			Stock_Prices."OrderNo",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
		Left JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
		WHERE
			STOCKMST."STOCK_TYPE" = 'V' and stock_prices.price_number= 1 
			and Stock_Prices.OrderNo = @ord
else
	if @orderno = 'All'
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			Stock_Prices."Price_Number",
			Stock_Prices."Price",
			Stock_Prices."VendorID",
			Stock_Prices."StartDate",
			Stock_Prices."EndDate",
			Stock_Prices."OrderNo",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
		Left JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
		WHERE
			STOCKMST."STOCK_TYPE" = 'V' and stock_prices.price_number= 1 
			and STOCKMST."STOCK_CODE" between @stock1 and @stock2
	else
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			Stock_Prices."Price_Number",
			Stock_Prices."Price",
			Stock_Prices."VendorID",
			Stock_Prices."StartDate",
			Stock_Prices."EndDate",
			Stock_Prices."OrderNo",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
		Left JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
		WHERE
			STOCKMST."STOCK_TYPE" = 'V' and stock_prices.price_number= 1 
			and STOCKMST."STOCK_CODE" between @stock1 and @stock2 and Stock_Prices.OrderNo = @ord
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK3011')
DROP PROCEDURE Sproc_SK3011
go
CREATE PROCEDURE [dbo].[Sproc_SK3011] 
	-- Add the parameters for the stored procedure here
	--@dept1 NVARCHAR(10),
	--@dept2 NVARCHAR(10),
	--@subdept1 NVARCHAR(10),
	--@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	@stock1 NVARCHAR(10),
	@stock2 NVARCHAR(10),
	@orderno varchar(3)
AS
BEGIN
	DECLARE @ord As Integer
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @orderno <> 'All'
		SET @ord = convert(int, @orderno)

if @stock1 = 'All'	
	if @orderno = 'All'
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			STOCKMST."DEPT",
			Stock_Prices."Price_Number",
			Stock_Prices."Price",
			Stock_Prices."VendorID",
			Stock_Prices."StartDate",
			Stock_Prices."EndDate",
			Stock_Prices."OrderNo",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
			LEFT JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
		WHERE
			STOCKMST."STOCK_CODE" <> ''
		ORDER BY
			STOCKMST."STOCK_CODE" ASC, Stock_Prices."VendorID" ASC, Stock_Prices."OrderNo" ASC
	else
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			STOCKMST."DEPT",
			Stock_Prices."Price_Number",
			Stock_Prices."Price",
			Stock_Prices."VendorID",
			Stock_Prices."StartDate",
			Stock_Prices."EndDate",
			Stock_Prices."OrderNo",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
			LEFT JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
		WHERE
			STOCKMST."STOCK_CODE" <> ''
			and Stock_Prices.OrderNo = @ord
		ORDER BY
			STOCKMST."STOCK_CODE" ASC, Stock_Prices."VendorID" ASC, Stock_Prices."OrderNo" ASC
else
	if @orderno = 'All'
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			STOCKMST."DEPT",
			Stock_Prices."Price_Number",
			Stock_Prices."Price",
			Stock_Prices."VendorID",
			Stock_Prices."StartDate",
			Stock_Prices."EndDate",
			Stock_Prices."OrderNo",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
			LEFT JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
		WHERE
			STOCKMST."STOCK_CODE" <> ''
			and STOCKMST."STOCK_CODE" between @stock1 and @stock2
		ORDER BY
			STOCKMST."STOCK_CODE" ASC, Stock_Prices."VendorID" ASC, Stock_Prices."OrderNo" ASC
	else
		SELECT
			STOCKMST."STOCK_CODE",
			STOCKMST."DESCRIPT",
			STOCKMST."STOCK_TYPE",
			STOCKMST."DEPT",
			Stock_Prices."Price_Number",
			Stock_Prices."Price",
			Stock_Prices."VendorID",
			Stock_Prices."StartDate",
			Stock_Prices."EndDate",
			Stock_Prices."OrderNo",
			Vendors."Name"
		FROM
			(CSCMaster.dbo.STOCKMST STOCKMST 
			INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code") 
			LEFT JOIN CSCMaster.dbo.Vendors Vendors ON Stock_Prices."VendorID" = Vendors."Code" 
		WHERE
			STOCKMST."STOCK_CODE" <> ''
			and STOCKMST."STOCK_CODE" between @stock1 and @stock2
			and Stock_Prices.OrderNo = @ord
		ORDER BY
			STOCKMST."STOCK_CODE" ASC, Stock_Prices."VendorID" ASC, Stock_Prices."OrderNo" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK4010')
DROP PROCEDURE Sproc_SK4010
go
CREATE PROCEDURE [dbo].[Sproc_SK4010] 
	-- Add the parameters for the stored procedure here
	--@dept1 NVARCHAR(10),
	--@dept2 NVARCHAR(10),
	--@subdept1 NVARCHAR(10),
	--@subdept2 NVARCHAR(10),
	--@subdetail1 NVARCHAR(10),
	--@subdetail2 NVARCHAR(10),
	@stock1 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


if @stock1 = 'All'	
	SELECT
		STOCKMST."STOCK_CODE",
		STOCKMST."DESCRIPT",
		STOCKMST."DEPT",
		KIT_MAST."KIT_CODE",
		Dept."DEPT_NAME",
		KIT_ITEM."KIT_CODE"
	FROM
		(CSCMaster.dbo.StockMst LEFT JOIN CSCMaster.dbo.KIT_MAST ON CSCMaster.dbo.STOCKMST.STOCK_CODE = KIT_MAST.KIT_CODE) 
		left join CSCMaster.dbo.kit_item on kit_item.stock_code = stockmst.stock_code 
		LEFT JOIN CSCMaster.dbo.DEPT ON STOCKMST.DEPT = DEPT.DEPT 
	WHERE
		(([KIT_MAST].[KIT_CODE] IS NULL AND [StockMst].[Stock_Type]='V') OR ([KIT_MAST].[KIT_CODE] IS NOT NULL AND [StockMst].[Stock_Type]='F') OR ([KIT_MAST].[KIT_CODE] IS NOT NULL AND [StockMst].[Stock_Type]='V'))
	ORDER BY
		StockMst.Stock_Code
else
	SELECT
		STOCKMST."STOCK_CODE",
		STOCKMST."DESCRIPT",
		STOCKMST."DEPT",
		KIT_MAST."KIT_CODE",
		Dept."DEPT_NAME",
		KIT_ITEM."KIT_CODE"
	FROM
		(CSCMaster.dbo.StockMst LEFT JOIN CSCMaster.dbo.KIT_MAST ON STOCKMST.STOCK_CODE = KIT_MAST.KIT_CODE) 
		left join CSCMaster.dbo.kit_item on kit_item.stock_code = stockmst.stock_code 
		LEFT JOIN CSCMaster.dbo.DEPT ON STOCKMST.DEPT = DEPT.DEPT 
	WHERE
		(([KIT_MAST].[KIT_CODE] IS NULL AND [StockMst].[Stock_Type]='V') OR ([KIT_MAST].[KIT_CODE] IS NOT NULL AND [StockMst].[Stock_Type]='F') OR ([KIT_MAST].[KIT_CODE] IS NOT NULL AND [StockMst].[Stock_Type]='V'))
		AND StockMst.STOCK_CODE = @Stock1
	ORDER BY
		StockMst.Stock_Code
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK4020')
DROP PROCEDURE Sproc_SK4020
go
CREATE PROCEDURE [dbo].[Sproc_SK4020] 
	-- Add the parameters for the stored procedure here
	@date1 DATETIME,
	@date2 DATETIME,
	@dept1 NVARCHAR(10),
	@dept2 NVARCHAR(10),
	@subDept1 NVARCHAR(10),
	@subDept2 NVARCHAR(10),
	@subDetail1 NVARCHAR(32),
	@subDetail2 NVARCHAR(32),
	@stockCode1 NVARCHAR(32),
	@stockCode2 NVARCHAR(32)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    DECLARE @code NVARCHAR(32) 
	DECLARE @description NVARCHAR(70) 
	DECLARE @OUTPUT TABLE
	(	
		STOCK_CODE NVARCHAR(32),
		STOCK_DESCRIPTION NVARCHAR(70),
		OPENINGQUANTITY FLOAT,
		QUANTITYIN FLOAT,
		QUANTITYOUT FLOAT,
		CLOSINGQUANTITY FLOAT,
		OPENINGVALUE MONEY,
		VALUEIN MONEY,
		VALUEOUT MONEY,
		CLOSINGVALUE MONEY
	)
	DECLARE db_cursor CURSOR FOR 
	SELECT STOCK_CODE, DESCRIPT FROM [CSCMaster].[dbo].[STOCKMST] 
	LEFT JOIN [CSCMASTER].[DBO].[FUELDEPT] FD 
			ON [CSCMASTER].[DBO].[STOCKMST].DEPT = FD.DEPT
	INNER JOIN [CSCMASTER].[DBO].[DEPT] 
		D ON D.DEPT = [CSCMASTER].[DBO].[STOCKMST].DEPT
		WHERE 
		FD.DEPT IS NULL 
		AND (@stockCode1 = 'All' OR STOCK_CODE = @stockCode1)
		AND (@dept1 = 'All' OR [CSCMASTER].[DBO].[STOCKMST].DEPT = @dept1)
		AND (@subDept1 = 'All' OR SUB_DEPT = @subDept1)
		AND (@subDetail1 = 'All' OR Sub_Detail = @subDetail1)
	OPEN db_cursor  
	FETCH NEXT FROM db_cursor INTO @code, @description  
	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		DECLARE @QUANTITYIN AS FLOAT
		DECLARE @QUANTITYOUT AS FLOAT
		DECLARE @OPENINGVALUE AS MONEY
		DECLARE @VALUEIN AS MONEY
		DECLARE @VALUEOUT AS MONEY
		DECLARE @CLOSINGVALUE AS MONEY
		DECLARE @CLOSINGQUANTITY AS FLOAT
		DECLARE @OPENINGQUANTITY AS FLOAT

		DECLARE @COST AS MONEY
		DECLARE @LASTCOST AS MONEY

		SELECT @OPENINGQUANTITY = ISNULL(SUM(Quantity),0) FROM INVENTORYHIST
			WHERE Inv_Date = @date1
			AND Stock_Code = @code

		SELECT @COST = ISNULL(SUM(COST),0) FROM INVENTORYHIST
			WHERE Inv_Date = @date1
			AND Stock_Code = @code
		
		SELECT @LASTCOST = ISNULL(SUM(COST),0) FROM INVENTORYHIST
			WHERE Inv_Date = @date2
			AND Stock_Code = @code

		SELECT @QUANTITYIN = 
			(SELECT ISNULL(SUM(PD.ReceivedQty),0) FROM [PurchaseHeader] PH
				INNER JOIN [PurchaseDetail] PD ON PH.PurchaseNo = PD.PurchaseNo 
				WHERE PH.ReceivedDate BETWEEN @date1 AND @date2 
				AND PD.StockCode = @code)
			+
			(SELECT ISNULL(SUM(QTY), 0) FROM StockHist
				WHERE Purpose = 1
				AND BatchDate BETWEEN @date1 AND @date2
				AND StockCode = @code)
			+
			(SELECT ISNULL(SUM(QUANTITY * -1), 0) FROM SALELINE SL
			INNER JOIN SALEHEAD SH 
				ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM 
				WHERE STOCK_CODE = @code AND SALE_TYPE = 'REFUND' AND sh.ShiftDate BETWEEN @date1 AND @date2 )
		 
		SELECT @QUANTITYOUT = 
		 	(SELECT ISNULL(SUM(QUANTITY),0) FROM SALELINE SL
			INNER JOIN SALEHEAD SH 
				ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM 
				WHERE STOCK_CODE = @code AND SALE_TYPE = 'SALE' AND sh.ShiftDate BETWEEN @date1 AND @date2)
		 +
			(SELECT ISNULL(SUM(QUANTITY), 0) FROM SALELINE SL
			INNER JOIN SALEHEAD SH 
				ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM 
				WHERE STOCK_CODE = @code AND SALE_TYPE = 'MARKDOWN' AND sh.ShiftDate BETWEEN @date1 AND @date2 )

		SELECT @CLOSINGQUANTITY = @OPENINGQUANTITY + @QUANTITYIN - @QUANTITYOUT

		SELECT @OPENINGVALUE = @OPENINGQUANTITY * @COST

		SELECT @VALUEIN = @QUANTITYIN * @LASTCOST 

		SELECT @VALUEOUT = @QUANTITYOUT * @LASTCOST

		SELECT @CLOSINGVALUE = @CLOSINGQUANTITY * @LASTCOST

		INSERT INTO @OUTPUT VALUES (
			@code,
			@description,
			@OPENINGQUANTITY,
			@QUANTITYIN,
			@QUANTITYOUT,
			@CLOSINGQUANTITY,
			@OPENINGVALUE,
			@VALUEIN,
			@VALUEOUT,
			@CLOSINGVALUE
		)
		
		FETCH NEXT FROM db_cursor INTO @code, @description 
	END		
	CLOSE db_cursor  
	DEALLOCATE db_cursor 
	SELECT * FROM @OUTPUT
	ORDER BY STOCK_CODE
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK4030')
DROP PROCEDURE Sproc_SK4030
go
CREATE PROCEDURE [dbo].[Sproc_SK4030] 
	-- Add the parameters for the stored procedure here
	@date1 SMALLDATETIME,
	@date2 SMALLDATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    DECLARE @code NVARCHAR(32) 
	DECLARE @description NVARCHAR(70) 
	DECLARE @dept NVARCHAR(10)
	DECLARE @dept_name NVARCHAR(35)

	DECLARE @OUTPUT TABLE
	(
		DEPT NVARCHAR(10),
		DEPT_NAME NVARCHAR(35),
		STOCK_CODE NVARCHAR(32),
		STOCK_DESCRIPTION NVARCHAR(70),
		QTY_ON_HAND FLOAT,
		UNIT_COST MONEY,
		EXT_COST MONEY
	)

	DECLARE db_cursor CURSOR FOR 
	SELECT STOCK_CODE, DESCRIPT, D.DEPT, D.DEPT_NAME FROM [CSCMaster].[dbo].[STOCKMST] 
	LEFT JOIN [CSCMASTER].[DBO].[FUELDEPT] FD 
			ON STOCKMST.DEPT = FD.DEPT
	INNER JOIN [CSCMASTER].[DBO].[DEPT] 
		D ON D.DEPT = STOCKMST.DEPT
	WHERE 
		FD.DEPT IS NULL 

	OPEN db_cursor  
	FETCH NEXT FROM db_cursor INTO @code, @description, @dept, @dept_name
	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		DECLARE @QUANTITYIN AS FLOAT
		DECLARE @QUANTITYOUT AS FLOAT

		SELECT @QUANTITYIN = 
			(SELECT ISNULL(SUM(PD.ReceivedQty),0) FROM [PurchaseHeader] PH
				INNER JOIN [PurchaseDetail] PD ON PH.PurchaseNo = PD.PurchaseNo
				WHERE PH.ReceivedDate BETWEEN @date1 AND @date2 
				AND PD.StockCode = @code)
			+
			(SELECT ISNULL(SUM(QTY), 0) FROM StockHist
				WHERE Purpose = 1
				AND BatchDate BETWEEN @date1 AND @date2
				AND StockCode = @code)
			+
			(SELECT ISNULL(SUM(QUANTITY * -1), 0) FROM SALELINE SL
			INNER JOIN SALEHEAD SH 
				ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM
				WHERE STOCK_CODE = @code AND SALE_TYPE = 'REFUND' AND sh.ShiftDate BETWEEN @date1 AND @date2 )
		 
		SELECT @QUANTITYOUT = 
		 	(SELECT ISNULL(SUM(QUANTITY),0) FROM SALELINE SL
			INNER JOIN SALEHEAD SH 
				ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM
				WHERE STOCK_CODE = @code AND SALE_TYPE = 'SALE' AND sh.ShiftDate BETWEEN @date1 AND @date2)
		 +
			(SELECT ISNULL(SUM(QUANTITY), 0) FROM SALELINE SL
			INNER JOIN SALEHEAD SH 
				ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM
				WHERE STOCK_CODE = @code AND SALE_TYPE = 'MARKDOWN' AND sh.ShiftDate BETWEEN @date1 AND @date2 )

		IF @QUANTITYIN = 0 AND @QUANTITYOUT = 0
			BEGIN
			
				DECLARE	@QTY_ON_HAND FLOAT
				DECLARE @UNIT_COST MONEY
				DECLARE @EXT_COST MONEY

				SELECT @QTY_ON_HAND = ISNULL(SUM(Quantity),0) FROM INVENTORYHIST
				WHERE Inv_Date = @date2
				AND Stock_Code = @code
		
				SELECT @UNIT_COST = ISNULL(SUM(COST),0) FROM INVENTORYHIST
					WHERE Inv_Date = @date2
					AND Stock_Code = @code

				SELECT @EXT_COST = @QTY_ON_HAND * @UNIT_COST

				INSERT INTO @OUTPUT VALUES (
					@dept,
					@dept_name,
					@code,
					@description,
					@QTY_ON_HAND,
					@UNIT_COST,
					@EXT_COST
				)
			END
		
		FETCH NEXT FROM db_cursor INTO @code, @description, @dept, @dept_name
	END		
	CLOSE db_cursor  
	DEALLOCATE db_cursor 

	SELECT * FROM @OUTPUT
	ORDER BY DEPT
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK4040')
DROP PROCEDURE Sproc_SK4040
go
CREATE PROCEDURE [dbo].[Sproc_SK4040] 
	@date1 DateTime,
	@qtyOh NVARCHAR(3),
	@dept1 NVARCHAR(10),
	@dept2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    DECLARE @code NVARCHAR(32) 
	DECLARE @description NVARCHAR(70) 
	DECLARE @dept NVARCHAR(10)
	DECLARE @dept_name NVARCHAR(35)
	DECLARE @qty  int

	if @qtyOh = 'All'
		SET @qty = 0
	else
		SET @qty = CAST(@qtyOh as int)

	if @dept1 = 'All' 
		if @qty = 0
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."STD_COST" AS UNIT_COST,
				STOCKMST."STD_COST" * STOCK_BR."IN_STOCK" AS EXT_COST,
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				Dept."DEPT_NAME",
				STOCK_BR."IN_STOCK" AS QTY_ON_HAND,
				Stock_Prices."Price_Number",
				Stock_Prices."Price"
			FROM
				((CSCMaster.dbo.STOCKMST STOCKMST 
				LEFT JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
				INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE") 
				INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON (STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code" 
				and (stockmst.vendor = stock_prices.vendorid or stockmst.vendor is null)) 
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' AND Stock_Prices."Price_Number" = 1 
				and stock_prices.orderno = 0 
				and STOCK_BR.LAST_SALE <= @date1
			ORDER BY
				STOCKMST."DEPT" ASC
		else
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."STD_COST" AS UNIT_COST,
				STOCKMST."STD_COST" * STOCK_BR."IN_STOCK" AS EXT_COST,
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				Dept."DEPT_NAME",
				STOCK_BR."IN_STOCK" AS QTY_ON_HAND,
				Stock_Prices."Price_Number",
				Stock_Prices."Price"
			FROM
				((CSCMaster.dbo.STOCKMST STOCKMST 
				LEFT JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
				INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE") 
				INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON (STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code" 
				and (stockmst.vendor = stock_prices.vendorid or stockmst.vendor is null)) 
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' AND Stock_Prices."Price_Number" = 1 
				and stock_prices.orderno = 0 
				and STOCK_BR.IN_STOCK >= @qty 
				and STOCK_BR.LAST_SALE <= @date1
			ORDER BY
				STOCKMST."DEPT" ASC
	else
		if @qty = 0
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."STD_COST" AS UNIT_COST,
				STOCKMST."STD_COST" * STOCK_BR."IN_STOCK" AS EXT_COST,
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				Dept."DEPT_NAME",
				STOCK_BR."IN_STOCK" AS QTY_ON_HAND,
				Stock_Prices."Price_Number",
				Stock_Prices."Price"
			FROM
				((CSCMaster.dbo.STOCKMST STOCKMST 
				JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT" and Dept.Dept between @dept1 and @dept2) 
				INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE") 
				INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON (STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code" 
				and (stockmst.vendor = stock_prices.vendorid or stockmst.vendor is null)) 
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' AND Stock_Prices."Price_Number" = 1 
				and stock_prices.orderno = 0 and (stockmst.dept between @dept1 and @dept2)
				and STOCK_BR.LAST_SALE <= @date1
			ORDER BY
				STOCKMST."DEPT" ASC
		else
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."STD_COST" AS UNIT_COST,
				STOCKMST."STD_COST" * STOCK_BR."IN_STOCK" AS EXT_COST,
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				Dept."DEPT_NAME",
				STOCK_BR."IN_STOCK" AS QTY_ON_HAND,
				Stock_Prices."Price_Number",
				Stock_Prices."Price"
			FROM
				((CSCMaster.dbo.STOCKMST STOCKMST 
				JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT" and Dept.Dept between @dept1 and @dept2) 
				INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE") 
				INNER JOIN CSCMaster.dbo.Stock_Prices Stock_Prices ON (STOCKMST."STOCK_CODE" = Stock_Prices."Stock_Code" 
				and (stockmst.vendor = stock_prices.vendorid or stockmst.vendor is null)) 
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' AND Stock_Prices."Price_Number" = 1 
				and stock_prices.orderno = 0 and (stockmst.dept between @dept1 and @dept2)
				and STOCK_BR.IN_STOCK >= @qty
				and STOCK_BR.LAST_SALE <= @date1
			ORDER BY
				STOCKMST."DEPT" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK4045')
DROP PROCEDURE Sproc_SK4045
go
CREATE PROCEDURE [dbo].[Sproc_SK4045] 
	@dept1 NVARCHAR(10),
	@dept2 NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    DECLARE @code NVARCHAR(32) 
	DECLARE @description NVARCHAR(70) 
	DECLARE @dept NVARCHAR(10)
	DECLARE @dept_name NVARCHAR(35)
	DECLARE @qty int

	if @dept1 = 'All' 
		if @qty = 0
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."STD_COST" AS UNIT_COST,
				STOCKMST."STD_COST" * STOCK_BR."IN_STOCK" AS EXT_COST,
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				Dept."DEPT_NAME",
				STOCK_BR."IN_STOCK" AS QTY_ON_HAND
			FROM
				(CSCMaster.dbo.STOCKMST STOCKMST 
				LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
				INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE" 
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' AND
				STOCKMST."DEPT" <> '01'
			ORDER BY
				STOCKMST."DEPT" ASC
		else
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."STD_COST" AS UNIT_COST,
				STOCKMST."STD_COST" * STOCK_BR."IN_STOCK" AS EXT_COST,
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				Dept."DEPT_NAME",
				STOCK_BR."IN_STOCK" AS QTY_ON_HAND
			FROM
				(CSCMaster.dbo.STOCKMST STOCKMST 
				LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT") 
				INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE" 
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' AND
				STOCKMST."DEPT" <> '01'
				and STOCK_BR.IN_STOCK <> @qty 
			ORDER BY
				STOCKMST."DEPT" ASC
	else
		if @qty = 0
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."STD_COST" AS UNIT_COST,
				STOCKMST."STD_COST" * STOCK_BR."IN_STOCK" AS EXT_COST,
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				Dept."DEPT_NAME",
				STOCK_BR."IN_STOCK" AS QTY_ON_HAND
			FROM
				(CSCMaster.dbo.STOCKMST STOCKMST 
				LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT" ) 
				INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE" 
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' AND
				STOCKMST."DEPT" <> '01' AND Dept.Dept between @dept1 and @dept2
			ORDER BY
				STOCKMST."DEPT" ASC
		else
			SELECT
				STOCKMST."STOCK_CODE",
				STOCKMST."DESCRIPT",
				STOCKMST."STOCK_TYPE",
				STOCKMST."STD_COST" AS UNIT_COST,
				STOCKMST."STD_COST" * STOCK_BR."IN_STOCK" AS EXT_COST,
				STOCKMST."AVG_COST",
				STOCKMST."DEPT",
				Dept."DEPT_NAME",
				STOCK_BR."IN_STOCK" AS QTY_ON_HAND
			FROM
				(CSCMaster.dbo.STOCKMST STOCKMST 
				LEFT OUTER JOIN CSCMaster.dbo.Dept Dept ON STOCKMST."DEPT" = Dept."DEPT" ) 
				INNER JOIN CSCMaster.dbo.STOCK_BR STOCK_BR ON STOCKMST."STOCK_CODE" = STOCK_BR."STOCK_CODE" 
			WHERE
				STOCKMST."STOCK_TYPE" = 'V' AND
				STOCKMST."DEPT" <> '01' AND Dept.Dept between @dept1 and @dept2
				and STOCK_BR.IN_STOCK <> @qty 
			ORDER BY
				STOCKMST."DEPT" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK5010')
DROP PROCEDURE Sproc_SK5010
go
CREATE PROCEDURE [dbo].[Sproc_SK5010] 
	-- Add the parameters for the stored procedure here
	@date1		DATETIME,
	@date2		DATETIME,
    @stock1		NVARCHAR(10),
    @stock2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;

	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @stock1 = 'All'
		SELECT
			StockHist."BatchDate",
			StockHist."Batchid",
			StockHist."StockCode",
			StockHist."Description",
			StockHist."Qty",
			StockHist."Purpose",
			StockHist."Inv_Num",
			StockHist."Cost",
			StockHist."Assoc",
			StockHist."Taxes",
			StockHist."Total"
		FROM
			CSCTrans.dbo.StockHist StockHist
		WHERE
			StockHist."Purpose" = 1 
			and (StockHist."BatchDate" between @date1 and @date2)
		ORDER BY
			StockHist."BatchDate" ASC,
			StockHist."Batchid" ASC,
			StockHist."Inv_Num" ASC
	else
		SELECT
			StockHist."BatchDate",
			StockHist."Batchid",
			StockHist."StockCode",
			StockHist."Description",
			StockHist."Qty",
			StockHist."Purpose",
			StockHist."Inv_Num",
			StockHist."Cost",
			StockHist."Assoc",
			StockHist."Taxes",
			StockHist."Total"
		FROM
			CSCTrans.dbo.StockHist StockHist
		WHERE
			StockHist."Purpose" = 1 
			and (StockHist."BatchDate" between @date1 and @date2)
			and (StockHist."StockCode" between @stock1 and @stock2)
		ORDER BY
			StockHist."BatchDate" ASC,
			StockHist."Batchid" ASC,
			StockHist."Inv_Num" ASC
END

GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK5011')
DROP PROCEDURE Sproc_SK5011
go
CREATE PROCEDURE [dbo].[Sproc_SK5011] 
	-- Add the parameters for the stored procedure here
	@date1		DATETIME,
	@date2		DATETIME,
	@dept1		NVARCHAR(10),
	@dept2		NVARCHAR(10),
    @stock1		NVARCHAR(10),
    @stock2		NVARCHAR(10) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @dept1 = 'All'
		if @stock1 = 'All'
			SELECT
				StockHist."BatchDate",
				StockHist."Batchid",
				StockHist."StockCode",
				StockHist."Description",
				StockHist."Qty",
				StockHist."BookQty",
				StockHist."Cost",
				STOCKMST."STOCK_TYPE",
				STOCKMST."DEPT",
				STOCKMST."SUB_DEPT",
				KIT_ITEM."STOCK_CODE",
				KIT_ITEM."QUANTITY"
			FROM
				(CSCTrans.dbo.StockHist StockHist 
				inner JOIN CSCMaster.dbo.STOCKMST STOCKMST ON StockHist."StockCode" = STOCKMST."STOCK_CODE") 
				LEFT JOIN CSCMaster.dbo.KIT_ITEM KIT_ITEM ON StockHist."StockCode" = KIT_ITEM."KIT_CODE"
			WHERE StockHist."BatchDate" between @date1 and @date2
			ORDER BY
				StockHist."BatchDate" ASC, StockHist."Batchid" ASC, stockhist. "sequence"
		else
			SELECT
				StockHist."BatchDate",
				StockHist."Batchid",
				StockHist."StockCode",
				StockHist."Description",
				StockHist."Qty",
				StockHist."BookQty",
				StockHist."Cost",
				STOCKMST."STOCK_TYPE",
				STOCKMST."DEPT",
				STOCKMST."SUB_DEPT",
				KIT_ITEM."STOCK_CODE",
				KIT_ITEM."QUANTITY"
			FROM
				(CSCTrans.dbo.StockHist StockHist 
				inner JOIN CSCMaster.dbo.STOCKMST STOCKMST ON StockHist."StockCode" = STOCKMST."STOCK_CODE") 
				LEFT JOIN CSCMaster.dbo.KIT_ITEM KIT_ITEM ON StockHist."StockCode" = KIT_ITEM."KIT_CODE"
			WHERE (StockHist."BatchDate" between @date1 and @date2)
				and (StockHist."StockCode" between @stock1 and @stock2)
			ORDER BY
				StockHist."BatchDate" ASC, StockHist."Batchid" ASC, stockhist. "sequence"
	else
		if @stock1 = 'All'
			SELECT
				StockHist."BatchDate",
				StockHist."Batchid",
				StockHist."StockCode",
				StockHist."Description",
				StockHist."Qty",
				StockHist."BookQty",
				StockHist."Cost",
				STOCKMST."STOCK_TYPE",
				STOCKMST."DEPT",
				STOCKMST."SUB_DEPT",
				KIT_ITEM."STOCK_CODE",
				KIT_ITEM."QUANTITY"
			FROM
				(CSCTrans.dbo.StockHist StockHist 
				inner JOIN CSCMaster.dbo.STOCKMST STOCKMST ON StockHist."StockCode" = STOCKMST."STOCK_CODE") 
				LEFT JOIN CSCMaster.dbo.KIT_ITEM KIT_ITEM ON StockHist."StockCode" = KIT_ITEM."KIT_CODE"
			WHERE (StockHist."BatchDate" between @date1 and @date2)
				and (STOCKMST."DEPT" BETWEEN @dept1 and @dept2)
			ORDER BY
				StockHist."BatchDate" ASC, StockHist."Batchid" ASC, stockhist. "sequence"
		else
			SELECT
				StockHist."BatchDate",
				StockHist."Batchid",
				StockHist."StockCode",
				StockHist."Description",
				StockHist."Qty",
				StockHist."BookQty",
				StockHist."Cost",
				STOCKMST."STOCK_TYPE",
				STOCKMST."DEPT",
				STOCKMST."SUB_DEPT",
				KIT_ITEM."STOCK_CODE",
				KIT_ITEM."QUANTITY"
			FROM
				(CSCTrans.dbo.StockHist StockHist 
				inner JOIN CSCMaster.dbo.STOCKMST STOCKMST ON StockHist."StockCode" = STOCKMST."STOCK_CODE") 
				LEFT JOIN CSCMaster.dbo.KIT_ITEM KIT_ITEM ON StockHist."StockCode" = KIT_ITEM."KIT_CODE"
			WHERE (StockHist."BatchDate" between @date1 and @date2)
				and (STOCKMST."DEPT" BETWEEN @dept1 and @dept2)
				and (StockHist."StockCode" between @stock1 and @stock2)
			ORDER BY
				StockHist."BatchDate" ASC, StockHist."Batchid" ASC, stockhist. "sequence"
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK5020')
DROP PROCEDURE Sproc_SK5020
go
CREATE PROCEDURE [dbo].[Sproc_SK5020]
	@date1 DATETIME
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	
	SELECT 
		SM.DESCRIPT AS STOCK_DESCRIPTION,
		SM.STOCK_CODE AS STOCK_CODE,
		INV_DATE,
		QUANTITY,
		COST,
		SM.STOCK_CODE,
        SM.DEPT,
		D.DEPT_NAME,
		COST * QUANTITY AS TOTAL_COST
	FROM INVENTORYHIST I
	INNER JOIN [CSCMaster].[dbo].[STOCKMST] SM
	ON SM.STOCK_CODE = I.STOCK_CODE
	INNER JOIN [CSCMASTER].[DBO].[DEPT] D
		ON SM.DEPT = D.DEPT
	WHERE
		I.INV_DATE = @date1
	ORDER BY DEPT_NAME, STOCK_DESCRIPTION
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK5030')
DROP PROCEDURE Sproc_SK5030
go
CREATE PROCEDURE [dbo].[Sproc_SK5030] 
	@Dept NVARCHAR(10),
	@SubDept NVARCHAR(10),
	@Vendor NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

with cte_1 
as 
(  select  
    ROW_NUMBER() over (order by v1.STOCK_CODE) as m  
	,ROW_NUMBER() over (order by v1.STOCK_CODE)-1 as n  
	,v1.STOCK_CODE  ,v2.DESCRIPT  
	,v3.Price  
	from [CSCMaster].[dbo].[STOCK_BR] v1  
	INNER JOIN [CSCMaster].[dbo].[STOCKMST] v2 on v2.STOCK_CODE=v1.STOCK_CODE  
	inner join [CSCMaster].[dbo].[Stock_Prices] v3 on v3.Stock_Code=v1.STOCK_CODE and v3.VendorID=v2.Vendor and v3.Price_Number=1 and v3.OrderNo=0  
	inner join [CSCMaster].[dbo].[Dept] v4 on v4.DEPT=v2.DEPT  
	left join [CSCMaster].[dbo].[SubDept] v5 on v5.SUB_DEPT=v2.SUB_DEPT and v5.DEPT=v2.DEPT  
	where ISNUMERIC(v1.STOCK_CODE)>0 
	and len(v1.STOCK_CODE)>=12 
	and (@Vendor='All' or v2.Vendor=@Vendor)
	and (@Dept='All' or v4.DEPT=@Dept)
	and (@SubDept='All' or v5.SUB_DEPT=@SubDept)
	) 
,cte_2 
as 
(  select 
     ROW_NUMBER() over (order by v1.m) as ky
	 ,  v1.*  
	 from cte_1 v1  
	 inner join cte_1 v2 on v1.m=v2.n*3+1 )  
,cte_3 
as 
(  select 
     ROW_NUMBER() over (order by v1.m) as ky
	 ,  v1.*  
	 from cte_1 v1  
	 inner join cte_1 v2 on v1.m=v2.n*3+2 )  
, cte_4 
as 
(  select 
      ROW_NUMBER() over (order by v1.m) as ky  
	  ,v1.*  from cte_1 v1  
	  inner join cte_1 v2 on v1.m=v2.n*3+3 )  
,cte_5 
as 
(  select isnull(v1.STOCK_CODE,'') as STOCK_CODE1  
          ,UPPER(left(v1.DESCRIPT,22)) as DESCRIPT1  
		  ,v1.Price as Price1  
		  ,'' as BarCode1  
		  ,isnull(v2.STOCK_CODE,'') as STOCK_CODE2  
		  ,UPPER(left(v2.DESCRIPT,22)) as DESCRIPT2  
		  ,v2.Price as Price2  
		  ,'' as BarCode2  
		  ,isnull(v3.STOCK_CODE,'') as STOCK_CODE3  
		  ,UPPER(left(v3.DESCRIPT,22)) as DESCRIPT3  
		  ,v3.Price as Price3  
		  ,'' as BarCode3  
		  from cte_2 v1  
		  left join cte_3 v2 on v1.ky=v2.ky  
		  left join cte_4 v3 on v1.ky=v3.ky )  
		  select  * from cte_5 
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SK5040')
DROP PROCEDURE Sproc_SK5040
go
CREATE PROCEDURE [dbo].[Sproc_SK5040] 
	@Dept NVARCHAR(10),
	@SubDept NVARCHAR(10),
	@Vendor NVARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

with cte_1 
as 
(  select  
    ROW_NUMBER() over (order by v1.STOCK_CODE) as m  
	,ROW_NUMBER() over (order by v1.STOCK_CODE)-1 as n  
	,v1.STOCK_CODE  ,v2.DESCRIPT  
	,v3.Price  
	from [CSCMaster].[dbo].[STOCK_BR] v1  
	INNER JOIN [CSCMaster].[dbo].[STOCKMST] v2 on v2.STOCK_CODE=v1.STOCK_CODE  
	inner join [CSCMaster].[dbo].[Stock_Prices] v3 on v3.Stock_Code=v1.STOCK_CODE and v3.VendorID=v2.Vendor and v3.Price_Number=1 and v3.OrderNo=0  
	inner join [CSCMaster].[dbo].[Dept] v4 on v4.DEPT=v2.DEPT  
	left join [CSCMaster].[dbo].[SubDept] v5 on v5.SUB_DEPT=v2.SUB_DEPT and v5.DEPT=v2.DEPT  
	where ISNUMERIC(v1.STOCK_CODE)>0 
	
	and (@Vendor='All' or v2.Vendor=@Vendor)
	and (@Dept='All' or v4.DEPT=@Dept)
	and (@SubDept='All' or v5.SUB_DEPT=@SubDept)
	) 
,cte_2 
as 
(  select 
     ROW_NUMBER() over (order by v1.m) as ky
	 ,  v1.*  
	 from cte_1 v1  
	 inner join cte_1 v2 on v1.m=v2.n*3+1 )  
,cte_3 
as 
(  select 
     ROW_NUMBER() over (order by v1.m) as ky
	 ,  v1.*  
	 from cte_1 v1  
	 inner join cte_1 v2 on v1.m=v2.n*3+2 )  
, cte_4 
as 
(  select 
      ROW_NUMBER() over (order by v1.m) as ky  
	  ,v1.*  from cte_1 v1  
	  inner join cte_1 v2 on v1.m=v2.n*3+3 )  
,cte_5 
as 
(  select isnull(v1.STOCK_CODE,'') as STOCK_CODE1  
          ,UPPER(left(v1.DESCRIPT,22)) as DESCRIPT1  
		  ,v1.Price as Price1  
		  ,'' as BarCode1  
		  ,isnull(v2.STOCK_CODE,'') as STOCK_CODE2  
		  ,UPPER(left(v2.DESCRIPT,22)) as DESCRIPT2  
		  ,v2.Price as Price2  
		  ,'' as BarCode2  
		  ,isnull(v3.STOCK_CODE,'') as STOCK_CODE3  
		  ,UPPER(left(v3.DESCRIPT,22)) as DESCRIPT3  
		  ,v3.Price as Price3  
		  ,'' as BarCode3  
		  from cte_2 v1  
		  left join cte_3 v2 on v1.ky=v2.ky  
		  left join cte_4 v3 on v1.ky=v3.ky )  
		  select  * from cte_5 
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SL0200')
DROP PROCEDURE Sproc_SL0200
go
CREATE PROCEDURE [dbo].[Sproc_SL0200]
	@date1		DATETIME,
	@date2		DATETIME,
    @dept1		NVARCHAR(10),
    @dept2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @dept1 = 'All'
		SELECT top 10
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SUM(SALELINE."QUANTITY") AS "QUANTITY",
			SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ") AS "AMOUNT",
			SALELINE."DEPT"
		FROM
			CSCTrans.dbo.SALEHEAD SALEHEAD 
			INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."TILL" = SALELINE."TILL_NUM" 
			AND SALEHEAD."SALE_NO" = SALELINE."SALE_NO"
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
		GROUP BY SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."DEPT"
		ORDER BY
			SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ") Desc
	else
		SELECT top 10
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SUM(SALELINE."QUANTITY") AS "QUANTITY",
			SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ") AS "AMOUNT",
			SALELINE."DEPT"
		FROM
			CSCTrans.dbo.SALEHEAD SALEHEAD 
			INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."TILL" = SALELINE."TILL_NUM" 
			AND SALEHEAD."SALE_NO" = SALELINE."SALE_NO"
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
			and SALELINE.Dept between @dept1 and @dept2
		GROUP BY SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."DEPT"
		ORDER BY
			SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ") Desc
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SL0202')
DROP PROCEDURE Sproc_SL0202
go
CREATE PROCEDURE [dbo].[Sproc_SL0202]
	@date1		DATETIME,
	@date2		DATETIME,
    @dept1		NVARCHAR(10),
    @dept2		NVARCHAR(10) 
AS

BEGIN
	SET NOCOUNT ON;
	SET @date1 = CONVERT(DATE, @date1)
	SET @date2 = CONVERT(DATE, @date2)

	if @dept1 = 'All'
		SELECT top 10
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SUM(SALELINE."QUANTITY") AS "QUANTITY",
			SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ") AS "AMOUNT",
			SUM(SALELINE."QUANTITY"*SALELINE."COST") AS "COST",
			(SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ")-SUM(SALELINE."QUANTITY"*SALELINE."COST")) AS "MARGIN",
			SALELINE."DEPT"
		FROM
			CSCTrans.dbo.SALEHEAD SALEHEAD INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON
				SALEHEAD."SALE_NO" = SALELINE."SALE_NO"
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
		GROUP BY SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."DEPT"
		ORDER BY
			(SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ")-SUM(SALELINE."QUANTITY"*SALELINE."COST")) Desc
	else
		SELECT top 10
			SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SUM(SALELINE."QUANTITY") AS "QUANTITY",
			SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ") AS "AMOUNT",
			SUM(SALELINE."QUANTITY"*SALELINE."COST") AS "COST",
			(SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ")-SUM(SALELINE."QUANTITY"*SALELINE."COST")) AS "MARGIN",
			SALELINE."DEPT"
		FROM
			CSCTrans.dbo.SALEHEAD SALEHEAD 
			INNER JOIN CSCTrans.dbo.SALELINE SALELINE ON SALEHEAD."TILL" = SALELINE."TILL_NUM" 
			AND SALEHEAD."SALE_NO" = SALELINE."SALE_NO"
		WHERE SALEHEAD.SHIFTDATE between @date1 and @date2
			and SALELINE.Dept between @dept1 and @dept2
		GROUP BY SALELINE."STOCK_CODE",
			SALELINE."DESCRIPT",
			SALELINE."DEPT"
		ORDER BY
			(SUM(SALELINE."AMOUNT"-SALELINE."DISCOUNT"-SALELINE."DISC_ADJ")-SUM(SALELINE."QUANTITY"*SALELINE."COST")) Desc
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SL0204')
DROP PROCEDURE Sproc_SL0204
go
CREATE PROCEDURE [dbo].[Sproc_SL0204]
	@date1 DATETIME,
	@date2 DATETIME
AS

BEGIN
	DECLARE @STARTDATE DATETIME
	DECLARE @ENDDATE DATETIME
	SET NOCOUNT ON;
	SET @STARTDATE = CONVERT(DATE, @date1)
	SET @ENDDATE = CONVERT(DATE, @date2)

	SET NOCOUNT ON;

	SELECT U.DESCRIPT, U.STOCK_CODE, U.QUANTITY, V.AMOUNT
	FROM 
	(
		SELECT TOP(20) SL.STOCK_CODE, SM.DESCRIPT, SUM(QUANTITY) AS QUANTITY FROM
		SALELINE SL
		INNER JOIN SALEHEAD SH
			ON SL.SALE_NO = SH.SALE_NO AND SL.TILL_NUM = SH.TILL
		LEFT JOIN CSCMaster.dbo.STOCKMST SM
			ON SL.STOCK_CODE = SM.STOCK_CODE
		WHERE 
			SH.SHIFTDATE BETWEEN @STARTDATE AND @ENDDATE
			AND SL.SALE_TYPE IN ('SALE', 'REFUND', 'PATP_APP') 
		GROUP BY SL.STOCK_CODE, SM.DESCRIPT
		ORDER BY SUM(QUANTITY) DESC
	) AS U

	LEFT JOIN 
	(
		SELECT SL.STOCK_CODE, SUM(AMOUNT) AS AMOUNT FROM
		SALELINE SL
		INNER JOIN SALEHEAD SH
			ON SL.SALE_NO = SH.SALE_NO AND SL.TILL_NUM = SH.TILL
		WHERE 
			SH.SHIFTDATE BETWEEN @STARTDATE AND @ENDDATE
			AND SH.T_TYPE IN ('SALE', 'REFUND', 'PATP_APP')
		GROUP BY SL.STOCK_CODE
	) AS V
	ON U.STOCK_CODE = V.STOCK_CODE
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SL0205')
DROP PROCEDURE Sproc_SL0205
go
CREATE PROCEDURE [dbo].[Sproc_SL0205]
	@date1 DATETIME,
	@date2 DATETIME,
	@dept1 NVARCHAR(10),
	@dept2 NVARCHAR(10),
	@stock1 NVARCHAR(10),
	@stock2 NVARCHAR(10)
AS

BEGIN
	DECLARE @STARTDATE  AS DATETIME
	DECLARE @ENDDATE	AS DATETIME
	DECLARE @DEPT	    NVARCHAR(10)

	SET NOCOUNT ON;
	SET @STARTDATE = CONVERT(DATE, @date1)
	SET @ENDDATE = CONVERT(DATE, @date2)

	SET NOCOUNT ON;

	if @dept1 = 'All'
		SELECT DEPT, 
			   DEPT_NAME, 
			   SALEDATE, 
			   SALEHOUR, 
			   SUM(QTY) AS QTY, 
			   SUM(AMOUNT) AS AMOUNT,
				CASE 
					WHEN SALEHOUR % 12 = 0 THEN '12.00 ' 
							+ 
							CASE WHEN SALEHOUR < 12  THEN 'AM  '
								ELSE 'PM  '
							END
					ELSE CONVERT(VARCHAR, SALEHOUR % 12) + '.00 '
							+ 
							CASE WHEN SALEHOUR < 12  THEN 'AM  '
								ELSE 'PM  '
							END
				END
					+
				CASE	
					WHEN (SALEHOUR + 1) % 12 = 0 THEN '12.00 ' 
							+ 
							CASE WHEN (SALEHOUR + 1 ) < 12  THEN 'AM  '
								WHEN (SALEHOUR + 1 ) = 24  THEN 'AM  '
								ELSE 'PM  '
							END
					ELSE CONVERT(VARCHAR, (SALEHOUR + 1) % 12) + '.00 '
							+ 
							CASE 
								WHEN (SALEHOUR + 1 ) < 12  THEN 'AM  '
								ELSE 'PM  '
							END
				END AS HOURRANGE,
				U.STOCK_CODE,
				U.DESCRIPT

		FROM
		(
			SELECT 
			D.DEPT_NAME,
			D.DEPT, 
			SL.STOCK_CODE,
			SL.DESCRIPT,
			SL.QUANTITY AS QTY,
			CONVERT(VARCHAR, SH.SALE_DATE, 101) AS SALEDATE,
			DATEPART(HOUR, SH.SALE_TIME) AS SALEHOUR,
			(ISNULL(SL.AMOUNT, 0) - ISNULL(SL.DISCOUNT, 0) - ISNULL(SL.DISC_ADJ, 0)) AS AMOUNT

			FROM
				[CSCTRANS].[DBO].[SALELINE] SL 
				INNER JOIN 
				[CSCTRANS].[DBO].[SALEHEAD] SH ON
					SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM 
				INNER JOIN 
					CSCMaster.DBO.Dept D
				ON D.DEPT = SL.DEPT

			WHERE 
				CONVERT(DATE, SH.SALE_DATE) BETWEEN @STARTDATE AND @ENDDATE
			AND SH.T_TYPE IN ('SALE', 'REFUND', 'PATP_APP')
		)
		AS U
		GROUP BY U.SALEDATE, U.SALEHOUR, U.DEPT, U.DEPT_NAME,U.STOCK_CODE,U.DESCRIPT
	else
		SELECT DEPT, 
			   DEPT_NAME, 
			   SALEDATE, 
			   SALEHOUR, 
			   SUM(QTY) AS QTY, 
			   SUM(AMOUNT) AS AMOUNT,
				CASE 
					WHEN SALEHOUR % 12 = 0 THEN '12.00 ' 
							+ 
							CASE WHEN SALEHOUR < 12  THEN 'AM  '
								ELSE 'PM  '
							END
					ELSE CONVERT(VARCHAR, SALEHOUR % 12) + '.00 '
							+ 
							CASE WHEN SALEHOUR < 12  THEN 'AM  '
								ELSE 'PM  '
							END
				END
					+
				CASE	
					WHEN (SALEHOUR + 1) % 12 = 0 THEN '12.00 ' 
							+ 
							CASE WHEN (SALEHOUR + 1 ) < 12  THEN 'AM  '
								WHEN (SALEHOUR + 1 ) = 24  THEN 'AM  '
								ELSE 'PM  '
							END
					ELSE CONVERT(VARCHAR, (SALEHOUR + 1) % 12) + '.00 '
							+ 
							CASE 
								WHEN (SALEHOUR + 1 ) < 12  THEN 'AM  '
								ELSE 'PM  '
							END
				END AS HOURRANGE,
				U.STOCK_CODE,
				U.DESCRIPT

		FROM
		(
			SELECT 
			D.DEPT_NAME,
			D.DEPT, 
			SL.STOCK_CODE,
			SL.DESCRIPT,
			SL.QUANTITY AS QTY,
			CONVERT(VARCHAR, SH.SALE_DATE, 101) AS SALEDATE,
			DATEPART(HOUR, SH.SALE_TIME) AS SALEHOUR,
			(ISNULL(SL.AMOUNT, 0) - ISNULL(SL.DISCOUNT, 0) - ISNULL(SL.DISC_ADJ, 0)) AS AMOUNT

			FROM
				[CSCTRANS].[DBO].[SALELINE] SL 
				INNER JOIN 
				[CSCTRANS].[DBO].[SALEHEAD] SH ON
					SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM 
				INNER JOIN 
					CSCMaster.DBO.Dept D
				ON D.DEPT = SL.DEPT

			WHERE 
				CONVERT(DATE, SH.SALE_DATE) BETWEEN @STARTDATE AND @ENDDATE
			AND SH.T_TYPE IN ('SALE', 'REFUND', 'PATP_APP')
			AND (SL.DEPT BETWEEN @dept1 and @dept2)
			AND (SL.STOCK_CODE  BETWEEN @stock1 and @stock2)
		)
		AS U
		GROUP BY U.SALEDATE, U.SALEHOUR, U.DEPT, U.DEPT_NAME,U.STOCK_CODE,U.DESCRIPT
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SS9010')
DROP PROCEDURE Sproc_SS9010
go
CREATE PROCEDURE [dbo].[Sproc_SS9010]
AS

BEGIN
	SET NOCOUNT ON;

	SET NOCOUNT ON;
	SELECT
		UIG."USER",
		UIG."UGROUP",
		[USER]."U_NAME"
	FROM
		CSCAdmin.dbo.UIG UIG INNER JOIN CSCAdmin.dbo.[USER] [USER] ON
			UIG."USER" = [USER]."U_CODE"
	ORDER BY
		UIG."USER" ASC,
		[USER]."U_NAME" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SS9020')
DROP PROCEDURE Sproc_SS9020
go
CREATE PROCEDURE [dbo].[Sproc_SS9020]
AS

BEGIN
	SET NOCOUNT ON;

	SELECT
		UGROUP."UGROUP",
		UGROUP."UG_NAME"
	FROM
		CSCAdmin.dbo.UGROUP UGROUP
END
GO


-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_SS9050')
DROP PROCEDURE Sproc_SS9050
go
CREATE PROCEDURE [dbo].[Sproc_SS9050]
AS

BEGIN
	SET NOCOUNT ON;

	SELECT
		[USER]."U_CODE",
		[USER]."U_NAME",
		UGROUP."UGROUP",
		UGROUP."UG_NAME"
	FROM
		(CSCAdmin.dbo.[USER] [USER] INNER JOIN CSCAdmin.dbo.UIG UIG ON
			[USER]."U_CODE" = UIG."USER")
		INNER JOIN CSCAdmin.dbo.UGROUP UGROUP ON
			UIG."UGROUP" = UGROUP."UGROUP"
	ORDER BY
		UGROUP."UGROUP" ASC,
		[USER]."U_CODE" ASC
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_TE4045')
DROP PROCEDURE Sproc_TE4045
go
CREATE PROCEDURE [dbo].[Sproc_TE4045]
	@date1 DATETIME
AS

BEGIN
	DECLARE @INVDATE AS DATETIME
	SET NOCOUNT ON;
	SET @INVDATE = CONVERT(DATE, @date1)
	
	SELECT 
		SM.DESCRIPT AS STOCK_DESCRIPTION,
		SM.STOCK_CODE AS STOCK_CODE,
		INV_DATE,
		QUANTITY,
		COST,
		SM.STOCK_CODE,
        SM.DEPT,
		D.DEPT_NAME,
		COST * QUANTITY AS TOTAL_COST
	FROM INVENTORYHIST I
	INNER JOIN [CSCMaster].[dbo].[STOCKMST] SM
	ON SM.STOCK_CODE = I.STOCK_CODE
	INNER JOIN [CSCMASTER].[DBO].[DEPT] D
		ON SM.DEPT = D.DEPT
	WHERE
		I.INV_DATE = @INVDATE
	ORDER BY DEPT_NAME, STOCK_DESCRIPTION
END
GO

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sproc_WK0100')
DROP PROCEDURE Sproc_WK0100
go
CREATE PROCEDURE [dbo].[Sproc_WK0100]
AS

BEGIN
	DECLARE @TODAY DATETIME
	SET NOCOUNT ON;
	SET @TODAY = CONVERT(DATE, getdate())

	SET NOCOUNT ON;
    DECLARE @DEPT NVARCHAR(10) 
    DECLARE @DEPT_NAME NVARCHAR(35) 

	DECLARE @OUTPUT TABLE (
		DEPT VARCHAR(10),
		DEPT_NAME VARCHAR(35),
		WEEK1 MONEY,
		WEEK2 MONEY,
		WEEK3 MONEY,
		WEEK4 MONEY,
		WEEK5 MONEY,
		WEEK6 MONEY
	)

	DECLARE DB_CURSOR CURSOR FOR 
	SELECT  DEPT, DEPT_NAME 
			FROM [CSCMASTER].[DBO].[DEPT]

	OPEN DB_CURSOR  
	FETCH NEXT FROM DB_CURSOR INTO @DEPT, @DEPT_NAME
	WHILE @@FETCH_STATUS = 0  
	BEGIN  

		DECLARE @Counter INT 
		DECLARE @START_DATE DATETIME
		DECLARE @END_DATE DATETIME

		DECLARE @W1 MONEY
		DECLARE @W2 MONEY
		DECLARE @W3 MONEY
		DECLARE @W4 MONEY
		DECLARE @W5 MONEY
		DECLARE @W6 MONEY

		SET @Counter=0
		WHILE ( @Counter <= 5)
		BEGIN
			SELECT @END_DATE = @TODAY - @Counter * 7
			SELECT @START_DATE = @TODAY - (@Counter + 1) * 7 + 1
			DECLARE @AMT MONEY

			SELECT @AMT = ISNULL(SUM(SL.AMOUNT), 0) 
						FROM CSCTrans.DBO.SALELINE SL
						INNER JOIN CSCTrans.DBO.SALEHEAD SH ON
						SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM

					WHERE @DEPT = SL.DEPT
					AND SH.T_TYPE IN ('SALE', 'REFUND', 'PATT_APP')
					AND SH.ShiftDate BETWEEN  @START_DATE AND @END_DATE

				IF @Counter = 0  SELECT @W6 = @AMT
				IF @Counter = 1  SELECT @W5 = @AMT
				IF @Counter = 2  SELECT @W4 = @AMT
				IF @Counter = 3  SELECT @W3 = @AMT
				IF @Counter = 4  SELECT @W2 = @AMT
				IF @Counter = 5  SELECT @W1 = @AMT

			SET @Counter  = @Counter  + 1
		END

		INSERT INTO @OUTPUT VALUES (
			@DEPT,
			@DEPT_NAME,
			@W1,
			@W2,
			@W3,
			@W4,
			@W5,
			@W6
		)
		
		FETCH NEXT FROM DB_CURSOR INTO @DEPT, @DEPT_NAME
	
	END
	CLOSE DB_CURSOR  
	DEALLOCATE DB_CURSOR 
		
	SELECT * FROM @OUTPUT
	ORDER BY DEPT

END
GO
--Jim end
