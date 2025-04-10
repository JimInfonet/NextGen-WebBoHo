namespace InfonetCSCR.Services;

public class RegionService : IRegionService
{
  private readonly IUnitOfWork _unitOfWork;

  public RegionService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<Region>?> GetRegion()
  {
    IEnumerable<Region>? regions = await _unitOfWork.Region.GetRegion();
    _unitOfWork.Commit();
    return regions;
  }

  public async Task<IEnumerable<Region>> GetRegionsByRegionIds(IEnumerable<string> regionIds)
  {
    IEnumerable<Region> regions = await _unitOfWork.Region.GetRegionsByRegionIdsAsync(regionIds);
    _unitOfWork.Commit();
    return regions;
  }

  public async Task<bool> AddRegion(Region region)
  {
    bool isAdded = await _unitOfWork.Region.AddRegionAsync(region);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteRegion(string regionId)
  {
    bool isStoreLinkDeleted = await _unitOfWork.Region.DeleteRegionAsync(regionId, "Storelink");
    bool isRegionDeleted = await _unitOfWork.Region.DeleteRegionAsync(regionId, "Region");
    _unitOfWork.Commit();
    return isRegionDeleted;
  }
  
  public async Task<IList<Store>?> GetStoreToLinkBySelectedRegion(string regionId)
  {
    IList<Store>? stores = await _unitOfWork.StoreLinks.GetStoreToLinkBySelectedRegionAsync(regionId);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<bool> AddStoreToTheRegion(StoreLink store)
  {
    StoreLink? currentStore = await _unitOfWork.Region.GetStoreFromStoreLinkByStoreIdAsync(store.Store_ID);
    bool isAdded;

    if (currentStore == null)
    {
      isAdded = await _unitOfWork.Region.AddStoreToStoreLinkAsync(store);
    }
    else
    {
      isAdded = await _unitOfWork.Region.UpdateStoreToStoreLinkAsync(store.Region_Id, currentStore);
    }

    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteStoreForSelectedRegion(StoreLink store)
  {
    StoreLink? currentStore = await _unitOfWork.Region.GetStoreFromStoreLinkByStoreIdAndRegionIdAsync(store);
    bool isDeleted; 

    if (currentStore == null)
    {
      return false;
    }
    
    if (string.IsNullOrWhiteSpace(currentStore.PriceGroup_Id) || currentStore.PriceGroup_Id.Length == 0)
    {
      isDeleted = await _unitOfWork.Region.DeleteStoreAsync(store);
    } 
    else
    {
      store.Region_Id = "";
      isDeleted = await _unitOfWork.Region.UpdateStoreToStoreLinkAsync(store.Region_Id, currentStore);
    }

    _unitOfWork.Commit();
    return isDeleted;
  }
}
