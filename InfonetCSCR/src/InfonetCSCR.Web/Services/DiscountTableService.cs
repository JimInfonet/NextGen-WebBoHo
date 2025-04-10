namespace InfonetCSCR.Web.Services;

public class DiscountTableService : IDiscountTableService
{
  private readonly IHttpService _http;

  public DiscountTableService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<DiscountTableViewModel>?> GetAllDiscountTableData(DiscountTablePolicyRecordViewModel discountTablePolicyRecordViewModel)
  {
    string url = "DiscountTable/GetAllDiscountTableData";
    IList<DiscountTableViewModel>? discountTableData = await _http.PostAndParseAsync<IList<DiscountTableViewModel>>(url, discountTablePolicyRecordViewModel);
    return discountTableData;
  }

  public async Task<bool> InsertDataOnDiscountTable(DiscountTableViewModel discountTableViewModel)
  {
    string url = "DiscountTable/InsertDataOnDiscountTable";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, discountTableViewModel);
    return isAdded;
  }

  public async Task<bool> UpdateDiscountPercentage(DiscountTableViewModel discountTableViewModel)
  {
    string url = "DiscountTable/UpdateDiscountPercentage";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, discountTableViewModel);
    return isUpdated;
  }

  public async Task<bool> DeleteDiscount(DiscountTableViewModel discountTableViewModel)
  {
    string url = "DiscountTable/DeleteDiscount";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, discountTableViewModel);
    return isDeleted;
  }
}
