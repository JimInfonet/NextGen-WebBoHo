namespace InfonetCSCR.Web.Services;

public class PricebookManagementService : IPricebookManagementService
{
  private readonly IHttpService _http;

  public PricebookManagementService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IEnumerable<SubDepartmentViewModel>?> GetSubDepartmentsByDepartments(List<string> departments)
  {
    string url = "SubDepartment/GetSubDepartmentsByDepartments";
    IEnumerable<SubDepartmentViewModel>? subDepartments = await _http.PostAndParseAsync<IEnumerable<SubDepartmentViewModel>>(url, departments);
    return subDepartments;
  }

  public async Task<IEnumerable<SubDetailViewModel>?> GetSubDetailsByDepartments(List<string> departments)
  {
    string url = "SubDetail/GetSubDetailsByDepartments";
    IEnumerable<SubDetailViewModel>? subDetails = await _http.PostAndParseAsync<IEnumerable<SubDetailViewModel>>(url, departments);
    return subDetails;
  }

  public async Task<IEnumerable<VendorViewModel>?> GetVendorsOrderByColumnNumber(int columnNumber)
  {
    string url = $"vendors/GetVendorsOrderByColumnNumber/{columnNumber}";
    var vendors = await _http.GetAndParseAsync<IEnumerable<VendorViewModel>?>(url);
    return vendors;
  }
}
