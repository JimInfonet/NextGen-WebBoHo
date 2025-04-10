namespace InfonetCSCR.Services;

public class RestrictionsService : IRestrictionsService
{
  private readonly IUnitOfWork _unitOfWork;

  public RestrictionsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<Restriction>> GetRestrictions()
  {
    IList<Restriction> restrictions = await _unitOfWork.Restrictions.GetRestrictionsAsync();
    _unitOfWork.Commit();
    return restrictions;
  }

  public async Task<bool> RestrictionAlreadyExists(int code)
  {
    var alreadyExists = await _unitOfWork.Restrictions.RestrictionAlreadyExistsAsync(code);
    _unitOfWork.Commit();
    return alreadyExists;
  }

  public async Task<bool> AddRestriction(Restriction restriction)
  {
    bool isAdded = await _unitOfWork.Restrictions.AddRestrictionAsync(restriction);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateRestriction(Restriction restriction)
  {
    var isUpdated = await _unitOfWork.Restrictions.UpdateRestrictionAsync(restriction);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteRestriction(int code)
  {
    var isDeleted = await _unitOfWork.Restrictions.DeleteRestrictionAsync(code);
    _unitOfWork.Commit();
    return isDeleted;
  }
}