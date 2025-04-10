namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class PriceLRepository : BaseRepository, IPriceLRepository
{
  private readonly IApplicationConfig _applicationConfig;
  public PriceLRepository(IApplicationConfig applicationConfig, IDbTransaction transaction) : base(transaction)
  {
    _applicationConfig = applicationConfig;
  }

  public async Task<bool> AddPriceLByStockCodeAsync(string stockCode, string baseStockCode)
  {
    string sql = (_applicationConfig.IsBackOffice) ?
                        @"INSERT INTO [CSCMaster].[dbo].[PRICEL] (
                          Stock_Code, 
                          Pr_F_Qty, 
                          Pr_T_Qty, 
                          Price, 
                          Pr_Type, 
                          Pr_Units, 
                          VendorID, 
                          StartDate, 
                          EndDate,OrderNo
                          ) 
                          SELECT @StockCode AS Stock_Code, 
                            Pr_F_Qty, 
                            Pr_T_Qty, 
                            Price, 
                            Pr_Type, 
                            Pr_Units, 
                            VendorID, 
                            StartDate, 
                            EndDate, 
                            OrderNo
                          FROM [CSCMaster].[dbo].[PRICEL] 
                          WHERE Stock_Code=@BaseStockCode" :
                        @"INSERT INTO [CSCMaster].[dbo].[PRICEL] (
                          Stock_Code, 
                          Pr_F_Qty, 
                          Pr_T_Qty, 
                          Price, 
                          Pr_Type, 
                          Pr_Units,
                          StoreID,
                          VendorID, 
                          StartDate, 
                          EndDate,OrderNo
                          ) 
                          SELECT @StockCode AS Stock_Code, 
                            Pr_F_Qty, 
                            Pr_T_Qty, 
                            Price, 
                            Pr_Type, 
                            Pr_Units,
                            StoreID,
                            VendorID, 
                            StartDate, 
                            EndDate, 
                            OrderNo
                          FROM [CSCMaster].[dbo].[PRICEL] 
                          WHERE Stock_Code=@BaseStockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;
    cmd.Parameters.Add("@BaseStockCode", SqlDbType.VarChar, 16).Value = baseStockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeletePriceLByStockCodeAsync(string stockCode)
  {
    const string sql = @"Delete from PriceL
                        Where PriceL.Stock_Code = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = stockCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }
}
