namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class TaxExemptRateRepository : BaseRepository, ITaxExemptRateRepository
{
  public TaxExemptRateRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<TaxExemptRate>> GetAllTaxExemptRateAsync()
  {
    const string sql = @"select 
	                        TAX_CODE TaxCode,
	                        TAX_RATE TaxRate,
	                        TAX_DESC TaxDescirption, 
	                        RateType RateType,
	                        ProductCode ProductCode,
	                        BaseQty BaseQuantity,
	                        BaseCode BaseCode
                        from TaxExemptRate";

    IEnumerable<TaxExemptRate> taxExemptRates = await Connection.QueryAsync<TaxExemptRate>(sql, null, Transaction);
    return taxExemptRates;
  }

  public async Task<TaxExemptRate?> GetTaxExemptRateByTaxCodeAsync(int taxCode)
  {
    const string sql = @"select 
	                        TAX_CODE TaxCode,
	                        TAX_RATE TaxRate,
	                        TAX_DESC TaxDescirption, 
	                        RateType RateType,
	                        ProductCode ProductCode,
	                        BaseQty BaseQuantity,
	                        BaseCode BaseCode,
	                        SurCharge SurCharge
                        from TaxExemptRate
                        where TAX_CODE = @TaxCode";

    var parameters = new { TaxCode = taxCode };
    TaxExemptRate? result = await Connection.QuerySingleOrDefaultAsync<TaxExemptRate?>(sql, parameters, Transaction);

    return result;
  }
}
