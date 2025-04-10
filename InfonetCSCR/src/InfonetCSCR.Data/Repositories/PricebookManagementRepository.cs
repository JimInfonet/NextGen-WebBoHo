namespace InfonetCSCR.Data.Repositories;

public class PricebookManagementRepository : BaseRepository, IPricebookManagementRepository
{

  public PricebookManagementRepository(IDbTransaction transaction) : base(transaction)
  {
  }

  public async Task<IEnumerable<LabelPrintDto>?> GetLabelPrintingReportDataAsync()
  {
    string sql = $@"SELECT
                      STOCKMST.STOCK_CODE AS StockCode,
                      STOCKMST.DESCRIPT AS Description,
                      STOCKMST.STOCK_TYPE AS StockType,
                      Stock_Prices.Price AS Price,
                      VendorParts.PartNo AS PartNo
                    FROM
                      CSCMaster.dbo.STOCKMST STOCKMST
                    INNER JOIN
                      CSCMaster.dbo.Stock_Prices Stock_Prices
                    ON (STOCKMST.STOCK_CODE = Stock_Prices.Stock_Code
                      AND stockmst.vendor = stock_prices.Vendorid )
                        INNER JOIN CSCMaster.dbo.Stock_Br ON StockMst.Stock_Code = Stock_Br.Stock_Code
                        LEFT JOIN CSCMaster.dbo.VendorParts on StockMst.Stock_Code = VendorParts.Stock_Code
                        AND StockMst.Vendor = VendorParts.VendorID
                      WHERE (Stock_Prices.Price_Number = 1 AND stock_prices.Orderno = 0)
                        AND STOCK_BR.AVAIL >= 0
                        AND StockMst.Stock_Code IN
                        (SELECT STOCK_Code FROM CSCTransfer.dbo.LabelPrintSelected) 
                        ORDER BY STOCKMST.STOCK_CODE ASC, STOCKMST.DESCRIPT ASC";

    IEnumerable<LabelPrintDto>? stockItems = await Connection.QueryAsync<LabelPrintDto>(sql, null, Transaction);
    return stockItems;
  }

  public async Task<IEnumerable<LabelPrintDto>?> GetLabelPrintingStandardReportDataAsync()
  {
    string sql = $@"SELECT 
                      STOCKMST.STOCK_CODE AS StockCode,
                      STOCKMST.DESCRIPT AS Description,
                      STOCKMST.STOCK_TYPE AS StockType,
                      Stock_Prices.Price_Number, -- Include Price_Number for consistency
                      Stock_Prices.Price AS Price
                  FROM 
                      CSCMaster.dbo.STOCKMST STOCKMST
                  INNER JOIN 
                      CSCTransfer.dbo.LabelprintSelected LabelprintSelected 
                      ON STOCKMST.STOCK_CODE = LabelprintSelected.STOCK_CODE
                  LEFT JOIN 
                      CSCMaster.dbo.Stock_Prices Stock_Prices 
                      ON STOCKMST.STOCK_CODE = Stock_Prices.Stock_Code
                      AND STOCKMST.VENDOR = Stock_Prices.Vendorid
                  WHERE 
                      Stock_Prices.Price_Number = 1 
                      AND Stock_Prices.Orderno = 0
                  ORDER BY 
                      STOCKMST.STOCK_CODE ASC, 
                      STOCKMST.DESCRIPT ASC;";

    IEnumerable<LabelPrintDto>? stockItems = await Connection.QueryAsync<LabelPrintDto>(sql, null, Transaction);
    return stockItems;
  }

}
