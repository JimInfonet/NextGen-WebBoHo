namespace InfonetCSCR.Core.Abstractions.Services;

public interface IRegionService
{
  Task<IEnumerable<Region>?> GetRegion();
  Task<IEnumerable<Region>> GetRegionsByRegionIds(IEnumerable<string> regionIds);
  Task<bool> AddRegion(Region region);
  Task<bool> DeleteRegion(string regionId);
  Task<IList<Store>?> GetStoreToLinkBySelectedRegion(string regionId);
  Task<bool> AddStoreToTheRegion(StoreLink store);
  Task<bool> DeleteStoreForSelectedRegion(StoreLink store);
}
