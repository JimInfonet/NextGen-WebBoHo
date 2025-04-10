namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CardDefinitionController : Controller
{
  #region Constants
  private const string _CalcTdrFees = "calcTdrfees";
  private const string _AddMode = "addMode";
  private const string _FromGrid = "fromGrid";
  private const string _OptionalProfileIDs = "optionalProfileIDs";
  private const string _IsCardTypeNull = "isCardTypeNull";
  #endregion

  #region Private Properties
  private readonly IHttpService _http;
  private readonly IPolicyService _policyService;
  private readonly ICardDefinitionService _cardDefinitionService;
  private static bool FromGrid { get; set; } = false;
  #endregion

  public CardDefinitionController(
    IHttpService http,
    IPolicyService policyService,
    ICardDefinitionService cardDefinitionService)
  {
    _http = http;
    _policyService = policyService;
    _cardDefinitionService = cardDefinitionService;
  }

  public async Task<IActionResult> Index(string? cardCode, string? tendCodeData, bool? fromGrid)
  {
    if (tendCodeData != null)
    {
      string url = $"tenders/GetSingleTender?tendCode={tendCodeData}";
      var tender = await _http.GetAndParseAsync<TenderViewModel>(url);

      if (tender == null) { return View(); }

      tender.ExchangeRate = decimal.Parse(string.Format("{0:0.####}", tender.ExchangeRate));

      CardViewModel cardViewModel = new()
      {
        CardCode = tender.CardCode ?? 0,
        Description = tender.FullName
      };
      await _cardDefinitionService.AddCardForUpdateTender(cardViewModel);
    }

    if (fromGrid != null)
    {
      FromGrid = (bool)fromGrid;
    }

    ViewData[_CalcTdrFees] = _policyService.GetPolicy(PolicyName.CALC_TDRFEES);
    ViewData[_AddMode] = tendCodeData != null;
    ViewData[_FromGrid] = FromGrid;
    List<string> optionalProfileIDs = await _cardDefinitionService.GetOptionalProfileIDs();
    ViewData[_OptionalProfileIDs] = optionalProfileIDs;
    if (cardCode == null)
    {
      return View();
    }

    CardDefinitionViewModel? cardDefinitionViewModel = await _cardDefinitionService.GetCardDefinitionByCardCode(cardCode);
    ViewData[_IsCardTypeNull] = cardDefinitionViewModel?.CardType == null;
    return View(cardDefinitionViewModel);
  }

  public async Task<IActionResult> SaveCardDefinition(CardDefinitionViewModel cardDefinitionViewModel)
  {
    string url = @$"CardDefinition/UpdateCardDefinition";
    bool? updated = await _http.PostAndParseAsync<bool>(url, cardDefinitionViewModel);

    if (updated is null or false)
    {
      return View("Index", cardDefinitionViewModel);
    }

    return RedirectToAction("Index", new { cardCode = cardDefinitionViewModel.CardCode, actionMode = "update", ShowToaster = "Edit" });
  }

  public async Task<IActionResult> GetCardRangesByCardCode(string cardCode)
  {
    string url = @$"CardDefinition/GetCardRangesByCardCode/{cardCode}";
    var cardRanges = await _http.GetAndParseAsync<IEnumerable<CardRangeViewModel>>(url);
    return Ok(cardRanges);
  }

  public async Task<IActionResult> GetCards()
  {
    string url = @$"CardDefinition/GetCards";
    var cards = await _http.GetAndParseAsync<IEnumerable<CardViewModel>>(url);
    return Ok(cards);
  }

  public async Task<IActionResult> DeleteCardDefinitionByCardCode(string cardCode)
  {
    string url = @$"CardDefinition/DeleteCardDefinitionByCardCode";
    bool deleted = await _http.PostAndParseAsync<bool>(url, cardCode);
    return Ok(deleted);
  }

  public async Task<IActionResult> AddCardRange([FromBody] CardRangeViewModel cardRangeViewModel)
  {
    string url = @$"CardDefinition/AddCardRange";
    bool added = await _http.PostAndParseAsync<bool>(url, cardRangeViewModel);
    return Ok(added);
  }

  public async Task<IActionResult> DeleteCardRangesByCardCodeData([FromBody] CardRangeViewModel cardRangeViewModel)
  {
    string url = @$"CardDefinition/DeleteCardRangesByCardCodeData";
    bool deleted = await _http.PostAndParseAsync<bool>(url, cardRangeViewModel);
    return Ok(deleted);
  }
}
