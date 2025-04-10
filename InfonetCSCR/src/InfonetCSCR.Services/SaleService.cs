namespace InfonetCSCR.Services;

public class SaleService : ISaleService
{
  private readonly IUnitOfWork _unitOfWork;

  public SaleService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransConnectionString);
  }

  public async Task<bool> SaleExistsForStockCode(string stockCode)
  {
    bool result = await _unitOfWork.Sales.SaleExistsForStockCodeAsync(stockCode);
    _unitOfWork.Commit();
    return result;
  }
}
