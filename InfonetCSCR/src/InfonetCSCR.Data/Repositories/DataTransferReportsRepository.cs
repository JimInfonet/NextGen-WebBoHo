namespace InfonetCSCR.Data.Repositories;

//CSCTransfer Connection
public class DataTransferReportsRepository : BaseRepository, IDataTransferReportsRepository
{
  private readonly IApplicationConfig _config;
  public DataTransferReportsRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  public async Task<IEnumerable<CommodityTransferReportDto>> GetCommodityTransferReportDataAsync()
  {
    string sql = $@"SELECT 
                        CommodityTransfer.BatchId, 
                        CommodityTransfer.Dept, 
                        CommodityTransfer.SubDept, 
                        CommodityTransfer.SubDetail, 
                        CommodityTransfer.Description, 
                        CommodityTransfer.Store_Id AS StoreId, 
                        CommodityTransfer.Trans_Date AS TransDate, 
                        CommodityTransfer.Trans_User AS TransUser, 
                        CommodityTransfer.Trans_Flag AS TransFlag, 
                        CommodityTransfer.Trans_Type AS TransType, 
                        STORE.Store_Desc AS StoreDesc, 
                        A.U_NAME AS UserName
                    FROM 
                        CSCTransfer.dbo.CommodityTransfer CommodityTransfer 
                        INNER JOIN CSCMaster.[dbo].[STORE] STORE ON CommodityTransfer.Store_Id = STORE.Store_ID 
                        INNER JOIN CSCAdmin.[dbo].[USER] A ON CommodityTransfer.Trans_User = A.U_CODE  
                    WHERE 
                        CommodityTransfer.Trans_Flag = 'O' 
                    ORDER BY 
                        CommodityTransfer.Store_Id ASC";

    IEnumerable<CommodityTransferReportDto> commodityTransferReportData = await Connection.QueryAsync<CommodityTransferReportDto>(sql, null, Transaction);
    return commodityTransferReportData;
  }

