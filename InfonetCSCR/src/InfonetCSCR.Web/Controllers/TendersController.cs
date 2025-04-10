namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class TendersController : Controller
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policyService;

  public TendersController(IHttpService http, IPolicyService policyService)
  {
    _http = http;
    _policyService = policyService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult TenderForm()
  {
    return View();
  }

  public async Task<IActionResult> TenderDetails(string tendCode)
  {
    string url = $"Tenders/GetSingleTender?tendCode={tendCode}";
    var tender = await _http.GetAndParseAsync<TenderViewModel>(url);
    tender.ExchangeRate = decimal.Parse(string.Format("{0:0.####}", tender?.ExchangeRate));
    ViewData["ShortName"] = tender.ShortName;
    return View(tender);
  }

  public async Task<IActionResult> GetTendersWithCardCode()
  {
    string url = "Tenders/GetTendersWithCardCode";
    var tenders = await _http.GetAndParseAsync<IEnumerable<TenderViewModel>>(url);
    return Ok(tenders);
  }

  public async Task<IActionResult> DeleteTender(string tendCode)
  {
    string url = "Tenders/DeleteTender";
    await _http.PostAsync(url, tendCode);
    return NoContent();
  }

  public async Task<IActionResult> ResetSequence()
  {
    string url = "Tenders/ResetSequence";
    await _http.GetAsync(url);
    return NoContent();
  }

  // TenderDetails section
  public async Task<IActionResult> CheckTranscationExistsForTendName(string tendName)
  {
    string url = $"Tenders/CheckTranscationExistsForTendName/{tendName}";
    var exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  [HttpPost]
  public async Task<IActionResult> UpdateTender(TenderViewModel tender)
  {
    // TODO: Need to delete previous cardCode from Cards and CardCodes
    // TODO: Need to update cardCode in TendCard

    TenderCardViewModel tenderCardViewModel = new()
    {
      TendCode = tender.Code,
      DiscountType = "P",
      CardCode = tender.CardCode,
    };

    string url = "";

    if (tender.CardCode != null)
    {
      url = "CardDefinition/AddOrUpdateTenderCardForUpdateTender";
      await _http.PostAsync(url, tenderCardViewModel);
    }

    string BASECURR = _policyService.GetPolicy(PolicyName.BASECURR);
    if (BASECURR == tender.Code && (bool)tender.Inactive)
    {
      ViewData["BaseCurrCantBeInactive"] = "True";
      return View("TenderDetails", tender);
    }

    url = "Tenders/UpdateTender";
    bool updated = await _http.PostAndParseAsync<bool>(url, tender);
    ViewData["IsTenderDetailsUpdated"] = updated.ToString();

    if (updated)
    {
      return View("Index");
    }
    else
    {
      return View("TenderDetails", tender);
    }
  }

  public async Task<IActionResult> GetPayout()
  {
    string url = $"Tenders/GetPayout";
    var payouts = await _http.GetAndParseAsync<IEnumerable<PayoutViewModel>>(url);
    return Ok(payouts);
  }

  [HttpPost]
  public async Task<IActionResult> AddTender(TenderViewModel tender)
  {
    string url = $"Tenders/IsTenderCodeExist?tendCode={tender.Code}";
    bool isAlreadyExist = await _http.GetAndParseAsync<bool>(url);

    if (isAlreadyExist)
    {
      ViewData["isTenderCodeExist"] = isAlreadyExist.ToString();
      return View("TenderForm");
    }

    url = $"tenders/IsFullNameExist?fullName={tender.FullName}";
    isAlreadyExist = await _http.GetAndParseAsync<bool>(url);

    if (isAlreadyExist)
    {
      ViewData["isFullNameExist"] = isAlreadyExist.ToString();
      return View("TenderForm");
    }

    url = "Tenders/AddTender";
    bool added = await _http.PostAndParseAsync<bool>(url, tender);
    ViewData["isTenderAdded"] = added.ToString();

    if (added)
    {
      return View("Index");
    }
    else
    {
      return View("TenderForm");
    }
  }

  public async Task<IActionResult> GetTenderClasses()
  {
    string url = $"Tenders/GetTenderClasses";
    var tenderClasses = await _http.GetAndParseAsync<IEnumerable<TenderClassViewModel>>(url);
    return Ok(tenderClasses);
  }

  public async Task<IActionResult> GetTendersList()
  {
    const string url = "Tenders/GetTendersList";
    var tendersList = await _http.GetAndParseAsync<IEnumerable<TenderViewModel>>(url);
    return Ok(tendersList);
  }

  public async Task<IActionResult> GetTendersListByClassName(string tendClass)
  {
    string url = $"Tenders/GetTendersListByClassName?tendClass={tendClass}";
    var tendersList = await _http.GetAndParseAsync<IEnumerable<TenderViewModel>>(url);
    return Ok(tendersList);
  }
}
