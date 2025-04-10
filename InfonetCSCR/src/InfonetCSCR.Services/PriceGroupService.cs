namespace InfonetCSCR.Services;

public class PriceGroupService : IPriceGroupService
{

  private readonly IUnitOfWork _unitOfWork;

  public PriceGroupService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<PriceGroup>> GetPriceGroupsByPriceGroupIdsAsync(IEnumerable<string> priceGroupIds)
  {
    IEnumerable<PriceGroup> priceGroups = await _unitOfWork.PriceGroups.GetPriceGroupsByPriceGroupIdsAsync(priceGroupIds);
    _unitOfWork.Commit();
    return priceGroups;
  }

  public async Task<IEnumerable<PriceGroup>> GetPriceGroupsListAsync()
  {
    var priceGroupsList = await _unitOfWork.PriceGroups.GetPriceGroupsListAsync();
    _unitOfWork.Commit();
    return priceGroupsList;
  }

  public async Task<bool> AddPriceGroupAsync(PriceGroup priceGroup)
  {
    bool isAdded = await _unitOfWork.PriceGroups.AddPriceGroupAsync(priceGroup);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeletePriceGroupAsync(string priceGroupId)
  {
    await _unitOfWork.PriceGroups.DeletePriceGroupAsync(priceGroupId, "Storelink");
    await _unitOfWork.PriceGroups.DeletePriceGroupAsync(priceGroupId, "PriceGroups");
    bool isDeleted = _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<IEnumerable<Store>?> GetStoreBySelectedPriceGroupAsync(string priceGroupId)
  {
    var stores = await _unitOfWork.PriceGroups.GetStoreBySelectedPriceGroupAsync(priceGroupId);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<IEnumerable<Store>?> GetStoreToLinkBySelectedPriceGroupAsync(string priceGroupId)
  {
    var stores = await _unitOfWork.PriceGroups.GetStoreToLinkBySelectedPriceGroupAsync(priceGroupId);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<bool> AddStoreToThePriceGroupAsync(StoreLink store)
  {
    StoreLink? currentStore = await _unitOfWork.PriceGroups.GetStoreFromStoreLinkByStoreIdAsync(store.Store_ID);
    bool isAdded;

    if (currentStore == null)
    {
      isAdded = await _unitOfWork.PriceGroups.AddStoreToStoreLinkAsync(store);
    }
    else
    {
      isAdded = await _unitOfWork.PriceGroups.UpdateStoreToStoreLinkAsync(currentStore, store.PriceGroup_Id);
    }

    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteStoreForSelectedPriceGroupAsync(StoreLink store)
  {
    StoreLink? currentStore = await _unitOfWork.PriceGroups.GetStoreFromStoreLinkByStoreIdAndPriceGroupIdAsync(store);
    bool isDeleted;

    if (currentStore == null)
    {
      return false;
    }

    if (string.IsNullOrWhiteSpace(currentStore.Region_Id) || currentStore.Region_Id.Length == 0)
    {
      isDeleted = await _unitOfWork.PriceGroups.DeleteStoreAsync(store);
    }
    else
    {
      isDeleted = await _unitOfWork.PriceGroups.UpdateStoreToStoreLinkAsync(currentStore, string.Empty);
    }

    _unitOfWork.Commit();
    return isDeleted;
  }
}
