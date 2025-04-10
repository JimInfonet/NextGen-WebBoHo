namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPriceGroupRepository
{
  Task<IEnumerable<PriceGroup>> GetPriceGroupsByPriceGroupIdsAsync(IEnumerable<string> priceGroupIds);
  Task<IEnumerable<PriceGroup>?> GetPriceGroupsListAsync();
  Task<bool> AddPriceGroupAsync(PriceGroup priceGroup);
  Task<bool> DeletePriceGroupAsync(string priceGroupId, string table);
  Task<IEnumerable<Store>?> GetStoreBySelectedPriceGroupAsync(string priceGroupId);
  Task<IEnumerable<Store>?> GetStoreToLinkBySelectedPriceGroupAsync(string priceGroupId);
  Task<StoreLink?> GetStoreFromStoreLinkByStoreIdAsync(string storeId);
  Task<bool> AddStoreToStoreLinkAsync(StoreLink store);
  Task<bool> UpdateStoreToStoreLinkAsync(StoreLink currentStore, string? newPriceGroupId);
  Task<StoreLink?> GetStoreFromStoreLinkByStoreIdAndPriceGroupIdAsync(StoreLink store);
  Task<bool> DeleteStoreAsync(StoreLink store);
}
