namespace InfonetCSCR.Services;

public class StockBranchService : IStockBranchService
{
  private readonly IUnitOfWork _unitOfWork;

  public StockBranchService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<StockBranch> GetStockBranch(string stockCode)
  {
    StockBranch stockBranch = await _unitOfWork.StockBranches.GetStockBranchAsync(stockCode);
    _unitOfWork.Commit();
    return stockBranch;
  }

  public async Task<bool> UpdateStockItemActiveStatus(string stockCode, bool flag)
  {
    await _unitOfWork.StockBranches.UpdateStockItemActiveStatusAsync(stockCode, flag);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> UpdateStockBranchReorderAndMaximumQuantity(StockBranch stockBranch)
  {
    await _unitOfWork.StockBranches.UpdateStockBranchAsync(stockBranch);
    bool result = _unitOfWork.Commit();
    return result;
  }
}
