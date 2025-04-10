namespace InfonetCSCR.Web.Services;

public class RestrictionsService : IRestrictionsService
{
  private readonly IHttpService _http;

  public RestrictionsService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<RestrictionsViewModel>?> GetRestrictions()
  {
    string url = "Restrictions/GetRestrictions";
    IList<RestrictionsViewModel>? restrictions = await _http.GetAndParseAsync<IList<RestrictionsViewModel>>(url);
    return restrictions;
  }

  public async Task<bool> RestrictionAlreadyExists(int code)
  {
    string url = $"Restrictions/RestrictionAlreadyExists";
    var isExist = await _http.PostAndParseAsync<bool>(url, code);
    return isExist;
  }

  public async Task<bool> AddRestriction(RestrictionsViewModel restriction)
  {
    string url = $"Restrictions/AddRestriction";
    var isAdded = await _http.PostAndParseAsync<bool>(url, restriction);
    return isAdded;
  }

  public async Task<bool> UpdateRestriction(RestrictionsViewModel restriction)
  {
    string url = $"Restrictions/UpdateRestriction";
    var isUpdated = await _http.PostAndParseAsync<bool>(url, restriction);
    return isUpdated;
  }

  public async Task<bool> DeleteRestriction(int code)
  {
    string url = $"Restrictions/DeleteRestriction";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, code);
    return isDeleted;
  }
}