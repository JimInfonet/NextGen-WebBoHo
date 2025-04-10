namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IFamilyRepository
{
  Task<IList<Family>> GetAllFamilyAsync();
  Task<bool> AddFamilyAsync(Family family);
  Task<bool> UpdateFamilyAsync(Family family);
  Task<bool> DeleteFamilyAsync(string familyId);
}
