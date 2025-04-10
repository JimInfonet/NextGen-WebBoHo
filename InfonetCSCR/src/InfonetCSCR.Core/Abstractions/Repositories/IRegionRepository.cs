namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IRegionRepository
{
  Task<IEnumerable<Region>?> GetRegion();
  Task<IEnumerable<Region>> GetRegionsByRegionIdsAsync(IEnumerable<string> storeIds);
  Task<bool> AddRegionAsync(Region region);
  Task<bool> DeleteRegionAsync(string regionId, string table);
  Task<StoreLink?> GetStoreFromStoreLinkByStoreIdAsync(string storeId);
  Task<bool> AddStoreToStoreLinkAsync(StoreLink store);
  Task<bool> UpdateStoreToStoreLinkAsync(string regionId, StoreLink currentStore);
  Task<StoreLink?> GetStoreFromStoreLinkByStoreIdAndRegionIdAsync(StoreLink store);
  Task<bool> DeleteStoreAsync(StoreLink store);
}
