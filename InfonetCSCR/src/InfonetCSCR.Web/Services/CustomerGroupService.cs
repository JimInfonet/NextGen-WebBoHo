namespace InfonetCSCR.Web.Services;

public class CustomerGroupService : ICustomerGroupService
{
  private readonly IPolicyService _policy;
  private readonly IHttpService _http;

  public CustomerGroupService(IPolicyService policy, IHttpService http)
  {
    _policy = policy;
    _http = http;
  }

  public CustomerPoliciesViewModel GetPolicies()
  {
    var policies = new CustomerPoliciesViewModel
    {
      CustMem = _policy.GetPolicy(PolicyName.CUST_MEM),
      CashBonus = _policy.GetPolicy(PolicyName.CASHBONUS),
      UseARInvoices = _policy.GetPolicy(PolicyName.USE_ARINVOICES)
    };

    return policies;
  }

  public async Task<bool> SaveCustomerGroup(CustomerGroupViewModel customerGroup, bool isUpdate)
  {
    string url = (isUpdate) ? "CustomerGroup/UpdateCustomerGroup" : "CustomerGroup/AddCustomerGroup";
    bool isCompleted = await _http.PostAndParseAsync<bool>(url, customerGroup);
    return isCompleted;
  }

  public async Task<bool> DeleteCustomerGroup(string groupID)
  {
    var customerGroup = await GetCustomerGroup(groupID);
    if (customerGroup == null) return false;

    string url = "CustomerGroup/DeleteCustomerGroup";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, groupID);
    return isDeleted;
  }

  public async Task<CustomerGroupViewModel?> GetCustomerGroup(string groupID)
  {
    if (string.IsNullOrEmpty(groupID)) return null;

    string url = $"CustomerGroup/GetCustomerGroup?groupID={groupID}";
    var customer = await _http.GetAndParseAsync<CustomerGroupViewModel?>(url);
    return customer;
  }

  public async Task<bool> UpdateCashBonusData(IEnumerable<CashBonusChartViewModel> cashBonusChartViewModels)
  {
    string url = "CustomerGroup/UpdateCashBonusData";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, cashBonusChartViewModels);

    return isUpdated;
  }

  public async Task DeleteCashBonusData(string groupID)
  {
    string url = $"CustomerGroup/DeleteCashBonusData?groupID={groupID}";
    await _http.PostAndParseAsync<bool>(url);
  }

  public async Task<IEnumerable<DiscountChartViewModel>> GetDiscountChartData(string? groupID, bool isUseARInvoice)
  {
    string url = $"CustomerGroup/GetDiscountChartData?groupID={groupID}&isUseARInvoice={isUseARInvoice}";
    var discountChartData = await _http.GetAndParseAsync<IEnumerable<DiscountChartViewModel>>(url);
    return discountChartData;
  }

  public async Task DeleteDiscountChartData(string groupID)
  {
    string url = $"CustomerGroup/DeleteDiscountChartData?groupID={groupID}";
    await _http.PostAndParseAsync<bool>(url);
  }

  public async Task SaveDiscountChartData(IEnumerable<DiscountChartViewModel> discountChartViewModels)
  {
    string url = $"CustomerGroup/SaveDiscountChartData";
    await _http.PostAsync(url, discountChartViewModels);
  }
}
