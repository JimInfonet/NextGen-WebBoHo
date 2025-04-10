namespace InfonetCSCR.Core.Abstractions.Services;

public interface IGroupService
{
  Task<IList<FamilyGroupJoinFamily>> GetAllGroups();
  Task<bool> AddGroup(FamilyGroupRecord group);
  Task<bool> DeleteGroup(FamilyGroupJoinFamily group);
  Task<IList<FamilyGroup>> GetAllFamilyGroups();
}
