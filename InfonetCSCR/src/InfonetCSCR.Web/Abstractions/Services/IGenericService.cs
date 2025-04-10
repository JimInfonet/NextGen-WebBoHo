namespace InfonetCSCR.Web.Abstractions.Services;

public interface IGenericService
{
  Task<IList<GenericViewModel>?> GetAllGenerics();
  Task<bool> AddGeneric(GenericViewModel generic);
  Task<bool> UpdateGeneric(GenericViewModel generic);
  Task<bool> DeleteGeneric(string genericId);
}
