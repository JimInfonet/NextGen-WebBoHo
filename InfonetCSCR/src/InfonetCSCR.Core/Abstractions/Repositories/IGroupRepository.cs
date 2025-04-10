namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IGroupRepository
{
  Task<IList<FamilyGroupJoinFamily>> GetAllGroupsAsync();
  Task<bool> AddGroupAsync(IList<FamilyGroup> groups);
  Task<bool> DeleteGroupAsync(FamilyGroupJoinFamily group);
  Task<IList<FamilyGroup>> GetAllFamilyGroupsAsync();
}
