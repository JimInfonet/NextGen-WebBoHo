namespace InfonetCSCR.Core.Abstractions.Services;

public interface IKitItemService
{
  Task<bool> CheckKitItemExistsByStockCode(string stockCode);
}
