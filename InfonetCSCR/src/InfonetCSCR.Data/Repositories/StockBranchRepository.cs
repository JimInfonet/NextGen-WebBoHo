namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StockBranchRepository : BaseRepository, IStockBranchRepository
{
  public StockBranchRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<StockBranch> GetStockBranchAsync(string stockCode)
  {
    const string sql = @"SELECT 
	                        LTRIM(RTRIM([STOCK_CODE])) AS StockCode,
                          [BRANCH] Branch,
                          [IN_STOCK] InStock,
                          [AVAIL] Avail,
                          [ONORDER_IC] OnOrderIC,
                          [ORDCOST_IC] OrderCostIC,
                          [ONORDER_PO] OnOrderPO,
                          [ALLOC_LAY] AllocLay,
                          [ALLOC_OE] AllocOE,
                          [RETURNED] Returned,
                          [HOLD] Hold,
                          [WASTE] Waste,
                          [STOCK_TAKE] StockTake,
                          [TAKE_ENTER] TakeEnter,
                          [MIN_QTY] MinQty,
                          [MAX_QTY] MaxQty,
                          [REORD_QTY] ReOrderQty,
                          [ST_DATE] StDate,
                          [LAST_RECV] LastRecv,
                          [LAST_ORDER] LastOrder,
                          [LAST_SALE] LastSale,
                          [MAXREORD_QTY] MaxReOrderQty,
                          [ActiveStock] ActiveStock
                        FROM [CSCMaster].[dbo].[STOCK_BR]
                        WHERE [STOCK_CODE] = @StockCode";

    var parameters = new { StockCode = stockCode };
    StockBranch resultStockBrunch = await Connection.QuerySingleOrDefaultAsync<StockBranch>(sql, parameters, Transaction);
    return resultStockBrunch;
  }

  public async Task<IEnumerable<StockBranch>> GetAllStockBranchAsync()
  {
    const string sql = @"SELECT 
	                        LTRIM(RTRIM([STOCK_CODE])) AS StockCode,
                          [BRANCH] Branch,
                          [IN_STOCK] InStock
                        FROM [CSCMaster].[dbo].[STOCK_BR];";

    IEnumerable<StockBranch> stockBranches = await Connection.QueryAsync<StockBranch>(sql, null, Transaction);
    return stockBranches;
  }

  public async Task<IEnumerable<StockBranch>> GetStockBranchesByStockCodeListAsync(List<string> stockCodes)
  {
    if (stockCodes.Count == 0) return new List<StockBranch>();
    string encodedStockCodes = "'" + string.Join("', '", stockCodes) + "'";
    StringBuilder query = new();

    string sql = $@"SELECT 
	                        LTRIM(RTRIM([STOCK_CODE])) AS StockCode,
                          [BRANCH] Branch,
                          [IN_STOCK] InStock,
                          [AVAIL] Avail,
                          [ONORDER_IC] OnOrderIC,
                          [ORDCOST_IC] OrderCostIC,
                          [ONORDER_PO] OnOrderPO,
                          [ALLOC_LAY] AllocLay,
                          [ALLOC_OE] AllocOE,
                          [RETURNED] Returned,
                          [HOLD] Hold,
                          [WASTE] Waste,
                          [STOCK_TAKE] StockTake,
                          [TAKE_ENTER] TakeEnter,
                          [MIN_QTY] MinQty,
                          [MAX_QTY] MaxQty,
                          [REORD_QTY] ReOrderQty,
                          [ST_DATE] StDate,
                          [LAST_RECV] LastRecv,
                          [LAST_ORDER] LastOrder,
                          [LAST_SALE] LastSale,
                          [MAXREORD_QTY] MaxReOrderQty,
                          [ActiveStock] ActiveStock
                        FROM [CSCMaster].[dbo].[STOCK_BR]
                        WHERE [STOCK_CODE] IN ({encodedStockCodes})";
    IEnumerable<StockBranch> resultStockBrunchs = await Connection.QueryAsync<StockBranch>(sql, null, Transaction);
    return resultStockBrunchs;
  }

  public async Task<bool> AddStockBranchAsync(StockBranch stockBranch)
  {
    const string sql = @"INSERT INTO [CSCMaster].[dbo].[STOCK_BR] (
	                        [STOCK_CODE],
                          [BRANCH],
                          [IN_STOCK],
                          [AVAIL],
                          [ONORDER_IC],
                          [ORDCOST_IC],
                          [ONORDER_PO],
                          [ALLOC_LAY],
                          [ALLOC_OE],
                          [RETURNED],
                          [HOLD],
                          [WASTE],
                          [STOCK_TAKE],
                          [TAKE_ENTER],
                          [MIN_QTY],
                          [MAX_QTY],
                          [REORD_QTY],
                          [ST_DATE],
                          [LAST_RECV],
                          [LAST_ORDER],
                          [LAST_SALE],
                          [MAXREORD_QTY],
                          [ActiveStock]
                        )
                        VALUES (  
                          @StockCode,
                          @Branch,
                          @InStock,
                          @Avail,
                          @OnOrderIC,
                          @OrderCostIC,
                          @OnOrderPO,
                          @AllocLay,
                          @AllocOE,
                          @Returned,
                          @Hold,
                          @Waste,
                          @StockTake,
                          @TakeEnter,
                          @MinQty,
                          @MaxQty,
                          @ReOrderQty,
                          @StDate,
                          @LastRecv,
                          @LastOrder,
                          @LastSale,
                          @MaxReOrderQty,
                          @ActiveStock
                        )";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockBranch.StockCode;
    cmd.Parameters.Add("@Branch", SqlDbType.VarChar, 4).Value = stockBranch.Branch;
    cmd.Parameters.Add("@InStock", SqlDbType.Real).Value = stockBranch.InStock ?? 0;
    cmd.Parameters.Add("@Avail", SqlDbType.Real, 25).Value = stockBranch.Avail ?? 0;
    cmd.Parameters.Add("@OnOrderIC", SqlDbType.Real).Value = stockBranch.OnOrderIC ?? 0;
    cmd.Parameters.Add("@OrderCostIC", SqlDbType.Real).Value = stockBranch.OrderCostIC ?? 0;
    cmd.Parameters.Add("@OnOrderPO", SqlDbType.Real).Value = stockBranch.OnOrderPO ?? 0;
    cmd.Parameters.Add("@AllocLay", SqlDbType.Real).Value = stockBranch.AllocLay ?? 0;
    cmd.Parameters.Add("@AllocOE", SqlDbType.Real).Value = stockBranch.AllocOE ?? 0;
    cmd.Parameters.Add("@Returned", SqlDbType.Real).Value = stockBranch.Returned ?? 0;
    cmd.Parameters.Add("@Hold", SqlDbType.Real).Value = stockBranch.Hold ?? 0;
    cmd.Parameters.Add("@Waste", SqlDbType.Real).Value = stockBranch.Waste ?? 0;
    cmd.Parameters.Add("@StockTake", SqlDbType.Real).Value = stockBranch.StockTake ?? 0;
    cmd.Parameters.Add("@TakeEnter", SqlDbType.Bit).Value = stockBranch.TakeEnter ?? false;
    cmd.Parameters.Add("@MinQty", SqlDbType.Real).Value = stockBranch.MinQty ?? 0;
    cmd.Parameters.Add("@MaxQty", SqlDbType.Real).Value = stockBranch.MaxQty ?? 0;
    cmd.Parameters.Add("@ReOrderQty", SqlDbType.Real).Value = stockBranch.ReOrderQty ?? 0;
    cmd.Parameters.Add("@StDate", SqlDbType.SmallDateTime).Value = stockBranch.StDate ?? (object)DBNull.Value;
    cmd.Parameters.Add("@LastRecv", SqlDbType.SmallDateTime).Value = stockBranch.LastRecv ?? (object)DBNull.Value;
    cmd.Parameters.Add("@LastOrder", SqlDbType.SmallDateTime).Value = stockBranch.LastOrder ?? (object)DBNull.Value;
    cmd.Parameters.Add("@LastSale", SqlDbType.SmallDateTime).Value = stockBranch.LastSale ?? (object)DBNull.Value;
    cmd.Parameters.Add("@MaxReOrderQty", SqlDbType.Real).Value = stockBranch.MaxReOrderQty ?? (object)DBNull.Value;
    cmd.Parameters.Add("@ActiveStock", SqlDbType.Bit).Value = stockBranch.ActiveStock ?? (object)DBNull.Value;

    cmd.CommandType = CommandType.Text;

    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> UpdateStockBranchAsync(StockBranch stockBranch)
  {
    const string sql = @"UPDATE [CSCMaster].[dbo].[STOCK_BR] 
                         SET
                          [MAX_QTY] = @MaxQty,
                          [REORD_QTY] = @ReOrderQty
                        WHERE [STOCK_CODE] = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockBranch.StockCode;
    cmd.Parameters.Add("@MaxQty", SqlDbType.Real).Value = stockBranch.MaxQty ?? 0;
    cmd.Parameters.Add("@ReOrderQty", SqlDbType.Real).Value = stockBranch.ReOrderQty ?? 0;

    cmd.CommandType = CommandType.Text;

    int rowAffected = cmd.ExecuteNonQuery();
    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteStockBranchByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from Stock_Br
                        Where Stock_Br.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> UpdateStockItemActiveStatusAsync(string stockCode, bool flag)
  {
    const string sql = @"Update Stock_Br Set ActiveStock = @Flag Where Stock_br.Stock_code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@Flag", SqlDbType.Bit).Value = flag;

    cmd.CommandType = CommandType.Text;

    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> UpdateInStockPOAsync(string productId, double qty, string sign)
  {
    string query = $@"UPDATE
                            [CSCMaster].[dbo].[Stock_Br]
                        SET
                            IN_STOCK = IN_STOCK {sign} @Qty,
                            Avail = Avail {sign} @Qty
                        WHERE
                            Stock_Code IN
                              (SELECT MST.STOCK_CODE FROM STOCKMST MST
                                  INNER JOIN Stock_Br BR
                                    ON MST.Stock_Code = @ProductId
                                    AND BR.Stock_Code = @ProductId)";
    var parameters = new { ProductId = productId, Qty = qty };
    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<decimal?> GetInStockByStockCodeAsync(string stockCode)
  {
    string query = $@"SELECT
                            IN_STOCK
                        FROM
                            [CSCMaster].[dbo].[Stock_Br]
                        WHERE
                            Stock_Code = @StockCode";
    var parameters = new { StockCode = stockCode };
    decimal result = await Connection.QueryFirstOrDefaultAsync<decimal>(query, parameters, Transaction);
    return result;
  }

}
