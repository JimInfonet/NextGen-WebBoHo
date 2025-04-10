namespace InfonetCSCR.Services;

public class GroupService : IGroupService
{
  private readonly IUnitOfWork _unitOfWork;

  public GroupService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<FamilyGroupJoinFamily>> GetAllGroups()
  {
    IList<FamilyGroupJoinFamily> groups = await _unitOfWork.Group.GetAllGroupsAsync();
    _unitOfWork.Commit();
    return groups;
  }

  public async Task<bool> AddGroup(FamilyGroupRecord group)
  {
    bool isAdded = true;
    string[] checkedFamilies = group.Families;
    IList<FamilyGroup> familyGroups = new List<FamilyGroup>();

    foreach (string family in checkedFamilies)
    {
      familyGroups.Add(new FamilyGroup { GroupID = group.GroupID, GroupName = group.GroupName, FamilyID = family });
    }

    isAdded = await _unitOfWork.Group.AddGroupAsync(familyGroups);

    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteGroup(FamilyGroupJoinFamily group)
  {
    var isDeleted = await _unitOfWork.Group.DeleteGroupAsync(group);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<IList<FamilyGroup>> GetAllFamilyGroups()
  {
    IList<FamilyGroup> groups = await _unitOfWork.Group.GetAllFamilyGroupsAsync();
    _unitOfWork.Commit();
    return groups;
  }
}
