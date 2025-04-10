namespace InfonetCSCR.Web.Abstractions.Services;

public interface IFamilyService
{
  Task<IList<FamilyViewModel>?> GetAllFamily();
  Task<bool> AddFamily(FamilyViewModel family);
  Task<bool> UpdateFamily(FamilyViewModel family);
  Task<bool> DeleteFamily(string familyId);
}
