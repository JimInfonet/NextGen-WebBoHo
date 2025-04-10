namespace InfonetCSCR.Services;

public class StockDescriptionElementsService : IStockDescriptionElementsService
{
  private readonly IUnitOfWork _unitOfWork;

  public StockDescriptionElementsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<Warehouse>> GetWarehouse()
  {
    IEnumerable<Warehouse> warehouses = await _unitOfWork.StockItemFormData.GetWarehouseAsync();
    _unitOfWork.Commit();
    return warehouses;
  }

  public async Task<IEnumerable<Grade>> GetGrade(string stockCode)
  {
    IEnumerable<Grade> grades = await _unitOfWork.StockItemFormData.GetGradeAsync(stockCode);
    _unitOfWork.Commit();
    return grades;
  }
}
