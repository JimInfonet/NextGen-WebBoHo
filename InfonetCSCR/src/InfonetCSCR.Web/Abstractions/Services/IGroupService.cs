namespace InfonetCSCR.Web.Abstractions.Services;

public interface IGroupService
{
  Task<IList<FamilyGroupJoinFamilyViewModel>?> GetAllGroups();
  Task<bool> AddGroup(FamilyGroupRecordViewModel group);
  Task<bool> DeleteGroup(FamilyGroupJoinFamilyViewModel group);
  Task<IList<FamilyGroupViewModel>?> GetAllFamilyGroups();
}
