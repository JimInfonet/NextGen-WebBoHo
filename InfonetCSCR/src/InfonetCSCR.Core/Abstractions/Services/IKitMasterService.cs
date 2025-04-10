namespace InfonetCSCR.Core.Abstractions.Services;

public interface IKitMasterService
{
  Task<bool> MakeStockItemKit(KitMast kitMast);
  Task<IEnumerable<KitsDto>> GetKits();
  Task<IEnumerable<KitItemsDto>> GetKitItemsByKitCodeWithDesc(string stockCode);
  Task<IEnumerable<KitItemsDto>> GetKitItems(bool isSellInactive);
  Task<bool> AddKitItem(KitItemsDto kitItem);
  Task<bool> IsKitItem(string stockCode);
  Task<bool> IsItemAlreadyInKit(KitItemsDto kitItem);
  Task<bool> UpdateKitItem(KitItemsDto kitItem);
  Task<bool> DeleteKitItem(KitItemsDto kitItem);
}
