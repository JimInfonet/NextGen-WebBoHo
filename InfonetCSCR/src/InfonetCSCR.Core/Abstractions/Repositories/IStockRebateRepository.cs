namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStockRebateRepository
{
  Task<bool> AddStockRebateByStockCodeAsync(string stockCode, string baseStockCode);
  Task<bool> DeleteStockRebateByStockCodeAsync(string stockCode);
}
