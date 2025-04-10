namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStoreLinkRepository
{
  Task<IEnumerable<string>> GetStoreIdsByStoreIds(IEnumerable<string> storeIds);
  Task<IEnumerable<string>> GetStoreIdsByVendorIds(IEnumerable<string> vendorIds);
  Task<IEnumerable<Store>> GetStoresByPriceGroupIds(IEnumerable<string> priceGroupIds);
  Task<IEnumerable<Store>> GetStoresByRegionIds(IEnumerable<string> regionIds);
  Task<IEnumerable<Store>> GetAllStoresByRegionIds(IEnumerable<string> regionIds);
  Task<IList<Store>?> GetStoreToLinkBySelectedRegionAsync(string regionId);
}
