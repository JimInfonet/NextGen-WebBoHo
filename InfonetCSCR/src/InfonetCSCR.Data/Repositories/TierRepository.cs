namespace InfonetCSCR.Data.Repositories;

public class TierRepository : BaseRepository, ITierRepository
{
  public TierRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<Tier>> GetAllDistinctIdAndDescriptionFromTierTableAsync()
  {
    string sql = $@"SELECT DISTINCT
                        [ID] AS Id,
                        [Description] AS Description
                    FROM [CSCPump].[dbo].[Tier];";

    IList<Tier> tiers = (IList<Tier>)await Connection.QueryAsync<Tier>(sql, null, Transaction);
    return tiers;
  }

  public async Task<IList<Tier>> GetTiersByStoreIdsAsync(IList<string> storeIds)
  {
    string sql = $@"SELECT DISTINCT
                        [ID] AS Id,
                        [Store_ID] AS StoreId,
                        [Description] AS Description
                    FROM [CSCPump].[dbo].[Tier]
                    WHERE Store_ID IN @StoreIds;";

    var parameters = new { StoreIds = storeIds };
    IList<Tier> tiers = (IList<Tier>)await Connection.QueryAsync<Tier>(sql, parameters, Transaction);
    return tiers;
  }
}
