namespace InfonetCSCR.Services;

public class FamilyService : IFamilyService
{
  private readonly IUnitOfWork _unitOfWork;

  public FamilyService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<Family>> GetAllFamily()
  {
    IList<Family> families = await _unitOfWork.Family.GetAllFamilyAsync();
    _unitOfWork.Commit();
    return families;
  }

  public async Task<bool> AddFamily(Family family)
  {
    bool isAdded = await _unitOfWork.Family.AddFamilyAsync(family);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateFamily(Family family)
  {
    var isUpdated = await _unitOfWork.Family.UpdateFamilyAsync(family);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteFamily(string familyId)
  {
    var isDeleted = await _unitOfWork.Family.DeleteFamilyAsync(familyId);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
