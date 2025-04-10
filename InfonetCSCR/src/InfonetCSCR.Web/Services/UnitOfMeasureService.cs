namespace InfonetCSCR.Web.Services;

public class UnitOfMeasureService : IUnitOfMeasureService
{
  private readonly IHttpService _http;

  public UnitOfMeasureService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<UnitOfMeasureViewModel>?> GetAllUnitOfMeasures()
  {
    string url = "UnitOfMeasure/GetAllUnitOfMeasures";
    IList<UnitOfMeasureViewModel>? unitOfMeasures = await _http.GetAndParseAsync<IList<UnitOfMeasureViewModel>>(url);
    return unitOfMeasures;
  }

  public async Task<bool> AddUnitOfMeasure(UnitOfMeasureViewModel unitOfMeasure)
  {
    string url = $"UnitOfMeasure/AddUnitOfMeasure";
    var isAdded = await _http.PostAndParseAsync<bool>(url, unitOfMeasure);
    return isAdded;
  }

  public async Task<bool> UpdateUnitOfMeasure(UnitOfMeasureViewModel unitOfMeasure)
  {
    string url = $"UnitOfMeasure/UpdateUnitOfMeasure";
    var isUpdated = await _http.PostAndParseAsync<bool>(url, unitOfMeasure);
    return isUpdated;
  }

  public async Task<bool> DeleteUnitOfMeasure(string unitOfMeasureId)
  {
    string url = $"UnitOfMeasure/DeleteUnitOfMeasure";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, unitOfMeasureId);
    return isDeleted;
  }
}
