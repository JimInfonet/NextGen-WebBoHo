namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IKitMasterRepository
{
  Task<bool> MakeStockItemKitAsync(KitMast kitMast);
  Task<bool> DeleteKitMasterByStockCodeAsync(string stockCode);
  Task<IEnumerable<KitItem>> GetAllKitItemsAsync();
  Task<IEnumerable<KitMast>> GetAllKitMastsAsync();
  Task<Tuple<string, bool>> GetKitCodeByStockCodeAsync(string stockCode);
  Task<List<KitItem>> GetKitItemsByKitCodeAsync(string stockCode);
  Task<List<KitItemsDto>> GetKitItemsByKitCodeWithDescAsync(string stockCode);
  Task<IEnumerable<KitsDto>> GetKitsAsync();
  Task<IEnumerable<KitItemsDto>> GetKitItemsAsync(bool isSellInactive);
  Task<bool> AddKitItemAsync(KitItemsDto kitItem);
  Task<bool> IsKitItemAsync(string stockCode);
  Task<bool> IsItemAlreadyInKitAsync(KitItemsDto kitItem);
  Task<bool> UpdateKitItemAsync(KitItemsDto kitItem);
  Task<bool> DeleteKitItemAsync(KitItemsDto kitItem);
  Task<List<(string, string, double)>> GetStockCodeAndQuantityByKitCodeAsync(string stockCode);
}
