namespace InfonetCSCR.Data.Repositories;

public class ServiceLevelRepository : BaseRepository, IServiceLevelRepository
{
  public ServiceLevelRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<ServiceLevel>> GetAllDistinctIdAndDescriptionFromServiceLevelAsync()
  {
    string sql = $@"SELECT DISTINCT
                        [ID] AS Id,
                        [Description] AS Description
                    FROM [CSCPump].[dbo].[ServiceLevel];";

    IList<ServiceLevel> serviceLevels = (IList<ServiceLevel>)await Connection.QueryAsync<ServiceLevel>(sql, null, Transaction);
    return serviceLevels;
  }

  public async Task<IList<ServiceLevel>> GetServiceLevelsByStoreIdsAsync(IList<string> storeIds)
  {
    string sql = $@"SELECT DISTINCT
                        [ID] AS Id,
                        [Store_ID] AS StoreId,
                        [Description] AS Description
                    FROM [CSCPump].[dbo].[ServiceLevel]
                    WHERE Store_ID IN @StoreIds;";

    var parameters = new { StoreIds = storeIds };
    IList<ServiceLevel> serviceLevels = (IList<ServiceLevel>)await Connection.QueryAsync<ServiceLevel>(sql, parameters, Transaction);
    return serviceLevels;
  }
}
