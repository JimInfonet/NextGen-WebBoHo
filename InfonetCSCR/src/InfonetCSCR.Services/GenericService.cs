namespace InfonetCSCR.Services;

public class GenericService : IGenericService
{
  private readonly IUnitOfWork _unitOfWork;

  public GenericService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<Generic>> GetAllGenerics()
  {
    IList<Generic> generics = await _unitOfWork.Generic.GetAllGenericsAsync();
    _unitOfWork.Commit();
    return generics;
  }

  public async Task<bool> AddGeneric(Generic generic)
  {
    bool isAdded = await _unitOfWork.Generic.AddGenericAsync(generic);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateGeneric(Generic generic)
  {
    var isUpdated = await _unitOfWork.Generic.UpdateGenericAsync(generic);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteGeneric(string genericId)
  {
    var isDeleted = await _unitOfWork.Generic.DeleteGenericAsync(genericId);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
