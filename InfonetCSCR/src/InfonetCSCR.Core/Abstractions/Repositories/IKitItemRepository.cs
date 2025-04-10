namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IKitItemRepository
{
  Task<bool> DeleteKitItemByKitCodeAsync(string kitCode);
  Task<bool> CheckKitItemExistsByStockCodeAsync(string stockCode);
  Task<bool> DeleteKitItemByStockCodeAsync(string stockCode);
  Task<IEnumerable<KitItem>> GetKitItemByStockCodesAsync(IList<string> stockCodes);
}
