namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStockAcRepository
{
  Task<bool> AddStockAcByStockCodeAsync(string stockCode, string baseStockCode);
  Task<bool> AddStockAcAsync(string stockCode, string asCode, string storeId);
  Task<bool> DeleteStockAcByStockCodeAndStoreIdAsync(string stockCode, string storeId);
  Task<bool> DeleteStockAcByStockCodeAsync(string stockCode);
  Task<bool> InsertStockChargesByStockChargeDataAsync(IEnumerable<StockAc> stockCharges);
  Task<bool> DeleteStockChargesByStockChargeDataAsync(IEnumerable<StockAc> stockCharges);
}
