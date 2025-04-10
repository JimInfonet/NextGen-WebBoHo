namespace InfonetCSCR.Web.Services;

public class FamilyService : IFamilyService
{
  private readonly IHttpService _http;

  public FamilyService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<FamilyViewModel>?> GetAllFamily()
  {
    string url = "Family/GetAllFamily";
    IList<FamilyViewModel>? families = await _http.GetAndParseAsync<IList<FamilyViewModel>>(url);
    return families;
  }

  public async Task<bool> AddFamily(FamilyViewModel family)
  {
    string url = $"Family/AddFamily";
    var isAdded = await _http.PostAndParseAsync<bool>(url, family);
    return isAdded;
  }

  public async Task<bool> UpdateFamily(FamilyViewModel family)
  {
    string url = $"Family/UpdateFamily";
    var isUpdated = await _http.PostAndParseAsync<bool>(url, family);
    return isUpdated;
  }

  public async Task<bool> DeleteFamily(string familyId)
  {
    string url = $"Family/DeleteFamily";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, familyId);
    return isDeleted;
  }
}
