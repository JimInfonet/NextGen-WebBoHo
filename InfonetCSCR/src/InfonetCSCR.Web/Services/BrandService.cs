namespace InfonetCSCR.Web.Services;

public class BrandService : IBrandService
{
  private readonly IHttpService _http;

  public BrandService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<BrandViewModel>?> GetAllBrands()
  {
    string url = "Brand/GetAllBrands";
    IList<BrandViewModel>? brands = await _http.GetAndParseAsync<IList<BrandViewModel>>(url);
    return brands;
  }

  public async Task<bool> AddBrand(BrandViewModel brand)
  {
    string url = $"Brand/AddBrand";
    var isAdded = await _http.PostAndParseAsync<bool>(url, brand);
    return isAdded;
  }

  public async Task<bool> UpdateBrand(BrandViewModel brand)
  {
    string url = $"Brand/UpdateBrand";
    var isUpdated = await _http.PostAndParseAsync<bool>(url, brand);
    return isUpdated;
  }

  public async Task<bool> DeleteBrand(string brandId)
  {
    string url = $"Brand/DeleteBrand";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, brandId);
    return isDeleted;
  }
}
