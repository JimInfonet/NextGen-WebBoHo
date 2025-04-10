namespace InfonetCSCR.Core.Abstractions.Services;

public interface IGenericService
{
  Task<IList<Generic>> GetAllGenerics();
  Task<bool> AddGeneric(Generic generic);
  Task<bool> UpdateGeneric(Generic generic);
  Task<bool> DeleteGeneric(string genericId);
}
