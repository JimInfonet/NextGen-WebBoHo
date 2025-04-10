namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStockBranchRepository
{
  Task<StockBranch> GetStockBranchAsync(string stockCode);
  Task<IEnumerable<StockBranch>> GetStockBranchesByStockCodeListAsync(List<string> stockCodes);
  Task<IEnumerable<StockBranch>> GetAllStockBranchAsync();
  Task<bool> AddStockBranchAsync(StockBranch stockBranch);
  Task<bool> UpdateStockBranchAsync(StockBranch stockBranch);
  Task<bool> DeleteStockBranchByStockCodeAsync(string stockCode);
  Task<bool> UpdateStockItemActiveStatusAsync(string stockCode, bool flag);
  Task<bool> UpdateInStockPOAsync(string productId, double qty, string sign);
  Task<decimal?> GetInStockByStockCodeAsync(string stockCode);
}
