namespace InfonetCSCR.Web.Services;

public class CashDrawsService : ICashDrawsService
{
  private readonly IHttpService _http;

  public CashDrawsService(IHttpService http)
  {
    _http = http;
  }

  public async Task<List<DrawTypesViewModel>?> GetAllDrawTypesData()
  {
    string url = "CashDraws/GetAllDrawTypesData";
    List<DrawTypesViewModel>? drawTypesViewModels = await _http.GetAndParseAsync<List<DrawTypesViewModel>>(url);
    return drawTypesViewModels;
  }

  public async Task<bool> InsertDrawType(DrawTypesViewModel drawTypes)
  {
    string url = "CashDraws/InsertDrawType";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, drawTypes);
    return isAdded;
  }

  public async Task<bool> UpdateDrawType(DrawTypesViewModel drawTypes)
  {
    string url = "CashDraws/UpdateDrawType";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, drawTypes);
    return isUpdated;
  }

  public async Task<bool> DeleteDrawType(DrawTypesViewModel drawTypes)
  {
    string url = "CashDraws/DeleteDrawType";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, drawTypes);
    return isDeleted;
  }
}
