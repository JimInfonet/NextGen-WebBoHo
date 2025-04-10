namespace InfonetCSCR.Services;

public class ReasonsService : IReasonsService
{
  private readonly IUnitOfWork _unitOfWork;

  public ReasonsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }
  public async Task<IList<Reason>> GetReasons(string reasonType)
  {
    IList<Reason> reasons = await _unitOfWork.Reasons.GetReasonsAsync(reasonType);
    _unitOfWork.Commit();
    return reasons;
  }

  public async Task<bool> ReasonAlreadyExists(Reason reason)
  {
    var alreadyExists = await _unitOfWork.Reasons.ReasonAlreadyExistsAsync(reason);
    _unitOfWork.Commit();
    return alreadyExists;
  }

  public async Task<bool> AddReason(Reason reason)
  {
    bool isAdded = await _unitOfWork.Reasons.AddReasonAsync(reason);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateReason(Reason reason)
  {
    var isUpdated = await _unitOfWork.Reasons.UpdateReasonAsync(reason);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteReason(Reason reason)
  {
    var isDeleted = await _unitOfWork.Reasons.DeleteReasonAsync(reason);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
