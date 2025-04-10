namespace InfonetCSCR.Core.Abstractions.Services;

public interface IFamilyService
{
  Task<IList<Family>> GetAllFamily();
  Task<bool> AddFamily(Family family);
  Task<bool> UpdateFamily(Family family);
  Task<bool> DeleteFamily(string familyId);
}
