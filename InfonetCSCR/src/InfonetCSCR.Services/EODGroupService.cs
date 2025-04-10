namespace InfonetCSCR.Services;

public class EODGroupService : IEODGroupService
{
  private readonly IUnitOfWork _unitOfWork;

  public EODGroupService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<bool> CheckGroupIdExists(string eodGroupId)
  {
    bool isGroupIdExists = await _unitOfWork.EODGroups.CheckGroupIdExistsAsync(eodGroupId);
    _unitOfWork.Commit();
    return isGroupIdExists;
  }

  public async Task<bool> AddEODGroup(EodGroup eodGroup)
  {
    bool isEodGroupAdded = await _unitOfWork.EODGroups.AddEODGroupAsync(eodGroup);
    _unitOfWork.Commit();
    return isEodGroupAdded;
  }

  public async Task<IEnumerable<EodGroup>> GetEODGroups()
  {
    IEnumerable<EodGroup> eodGroups = await _unitOfWork.EODGroups.GetGroupsAsync();
    _unitOfWork.Commit();
    return eodGroups;
  }

  public async Task<bool> DeleteEodGroupById(bool isDeptExistsForEodGroupId, string eodGroupId)
  {
    if (isDeptExistsForEodGroupId)
    {
      await _unitOfWork.Category.MakeEODGroupNullInDeptAsync(eodGroupId);
    }

    await _unitOfWork.EODGroups.DeleteEodGroupByEodGroupId(eodGroupId);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> UpdateEODGroupByDept(string eodGroupId, string dept)
  {
    bool result = await _unitOfWork.Category.UpdateEODGroupByDeptAsync(eodGroupId, dept);
    _unitOfWork.Commit();
    return result;
  }
}
