namespace InfonetCSCR.Web.Abstractions.Services;

public interface IRegionService
{
  Task<bool> AddRegion(RegionViewModel region);
  Task<bool> DeleteRegion(string regionId);
  Task<IList<StoreViewModel>?> GetStoreToLinkBySelectedRegion(string regionId);
  Task<bool> AddStoreToTheRegion(StoreLinkViewModel store);
  Task<bool> DeleteStoreForSelectedRegion(StoreLinkViewModel store);
}
