namespace InfonetCSCR.Web.Services;

public class CustomerService : ICustomerService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policy;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CustomerService(IHttpService http, IPolicyService policy, IHttpContextAccessor httpContextAccessor)
  {
    _http = http;
    _policy = policy;
    _httpContextAccessor = httpContextAccessor;
  }

  public CustomerPoliciesViewModel GetPolicies()
  {
    string userJson = _httpContextAccessor.HttpContext?.Session.GetString("UserDetails")!;
    UserDetailsViewModel user = JsonSerializer.Deserialize<UserDetailsViewModel>(userJson)!;

    var policies = new CustomerPoliciesViewModel
    {
      UseDataCard = _policy.GetPolicy(PolicyName.USE_DATACARD),
      RSTRProfile = _policy.GetPolicy(PolicyName.RSTR_PROFILE),
      CredTerm = _policy.GetPolicy(PolicyName.CREDTERM),
      FuelRebate = _policy.GetPolicy(PolicyName.FuelRebate),
      UCLStatus = _policy.GetPolicy(PolicyName.U_CLSTATUS, user),
      CustMem = _policy.GetPolicy(PolicyName.CUST_MEM),
      SyncCustInfo = _policy.GetPolicy(PolicyName.SYNC_CUSTINFO),
      TaxExempt = _policy.GetPolicy(PolicyName.TAX_EXEMPT),
      TEType = _policy.GetPolicy(PolicyName.TE_Type),
      EnableBandAccT = _policy.GetPolicy(PolicyName.ENABLE_BANDACCT),
      TEPayPump = _policy.GetPolicy(PolicyName.TE_PAYPUMP),
      UseKickBack = _policy.GetPolicy(PolicyName.USE_KICKBACK),
      CashBonus = _policy.GetPolicy(PolicyName.CASHBONUS)
    };

    return policies;
  }

  public async Task<bool> DeleteClient(string code)
  {
    CustomerViewModel? customer = await GetCustomer(code);
    if (customer == null) return false;

    customer.IsSyncCustInfo = _policy.GetPolicy(PolicyName.SYNC_CUSTINFO);

    string url = "Customers/DeleteClient";
    bool deleted = await _http.PostAndParseAsync<bool>(url, customer);
    return deleted;
  }

  public async Task<CustomerViewModel?> GetCustomer(string code)
  {
    if (code == null) return null;

    string url = $@"Customers/GetCustomer?code={code}";
    var customer = await _http.GetAndParseAsync<CustomerViewModel?>(url);
    return customer;
  }

  public async Task<CustomerViewModel?> GetCustomerByLoyaltyNum(string loyaltyNum)
  {
    string url = $@"Customers/GetCustomer?loyaltyNum={loyaltyNum}";
    var customer = await _http.GetAndParseAsync<CustomerViewModel?>(url);
    return customer;
  }

  public async Task SaveCustomer(CustomerViewModel customer, bool isUpdate)
  {
    string url = (isUpdate) ? "Customers/UpdateCustomer" : "Customers/AddCustomer";
    await _http.PostAsync(url, customer);
  }

  public async Task<bool> GetSecurityAR()
  {
    string url = "Customers/GetSecurityAR";
    bool securityAR = await _http.GetAndParseAsync<bool>(url);
    return securityAR;
  }
}
