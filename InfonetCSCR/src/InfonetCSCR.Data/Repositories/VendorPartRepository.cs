namespace InfonetCSCR.Data.Repositories;

public class VendorPartRepository : BaseRepository, IVendorPartRepository
{
  public VendorPartRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> DeleteVendorPartByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from VendorParts
                        Where VendorParts.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<IEnumerable<VendorPart>> GetVendorPartsFromVendorPartsAndStockMstAsync(string sortByColumn, string vendorId)
  {
    string sql = @$"SELECT DISTINCT
                        PartNo,
                        VendorId,
                        LTRIM(RTRIM(vendorparts.Stock_code)) StockCode
                    FROM
                        [CSCMaster].[dbo].[VendorParts]
                    INNER JOIN
                        [CSCMaster].[dbo].[STOCKMST]
                    ON
                        StockMst.Stock_code = VendorParts.Stock_code 
                    WHERE
                        (stock_type = 'V' OR stock_type = 'O')
                        AND Vendorid = @VendorId
                    ORDER BY
                        Partno;";

    if(sortByColumn != null && sortByColumn != "")
    {
      sql = $@"SELECT
                  VendorParts.PartNo PartNo,
                  VendorParts.VendorId,
                  LTRIM(RTRIM(vendorparts.Stock_code)) StockCode
              FROM
                  [CSCMaster].[dbo].[VendorParts]
              INNER JOIN
                  [CSCMaster].[dbo].[STOCKMST]
              ON
                  StockMst.Stock_code = VendorParts.Stock_code
              WHERE
                  (stock_type = 'V' OR stock_type = 'O')
                  AND VendorParts.Vendorid = @VendorId
              ORDER BY {sortByColumn}";
    }

    var parameter = new { VendorId = vendorId };
    IEnumerable<VendorPart> vendorParts = await Connection.QueryAsync<VendorPart>(sql, parameter, Transaction);
    return vendorParts;
  }

  public async Task<IEnumerable<ProductAndVendorPartDataOfKitItemDto>> GetProductAndVendorPartDataOfKitItemsAsync(
    IList<(string VendorId, string StockCode)> vendorIDStockCodePairList)
  {
    string sql = @$"SELECT
                      LTRIM(RTRIM(StockMst.stock_code)) AS StockCode,
                      Packing,
                      OrderUnit,
                      StockMst.Dept Department,
                      StockMst.sub_dept SubDepartment,
                      StockMst.sub_detail SubDetail,
                      StockMst.descript Description,
                      VendorParts.PartNo,
                      VendorParts.VendorId
                  FROM
                      [CSCMaster].[dbo].[VendorParts]
                  INNER JOIN
                      [CSCMaster].[dbo].[STOCKMST] ON StockMst.Stock_code = VendorParts.Stock_code
                  INNER JOIN
                      [CSCMaster].[dbo].[STOCK_BR] ON StockMst.stock_code = Stock_Br.stock_code";

    if (vendorIDStockCodePairList.Count() > 0)
    {
      sql += " WHERE (";
      bool isFirstIteration = true;
      foreach ((string VendorId, string StockCode) vendorIDStockCodePair in vendorIDStockCodePairList)
      {
        if (!isFirstIteration)
        {
          sql += " OR";
        }

        isFirstIteration = false;
        sql += @$" (stockmst.stock_code = '{vendorIDStockCodePair.StockCode}'
                  and VendorParts.Vendorid = '{vendorIDStockCodePair.VendorId}')";
      }

      sql += ");";
    }
    else
    {
      sql += " WHERE (stockmst.stock_code = null and VendorParts.Vendorid = null)";
    }

    IEnumerable<ProductAndVendorPartDataOfKitItemDto> productAndVendorPartDataOfKitItems = await Connection.QueryAsync<ProductAndVendorPartDataOfKitItemDto>(sql, null, Transaction);
    return productAndVendorPartDataOfKitItems;
  }

  public async Task<IEnumerable<VendorPart>> GetVendorPartsByPartNosAsync(IList<string> partNos)
  {
    string sql = $@"SELECT LTRIM(RTRIM(STOCK_CODE)) AS StockCode, PartNo
                      FROM [CSCMaster].[dbo].[VendorParts]
                      WHERE PartNo IN @PartNos;";

    var parameter = new { PartNos = partNos };
    IEnumerable<VendorPart> vendorParts = await Connection.QueryAsync<VendorPart>(sql, parameter, Transaction);
    return vendorParts;
  }

  public async Task<IEnumerable<VendorPartDto>> GetVendorPartsFromVendorPartStockMstAndStockBrAsync()
  {
    string sql = $@"SELECT DISTINCT
                        VendorParts.PartNo PartNo,
                        VendorParts.Vendorid VendorId,
                        LTRIM(RTRIM(vendorparts.Stock_code)) AS StockCode,
                        Stockmst.Descript Description
                    FROM
                        [CSCMaster].[dbo].[VendorParts]
                    INNER JOIN
                        [CSCMaster].[dbo].[STOCKMST] ON Vendorparts.Stock_code = Stockmst.Stock_code
                    LEFT JOIN
                        [CSCMaster].[dbo].[STOCK_BR] ON Stock_Br.stock_code = Stockmst.Stock_code
                    WHERE
                        (stock_type = 'V' OR stock_type = 'O')
                        AND (Stock_br.ActiveStock = 1 OR Stock_br.ActiveStock IS NULL)
                    ORDER BY
                        VendorParts.Vendorid,
                        PartNo,
                        StockCode;";

    IEnumerable<VendorPartDto> vendorParts = await Connection.QueryAsync<VendorPartDto>(sql, null, Transaction);
    return vendorParts;
  }

  public async Task<VendorPartDto> GetVendorPartFromVendorPartStockBrByPartNumberAsync(string partNo)
  {
    string sql = $@"SELECT 
	                    LTRIM(RTRIM(vendorparts.Stock_code)) AS StockCode,
	                    Reord_qty ReOrderQuantity, 
	                    Max_qty MaximumQuantity,
	                    In_Stock InStock
                    from [CSCMaster].[dbo].[VendorParts] 
                    Inner Join [CSCMaster].[dbo].[STOCK_BR] 
                    on VendorParts.stock_code = stock_br.Stock_code 
                    where vendorparts.partno = @PartNo
                    and (Stock_br.ActiveStock =1  or  Stock_br.ActiveStock  is null);";

    var parameter = new { PartNo = partNo };
    VendorPartDto vendorPart = await Connection.QueryFirstOrDefaultAsync<VendorPartDto>(sql, parameter, Transaction);
    return vendorPart;
  }
}
