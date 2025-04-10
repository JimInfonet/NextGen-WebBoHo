namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPriceGroupService
{
  Task<IEnumerable<PriceGroup>> GetPriceGroupsByPriceGroupIdsAsync(IEnumerable<string> priceGroupIds);
  Task<IEnumerable<PriceGroup>?> GetPriceGroupsListAsync();
  Task<bool> AddPriceGroupAsync(PriceGroup priceGroup);
  Task<bool> DeletePriceGroupAsync(string priceGroupId);
  Task<IEnumerable<Store>?> GetStoreBySelectedPriceGroupAsync(string priceGroupId);
  Task<IEnumerable<Store>?> GetStoreToLinkBySelectedPriceGroupAsync(string priceGroupId);
  Task<bool> AddStoreToThePriceGroupAsync(StoreLink store);
  Task<bool> DeleteStoreForSelectedPriceGroupAsync(StoreLink store);
}
