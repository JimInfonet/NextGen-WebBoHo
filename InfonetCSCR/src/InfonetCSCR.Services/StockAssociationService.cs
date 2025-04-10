namespace InfonetCSCR.Services;

public class StockAssociationService : IStockAssociationService
{
  private readonly IUnitOfWork _unitOfWork;

  public StockAssociationService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<bool> InsertStockChargesByStockChargeData(IEnumerable<StockAc> stockCharges)
  {
    await _unitOfWork.StockAcs.DeleteStockChargesByStockChargeDataAsync(stockCharges);
    await _unitOfWork.StockAcs.InsertStockChargesByStockChargeDataAsync(stockCharges);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeleteStockChargesByStockChargeData(IEnumerable<StockAc> stockCharges)
  {
    bool isDeleted = await _unitOfWork.StockAcs.DeleteStockChargesByStockChargeDataAsync(stockCharges);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
