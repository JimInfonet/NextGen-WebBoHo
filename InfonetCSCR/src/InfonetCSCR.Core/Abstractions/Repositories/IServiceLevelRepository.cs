namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IServiceLevelRepository
{
  Task<IList<ServiceLevel>> GetAllDistinctIdAndDescriptionFromServiceLevelAsync();
  Task<IList<ServiceLevel>> GetServiceLevelsByStoreIdsAsync(IList<string> storeIds);
}
