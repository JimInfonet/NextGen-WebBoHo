namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITierLevelPriceDifferenceRepository
{
  Task<IList<TierLevelPriceDifference>> GetTierLevelPriceDifferencesByStoreIdsAsync(IList<string> storeIds);
}
