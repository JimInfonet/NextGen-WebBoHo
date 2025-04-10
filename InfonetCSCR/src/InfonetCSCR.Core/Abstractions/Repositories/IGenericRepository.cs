namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IGenericRepository
{
  Task<IList<Generic>> GetAllGenericsAsync();
  Task<bool> AddGenericAsync(Generic generic);
  Task<bool> UpdateGenericAsync(Generic generic);
  Task<bool> DeleteGenericAsync(string genericId);
}
