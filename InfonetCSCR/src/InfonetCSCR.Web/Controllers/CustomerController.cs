namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CustomerController : Controller
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policy;
  private readonly IApplicationConfig _config;
  private readonly ICustomerService _customersService;

  static bool _isUpdateCustomer = false;

  public CustomerController(
    IHttpService http,
    IPolicyService policy,
    IApplicationConfig config,
    ICustomerService customersService)
  {
    _http = http;
    _config = config;
    _policy = policy;
    _customersService = customersService;
  }

  public async Task<IActionResult> Index(string? code, string? actionMode, bool? showToaster)
  {
    bool securityAR = !_config.IsBackOffice || await _customersService.GetSecurityAR();
    var policies = _customersService.GetPolicies();

    ViewData["CUST_MEM"] = policies.CustMem;
    ViewData["securityAR"] = securityAR;
    ViewData["IsBackOffice"] = $"{_config.IsBackOffice}";
    ViewData["TE_Type"] = policies.TEType;
    ViewData["TAX_EXEMPT"] = policies.TaxExempt;
    ViewData["TE_PAYPUMP"] = policies.TEPayPump;
    ViewData["EnableBandAccT"] = policies.EnableBandAccT;

    _isUpdateCustomer = (bool)(code != null && code != "");

    if (!_isUpdateCustomer)
    {
      return View();
    }

    CustomerViewModel? customer = await _customersService.GetCustomer(code);

    if (customer == null)
    {
      return View();
    }

    return View(customer);
  }

  public async Task<IActionResult> GetCustomer(string code)
  {
    var customer = await _customersService.GetCustomer(code);
    return Ok(customer);
  }

  public async Task<IActionResult> GetCustomerByLoyaltyNum(string loyaltyNum)
  {
    var customer = await _customersService.GetCustomerByLoyaltyNum(loyaltyNum);
    return Ok(customer);
  }

  public async Task<IActionResult?> SaveCustomer(CustomerViewModel customer)
  {
    bool securityAR = !_config.IsBackOffice || await _customersService.GetSecurityAR();
    var policies = _customersService.GetPolicies();

    ViewData["CUST_MEM"] = policies.CustMem;
    ViewData["securityAR"] = securityAR;
    ViewData["IsBackOffice"] = $"{_config.IsBackOffice}";
    ViewData["TE_Type"] = policies.TEType;
    ViewData["TAX_EXEMPT"] = policies.TaxExempt;
    ViewData["TE_PAYPUMP"] = policies.TEPayPump;
    ViewData["EnableBandAccT"] = policies.EnableBandAccT;

    CustomerViewModel? customerViewModel = await _customersService.GetCustomer(customer.Code);
    bool isCustomerViewModelNull = (customerViewModel == null);
    if (!_isUpdateCustomer && customerViewModel != null)
    {
      ViewData["IsCustomerCodeAlreadyExists"] = true;
      return View("Index", customerViewModel);
    }

    customerViewModel = await _customersService.GetCustomerByLoyaltyNum(customer.LoyaltyNumber);
    if (customerViewModel != null && customerViewModel.Code != customer.Code)
    {
      ViewData["IsLoyaltyNumberAlreadyExists"] = true;
      if (isCustomerViewModelNull)
      {
        ViewData["IsLoyaltyNumberAlreadyExistsAdd"] = true;
      }
      else
      {
        ViewData["IsLoyaltyNumberAlreadyExistsUpdate"] = true;
      }

      return View("Index", customerViewModel);
    }

    customer.IsSyncCustInfo = _policy.GetPolicy(PolicyName.SYNC_CUSTINFO);

    await _customersService.SaveCustomer(customer, _isUpdateCustomer);

    return RedirectToAction("Index", new { Code = customer.Code, ActionMode = _isUpdateCustomer ? "update" : "create", ShowToaster = true });
  }

  public async Task<IActionResult> GetAllCustomerGroups()
  {
    string url = "Customers/GetAllCustomerGroups";
    var groups = await _http.GetAndParseAsync<IEnumerable<CustomerGroupViewModel>>(url);
    return Ok(groups);
  }

  public IActionResult GetPolicies()
  {
    var policies = _customersService.GetPolicies();
    return Ok(policies);
  }

  public async Task<IActionResult> GetAllRestrictionProfiles()
  {
    string url = "CardRestrictionProfile/GetAllRestrictionProfiles";
    var restrictionProfiles = await _http.GetAndParseAsync<IEnumerable<CardRestrictionProfileViewModel>>(url);
    return Ok(restrictionProfiles);
  }
}
