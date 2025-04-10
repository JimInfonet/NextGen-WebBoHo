namespace InfonetCSCR.Services;

public class UnitOfMeasureService : IUnitOfMeasureService
{
  private readonly IUnitOfWork _unitOfWork;

  public UnitOfMeasureService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<UnitOfMeasure>> GetAllUnitOfMeasures()
  {
    IList<UnitOfMeasure> unitOfMeasures = await _unitOfWork.UnitOfMeasure.GetAllUnitOfMeasuresAsync();
    _unitOfWork.Commit();
    return unitOfMeasures;
  }

  public async Task<bool> AddUnitOfMeasure(UnitOfMeasure unitOfMeasure)
  {
    bool isAdded = await _unitOfWork.UnitOfMeasure.AddUnitOfMeasureAsync(unitOfMeasure);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateUnitOfMeasure(UnitOfMeasure unitOfMeasure)
  {
    var isUpdated = await _unitOfWork.UnitOfMeasure.UpdateUnitOfMeasureAsync(unitOfMeasure);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteUnitOfMeasure(string unitOfMeasureId)
  {
    var isDeleted = await _unitOfWork.UnitOfMeasure.DeleteUnitOfMeasureAsync(unitOfMeasureId);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
