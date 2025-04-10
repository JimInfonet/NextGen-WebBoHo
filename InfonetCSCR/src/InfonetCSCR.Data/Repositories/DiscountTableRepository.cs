namespace InfonetCSCR.Data.Repositories;

public class DiscountTableRepository : BaseRepository, IDiscountTableRepository
{
  public DiscountTableRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<DiscountTable>> GetAllDiscountTableDataAsync(DiscountTablePolicyRecord discountTablePolicyRecord)
  {
    string sql = $@"SELECT 
                          Prod_Disc ProductDiscount, 
                          Cust_Disc CustomerDiscount, 
                          Disc_Perc DiscountPercentage
                      FROM 
                          [CSCMaster].[dbo].[DISCTAB]";

    if (discountTablePolicyRecord.IsCustomerDiscount && !discountTablePolicyRecord.IsProductDiscount)
    {
      sql += $@" WHERE Prod_Disc = 0";
    }

    if (!discountTablePolicyRecord.IsCustomerDiscount && discountTablePolicyRecord.IsProductDiscount)
    {
      sql += $@" WHERE Cust_Disc = 0";
    }

    sql += $@" ORDER BY Prod_Disc, Cust_disc;";

    IList<DiscountTable> discountTableData = (IList<DiscountTable>)await Connection.QueryAsync<DiscountTable>(sql, null, Transaction);
    return discountTableData;
  }

  public async Task<bool> InsertDataOnDiscountTableAsync(DiscountTable discountTable)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[DISCTAB] 
                      (Prod_Disc, Cust_Disc, Disc_Perc)
                     VALUES (@ProductDiscount, @CustomerDiscount, @DiscountPercentage);";


    int rowsAffected = await Connection.ExecuteAsync(sql, discountTable, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateDiscountPercentageAsync(DiscountTable discountTable)
  {
    string sql = @"UPDATE [CSCMaster].[dbo].[DISCTAB]
                   SET Disc_Perc = @DiscountPercentage
                   WHERE Prod_Disc = @ProductDiscount AND Cust_Disc = @CustomerDiscount;";

    int rowsAffected = await Connection.ExecuteAsync(sql, discountTable, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteDiscountAsync(DiscountTable discountTable)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[DISCTAB]
                   WHERE Prod_Disc = @ProductDiscount AND Cust_Disc = @CustomerDiscount;";

    int rowsAffected = await Connection.ExecuteAsync(sql, discountTable, Transaction);
    return rowsAffected > 0;
  }
}
