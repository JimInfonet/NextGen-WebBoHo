namespace InfonetCSCR.Web.Abstractions.Services;

public interface IKitsService
{
  Task<IEnumerable<KitsViewModel>?> GetKits();
  KitPoliciesViewModel GetKitPolicies();
  Task<IEnumerable<KitItemsViewModel>?> GetKitItemsByKitCodeWithDesc(string stockCode);
  Task<IEnumerable<KitItemsViewModel>?> GetKitItems(bool isSellInactive);
  Task<bool> AddKitItem(KitItemsViewModel kitItem);
  Task<bool> IsKitItem(string stockCode);
  Task<bool> IsItemAlreadyInKit(KitItemsViewModel kitItem);
  Task<bool> UpdateKitItem(KitItemsViewModel kitItem);
  Task<bool> DeleteKitItem(KitItemsViewModel kitItem);
}
