namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITierRepository
{
  Task<IList<Tier>> GetAllDistinctIdAndDescriptionFromTierTableAsync();
  Task<IList<Tier>> GetTiersByStoreIdsAsync(IList<string> storeIds);
}
