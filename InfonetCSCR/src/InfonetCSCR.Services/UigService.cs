namespace InfonetCSCR.Services;

public class UigService : IUigService
{
  private readonly IUnitOfWork _unitOfWork;

  public UigService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCAdminConnectionString);
  }

  public async Task<IEnumerable<GroupInfoDto>?> GetGroupInfo(string userCode)
  {
    IEnumerable<GroupInfoDto>? groupInfo = await _unitOfWork.Uig.GetGroupInfoAsync(userCode);
    _unitOfWork.Commit();

    return groupInfo;
  }

  public async Task<bool> AddUigRecord(Uig uig)
  {
    bool isAdded = await _unitOfWork.Uig.AddUigRecordAsync(uig);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> DeleteUigRecord(Uig uig, bool isGroupIncluded = true)
  {
    bool isDeleted = await _unitOfWork.Uig.DeleteUigRecordAsync(uig, isGroupIncluded);
    _unitOfWork.Commit();

    return isDeleted;
  }
}