  public async Task<IEnumerable<PriceChangeReportDto>> GetPriceChangeReportDataAsync()
  {
    string sql = $@"SELECT DISTINCT
                        PriceChangeTransfer.BatchId,
                        PriceChangeTransfer.ProductID AS ProductId,
                        PriceChangeTransfer.Description,
                        PriceChangeTransfer.PriceType,
                        PriceChangeTransfer.Store_id AS StoreId,
                        PriceChangeTransfer.VendorId,
                        PriceChangeTransfer.OrderNo,
                        CAST(PriceChangeTransfer.Trans_Date AS date) AS TransDate,
                        PriceDefinition.PR_Units AS PrUnits,
                        PriceDefinition.PR_From_Qty AS FromQty,
                        PriceDefinition.PR_To_Qty AS PrToQty,
                        PriceDefinition.Price AS PDPrice,
                        PriceDefinition.PR_FROM AS PrFrom,
                        PriceDefinition.PR_TO AS PrTo,
                        PriceDefinition.Margin,
                        Vendors.Name,
                        {(_config.IsBackOffice ? "" : " STORE.Store_Desc AS StoreDesc, ")}
                        Stock_Prices.Price_Number AS PriceNumber,
                        Stock_Prices.Price AS SPPrice
                    FROM
                        (( CSCTransfer.dbo.PriceChangeTransfer PriceChangeTransfer
                        INNER JOIN CSCTransfer.dbo.PriceDefinition PriceDefinition
                        ON PriceChangeTransfer.BatchId = PriceDefinition.Batchid
                        AND PriceChangeTransfer.ProductID = PriceDefinition.ProductId
                        AND PriceChangeTransfer.Store_id = PriceDefinition.Store_Id
                        AND PriceChangeTransfer.OrderNo = PriceDefinition.OrderNo
                        AND PriceChangeTransfer.VendorId = PriceDefinition.VendorId)
                        LEFT JOIN 
                            CSCMASTER.dbo.Stock_Prices Stock_prices
                            ON Stock_Prices.Stock_Code = PriceDefinition.ProductId
                            {(_config.IsBackOffice ? "" : " AND Stock_Prices.Storeid = PriceDefinition.Store_Id ")}
                            AND Stock_Prices.VendorId = PriceDefinition.VendorId)
                        {(_config.IsBackOffice ? "" : @" INNER JOIN
                            CSCMaster.dbo.Store STORE ON PriceChangeTransfer.Store_id = STORE.Store_ID")}
                        LEFT JOIN CSCMaster.dbo.Vendors Vendors
                            ON PriceChangeTransfer.VendorId = Vendors.Code
                    WHERE PRICECHANGETRANSFER.TRANS_FLAG = 'O'
                        AND ((Stock_prices.price_number = pricedefinition.price_number
                        AND pricechangetransfer.pricetype = 'R')
                    OR (Stock_prices.price_number = 1
                        AND pricechangetransfer.pricetype <> 'R'))
                         {(_config.IsBackOffice ? "" : "AND Stock_prices.orderno = 0")}
                    ORDER BY
                        PriceChangeTransfer.Store_id ASC,
                        PriceChangeTransfer.VendorId ASC";

    IEnumerable<PriceChangeReportDto> priceChangeReportData = await Connection.QueryAsync<PriceChangeReportDto>(sql, null, Transaction);
    return priceChangeReportData;
  }

  public async Task<IEnumerable<PromotionalDiscountTransferReportDto>> GetPromotionalDiscountTransferReportDataAsync()
  {
    string sql = $@"SELECT PromotionTransfer.BatchId,
                           PromotionTransfer.PromoID AS PromoId,
                           PromotionTransfer.Description,
                           PromotionTransfer.Store_Id AS StoreId,
                           PromotionTransfer.Trans_Date AS TransDate,
                           PromotionTransfer.Trans_User AS TransUser,
                           PromotionTransfer.Trans_Type AS TransType,
                           PromoHeader.Price,
                           PromoHeader.StartDate,
                           PromoHeader.EndDate,
                           PromoHeader.DiscType,
                           PromoHeader.Items,
                           PromoHeader.Day,
                           STORE.Store_Desc AS StoreDesc,
                           UserList.U_NAME AS UserName
                    FROM ((CSCTRANSFER.dbo.PromotionTransfer PromotionTransfer
                    INNER JOIN CSCMASTER.dbo.PromoHeader PromoHeader ON PromotionTransfer.PromoID = PromoHeader.PromoID)
                    INNER JOIN CSCMASTER.dbo.STORE STORE ON PromotionTransfer.Store_Id = STORE.Store_ID)
                    INNER JOIN CSCAdmin.dbo.[USER] UserList ON PromotionTransfer.Trans_User = UserList.U_CODE
                    WHERE PromotionTransfer.Trans_Flag = 'O'
                    ORDER BY PromotionTransfer.Store_Id ASC";
    IEnumerable<PromotionalDiscountTransferReportDto> promotionalDiscountTransferData = await Connection.QueryAsync<PromotionalDiscountTransferReportDto>(sql, null, Transaction);
    return promotionalDiscountTransferData;
  }

  public async Task<IEnumerable<StockTransferReportDto>> GetStockTransferReportDataAsync()
  {
    string sql = $@"Select StockTransfer.BatchId,
                        StockTransfer.ProductID AS ProductId,
                        StockTransfer.Description,
                        StockTransfer.Store_id AS StoreId,
                        StockTransfer.Trans_Date AS TransDate,
                        StockTransfer.Trans_User AS TransUser,
                        StockTransfer.Trans_Flag AS TransFlag,
                        StockTransfer.Trans_Type AS TransType,
                        A.U_NAME AS UserName
                    FROM
                        (CSCTransfer.dbo.StockTransfer StockTransfer
                        INNER JOIN CSCMaster.dbo.STORE STORE ON StockTransfer.Store_id = STORE.Store_ID)
                        INNER JOIN CSCAdmin.dbo.[USER] A ON StockTransfer.Trans_User = A.U_CODE
                    WHERE
                        StockTransfer.Trans_Flag = 'O'
                    ORDER BY
                        StockTransfer.Store_id ASC";

    IEnumerable<StockTransferReportDto> stockTransferReportData = await Connection.QueryAsync<StockTransferReportDto>(sql, null, Transaction);
    return stockTransferReportData;
  }

  public async Task<IEnumerable<HotButtonsReportDto>> GetHotButtonsReportDataAsync()
  {
    string sql = $@"SELECT HotButtonTransfer.BatchId, 
                    HotButtonTransfer.Button, 
                    HotButtonTransfer.ProductId, 
                    HotButtonTransfer.Description, 
                    HotButtonTransfer.Caption1, 
                    HotButtonTransfer.Caption2, 
                    HotButtonTransfer.Store_Id AS StoreId, 
                    HotButtonTransfer.Trans_Date AS TransDate, 
                    HotButtonTransfer.Trans_User AS TransUser, 
                    HotButtonTransfer.Trans_Flag AS TransFlag, 
                    HotButtonTransfer.Trans_Type AS TransType, 
                    STORE.Store_Desc AS StoreDesc, 
                    A.U_NAME AS Name
                    FROM (CSCTransfer.dbo.HotButtonTransfer HotButtonTransfer 
                    INNER JOIN CSCMaster.dbo.STORE STORE 
                    ON HotButtonTransfer.Store_Id = STORE.Store_ID) 
                    INNER JOIN CSCAdmin.dbo.[USER] A 
                    ON HotButtonTransfer.Trans_User = A.U_CODE  
                    WHERE HotButtonTransfer.Trans_Flag = 'O' 
                    ORDER BY HotButtonTransfer.Store_Id ASC";

    IEnumerable<HotButtonsReportDto> hotButtonsReportData = await Connection.QueryAsync<HotButtonsReportDto>(sql, null, Transaction);
    return hotButtonsReportData;
  }

  public async Task<IEnumerable<CostChangeReportDto>> GetCostChangeReportDataAsync()
  {
    string sql = $@"SELECT DISTINCT
                    CostChangeTransfer.BatchId,
                    CostChangeTransfer.ProductID AS ProductId,
                    CostChangeTransfer.Description,
                    CostChangeTransfer.StartDate,
                    CostChangeTransfer.EndDate,
                    CostChangeTransfer.OldCost,
                    CostChangeTransfer.NewCost,
                    CostChangeTransfer.Store_id AS StoreId,
                    CostChangeTransfer.VendorId,
                    CostChangeTransfer.OrderNo,
                    CAST(CostChangeTransfer.Trans_Date AS date) AS TransDate,
                    CostChangeTransfer.Trans_User AS TransUser,
                    CostChangeTransfer.Trans_Flag AS TransFlag,
                    Vendors.Name
                    {(_config.IsBackOffice ? "" : ", STORE.Store_Desc AS StoreDesc")}
                    FROM (CSCTransfer.dbo.CostChangeTransfer CostChangeTransfer 
                    LEFT JOIN CSCMaster.dbo.Vendors Vendors 
                    ON CostChangeTransfer.VendorId = Vendors.Code) 
                    {(_config.IsBackOffice ? "" : @" INNER JOIN CSCMaster.dbo.STORE STORE
                    ON CostChangeTransfer.Store_id = STORE.Store_ID ")}
                    WHERE CostChangeTransfer.Trans_Flag = 'O'
                    ORDER BY CostChangeTransfer.Store_id ASC,
                    CostChangeTransfer.ProductID ASC, 
                    CostChangeTransfer.BatchId ASC";

    IEnumerable<CostChangeReportDto> costChangeReportData = await Connection.QueryAsync<CostChangeReportDto>(sql, null, Transaction);
    return costChangeReportData;
  }
}
