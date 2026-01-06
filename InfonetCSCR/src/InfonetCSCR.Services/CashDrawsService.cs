namespace InfonetCSCR.Services;

public class CashDrawsService : ICashDrawsService
{
  private readonly IUnitOfWork _unitOfWork;

  public CashDrawsService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<DrawTypes>> GetAllDrawTypesData()
  {
    IList<DrawTypes> drawTypes = await _unitOfWork.CashDraws.GetAllDrawTypesDataAsync();
    _unitOfWork.Commit();
    return drawTypes;
  }

  public async Task<bool> InsertDrawType(DrawTypes drawTypes)
  {
    bool isAdded = await _unitOfWork.CashDraws.InsertDrawTypeAsync(drawTypes);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateDrawType(DrawTypes drawTypes)
  {
    bool isUpdated = await _unitOfWork.CashDraws.UpdateDrawTypeAsync(drawTypes);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteDrawType(DrawTypes drawTypes)
  {
    bool isDeleted = await _unitOfWork.CashDraws.DeleteDrawTypeAsync(drawTypes);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
