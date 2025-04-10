namespace InfonetCSCR.Web.Services;

public class GenericService : IGenericService
{
  private readonly IHttpService _http;

  public GenericService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<GenericViewModel>?> GetAllGenerics()
  {
    string url = "Generic/GetAllGenerics";
    IList<GenericViewModel>? generics = await _http.GetAndParseAsync<IList<GenericViewModel>>(url);
    return generics;
  }

  public async Task<bool> AddGeneric(GenericViewModel generic)
  {
    string url = $"Generic/AddGeneric";
    var isAdded = await _http.PostAndParseAsync<bool>(url, generic);
    return isAdded;
  }

  public async Task<bool> UpdateGeneric(GenericViewModel generic)
  {
    string url = $"Generic/UpdateGeneric";
    var isUpdated = await _http.PostAndParseAsync<bool>(url, generic);
    return isUpdated;
  }

  public async Task<bool> DeleteGeneric(string genericId)
  {
    string url = $"Generic/DeleteGeneric";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, genericId);
    return isDeleted;
  }
}
