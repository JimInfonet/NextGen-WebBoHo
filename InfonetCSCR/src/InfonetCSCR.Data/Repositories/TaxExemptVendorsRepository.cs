namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class TaxExemptVendorsRepository : BaseRepository, ITaxExemptVendorsRepository
{
  public TaxExemptVendorsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<TaxExemptVendor>> GetAllTaxExemptVendorsAsync(string stockCode)
  {
    const string sql = @"SELECT distinct vendorid VendorId, name VendorName FROM stockcosts  inner join vendors on code = vendorid 
                         where Stock_code = @StockCode  and ISTEVendor  = 1 order by Vendorid ";

    var parameters = new { StockCode = stockCode };
    IEnumerable<TaxExemptVendor> taxExemptVendors = await Connection.QueryAsync<TaxExemptVendor>(sql, parameters, Transaction);
    return taxExemptVendors;
  }
}
