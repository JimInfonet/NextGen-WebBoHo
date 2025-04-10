namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class TaxExemptCategoryRepository : BaseRepository, ITaxExemptCategoryRepository
{
  public TaxExemptCategoryRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<TaxExemptCategory>> GetAllTaxExemptCategoryAsync()
  {
    const string sql = @"SELECT 
	                        [ID] Id,
	                        [Name] Name,
	                        [TaxFreeLimit] TaxFreeLimit,
	                        [LimitProductSale] LimitProductSale
                        FROM [CSCMaster].[dbo].[Category]";

    IEnumerable<TaxExemptCategory> taxExemptCategories = await Connection.QueryAsync<TaxExemptCategory>(sql, null, Transaction);
    return taxExemptCategories;
  }

  public async Task<IEnumerable<Reason>> GetTaxExemptReasonsListByTypeAsync(string reasonType)
  {
    const string sql = @"
      SELECT 
	      ReasonCode [Code],
	      ReasonDescription [Description]
      FROM [CSCMaster].[dbo].[TaxExemptReasons]
      WHERE ReasonType = @ReasonType
      ORDER BY ReasonType, ReasonCode";

    return await Connection.QueryAsync<Reason>(sql, new { ReasonType = reasonType }, Transaction);
  }
}
