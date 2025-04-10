namespace InfonetCSCR.Web.Abstractions.Services;

public interface IPriceGroupService
{
  Task<IList<PriceGroupViewModel>?> GetAllPriceGroupsAsync();
  Task<bool> AddPriceGroupAsync(PriceGroupViewModel priceGroup);
  Task<bool> DeletePriceGroupAsync(string priceGroupId);
  Task<IEnumerable<StoreViewModel>?> GetStoreBySelectedPriceGroupAsync(string priceGroupId);
  Task<IEnumerable<StoreViewModel>?> GetStoreToLinkBySelectedPriceGroupAsync(string priceGroupId);
  Task<bool> AddStoreToThePriceGroupAsync(StoreLinkViewModel store);
  Task<bool> DeleteStoreForSelectedPriceGroupAsync(StoreLinkViewModel store);
}
