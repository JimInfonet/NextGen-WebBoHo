namespace InfonetCSCR.Services;

public class KitItemService : IKitItemService
{
  private readonly IUnitOfWork _unitOfWork;

  public KitItemService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<bool> CheckKitItemExistsByStockCode(string stockCode)
  {
    bool result = await _unitOfWork.KitItems.CheckKitItemExistsByStockCodeAsync(stockCode);
    _unitOfWork.Commit();
    return result;
  }
}
