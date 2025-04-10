namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPriceLRepository
{
  Task<bool> AddPriceLByStockCodeAsync(string stockCode, string baseStockCode);
  Task<bool> DeletePriceLByStockCodeAsync(string stockCode);
}
