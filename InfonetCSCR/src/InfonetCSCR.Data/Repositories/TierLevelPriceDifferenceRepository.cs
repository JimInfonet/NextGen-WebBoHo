namespace InfonetCSCR.Data.Repositories;

public class TierLevelPriceDifferenceRepository : BaseRepository, ITierLevelPriceDifferenceRepository
{
  public TierLevelPriceDifferenceRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<TierLevelPriceDifference>> GetTierLevelPriceDifferencesByStoreIdsAsync(IList<string> storeIds)
  {
    string sql = $@"SELECT 
                      [Store_ID] AS StoreId,
                      [Tier],
                      [Level],
                      [CashDiff] as CashDifference,
                      [CreditDiff] as CreditDifference,
                      [TaxExemptCashDiff] as TaxExemptCashDifference,
                      [TaxExemptCreditDiff] as TaxExemptCreditDifference
                  FROM [CSCPump].[dbo].[TierLevelPriceDiff]
                  WHERE Store_ID IN @StoreIds;";

    var parameters = new { StoreIds = storeIds };
    IList<TierLevelPriceDifference> tierLevelPriceDifferences = (IList<TierLevelPriceDifference>)await Connection.QueryAsync<TierLevelPriceDifference>(sql, parameters, Transaction);
    return tierLevelPriceDifferences;
  }
}
