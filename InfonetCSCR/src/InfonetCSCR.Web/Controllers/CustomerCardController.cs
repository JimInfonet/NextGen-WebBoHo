namespace InfonetCSCR.Web.Controllers;

public class CustomerCardController : Controller
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;
  private readonly ICustomerService _customersService;

  static string? _code;
  private static bool _isArCustomer;

  public CustomerCardController(
    IHttpService http,
    IApplicationConfig config,
    IPolicyService policyService,
    ICustomerService customersService)
  {
    _http = http;
    _config = config;
    _policyService = policyService;
    _customersService = customersService;
  }

  public async Task<ActionResult> Index(string code)
  {
    _code = code;
    var policies = _customersService.GetPolicies();

    string url = $"CustomerCard/IsArCustomer?code={code}";
    _isArCustomer = await _http.GetAndParseAsync<bool>(url);

    ViewData["CUST_MEM"] = policies.CustMem;
    ViewData["USE_DATACARD"] = policies.UseDataCard;
    ViewData["CL_ARCUST"] = _isArCustomer;
    ViewData["TE_Type"] = policies.TEType;
    ViewData["TAX_EXEMPT"] = policies.TaxExempt;
    ViewData["TE_PAYPUMP"] = policies.TEPayPump;
    ViewData["USE_KICKBACK"] = policies.UseKickBack;
    ViewData["RSTR_PROFILE"] = policies.RSTRProfile;
    ViewData["IsBackOffice"] = _config.IsBackOffice;

    return View();
  }

  public async Task<IActionResult> GetCustomerCards()
  {
    string url = $"CustomerCard/GetCustomerCardsByCode?code={_code}";
    var customerCards = await _http.GetAndParseAsync<IEnumerable<CustomerCardViewModel>?>(url);
    return Ok(customerCards);
  }

  public async Task<IActionResult> SaveCustomerCard([FromBody] CustomerCardViewModel customerCard)
  {
    bool update = customerCard.IsEditMode == true;
    customerCard.IsSyncCustInfo = _policyService.GetPolicy(PolicyName.SYNC_CUSTINFO);
    customerCard.IsArCustomer = _isArCustomer;

    string url = update ? "CustomerCard/UpdateCustomerCard" : "CustomerCard/AddCustomerCard";
    await _http.PostAsync(url, customerCard);
    return NoContent();
  }

  public async Task<IActionResult> DeleteCustomerCard(string customerCardNumber)
  {
    bool policySyncCustInfo = _policyService.GetPolicy(PolicyName.SYNC_CUSTINFO);

    string url = $"CustomerCard/DeleteCustomerCardByNumber";
    await _http.PostAsync(url, new
    {
      CustomerCardNumber = customerCardNumber,
      IsSyncCustInfo = policySyncCustInfo,
      IsArCustomer = _isArCustomer
    });

    return NoContent();
  }

  public async Task<IActionResult> DeleteRewardCard(string customerCardNumber, string rewardCardNumber)
  {
    string url = $"CustomerCard/DeleteRewardCard?customerCardNumber={customerCardNumber}&rewardCardNumber={rewardCardNumber}";
    await _http.PostAsync(url, string.Empty);
    return NoContent();
  }

  public async Task<IActionResult> GetRewardCardsByCardNumber(string cardNumber)
  {
    string url = $"CustomerCard/GetRewardCardsByCardNumber?cardNumber={cardNumber}";
    var customerCards = await _http.GetAndParseAsync<IEnumerable<RewardCardViewModel>>(url);
    return Ok(customerCards);
  }
  public async Task<IActionResult> GetCustomerCardByNumber(string cardNumber)
  {
    string url = $"CustomerCard/GetCustomerCardByNumber?cardNumber={cardNumber}";
    var customerCard = await _http.GetAndParseAsync<CustomerCardViewModel?>(url);
    return Ok(customerCard);
  }

  public async Task<IActionResult> IsRewardCardExistByCustomerCardAndRewardCardNumber(string custCardNumber, string rewardCardNumber)
  {
    string url = $"CustomerCard/IsRewardCardExistByCustomerCardAndRewardCardNumber?custCardNumber={custCardNumber}&rewardCardNumber={rewardCardNumber}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> IsRewardCardExistByCustomerCardAndPhone(string custCardNumber, string phoneNumber)
  {
    string url = $"CustomerCard/IsRewardCardExistByCustomerCardAndPhone?custCardNumber={custCardNumber}&phoneNumber={phoneNumber}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> IsCustomerCardExist(string cardNumber)
  {
    string url = $"CustomerCard/IsCustomerCardExist?custCardNumber={cardNumber}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> SaveRewardCard([FromBody] RewardCardViewModel rewardCard)
  {
    string url = rewardCard.IsEditMode == true ? "CustomerCard/UpdateRewardCard" : "CustomerCard/AddRewardCard";
    await _http.PostAsync(url, rewardCard);
    return NoContent();
  }

  public async Task<IActionResult> GetNextSequenceNumber(string customerCode)
  {
    string url = $"CustomerCard/GetNextSequenceNumber?customerCode={customerCode}";
    int nextSeqNumber = await _http.GetAndParseAsync<int>(url);
    return Ok(nextSeqNumber);
  }
}
