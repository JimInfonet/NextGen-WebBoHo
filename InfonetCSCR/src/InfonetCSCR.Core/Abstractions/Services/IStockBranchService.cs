namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockBranchService
{
  Task<StockBranch> GetStockBranch(string stockCode);
  Task<bool> UpdateStockItemActiveStatus(string stockCode, bool flag);
  Task<bool> UpdateStockBranchReorderAndMaximumQuantity(StockBranch stockBranch);
}
