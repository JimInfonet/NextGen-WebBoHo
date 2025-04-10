namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class HotButtonsAndBottleReturnController : Controller
{
  private readonly IHotButtonsService _hotButtonsService;
  private readonly IHotButtonPagesService _hotButtonPagesService;
  private readonly ICardProductRestrictionService _cardProductRestrictionService;
  private readonly IPolicyService _policyService;

  public HotButtonsAndBottleReturnController(
    IHotButtonsService hotButtonsService, 
    IHotButtonPagesService hotButtonPagesService,
    ICardProductRestrictionService cardProductRestrictionService,
    IPolicyService policyService)
  {
    _hotButtonsService = hotButtonsService;
    _hotButtonPagesService = hotButtonPagesService;
    _cardProductRestrictionService = cardProductRestrictionService;
    _policyService = policyService;
  }

  public IActionResult Index(string pageType)
  {
    ViewData["pageType"] = pageType;
    return View();
  }

  public async Task<IActionResult> GetHotButtonsByButtonIdRange(HotButtonsIdRangeRecordViewModel hotButtonsIdRangeRecordViewModel)
  {
    List<HotButtonsViewModel>? hotButtons = await _hotButtonsService.GetHotButtonsByButtonIdRange(hotButtonsIdRangeRecordViewModel);
    return Ok(hotButtons);
  }

  public async Task<IActionResult> GetAllHotButtonPages()
  {
    IList<HotButtonPagesViewModel>? hotButtonPagesViewModels = await _hotButtonPagesService.GetAllHotButtonPages();
    return Ok(hotButtonPagesViewModels);
  }

  public async Task<IActionResult> UpdateHotButtonPage([FromBody] HotButtonPagesViewModel? hotButtonPagesViewModel)
  {
    bool? isUpdated = await _hotButtonPagesService.UpdateHotButtonPage(hotButtonPagesViewModel);
    return Ok(isUpdated);
  }

  public async Task<IActionResult> InsertHotButtons([FromBody] HotButtonsViewModel hotButtonsViewModel)
  {
    bool? isAdded = await _hotButtonsService.InsertHotButtons(hotButtonsViewModel);
    return Ok(isAdded);
  }

  public async Task<IActionResult> UpdateHotButtonsColor([FromBody] HotButtonsViewModel hotButtonsViewModel)
  {
    bool? isColorUpdated = await _hotButtonsService.UpdateHotButtonsColor(hotButtonsViewModel);
    return Ok(isColorUpdated);
  }

  public async Task<IActionResult> DeleteHotButton([FromBody] HotButtonsViewModel hotButtonsViewModel)
  {
    bool? isDeleted = await _hotButtonsService.DeleteHotButton(hotButtonsViewModel);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetPLUMastByPLUCode(string pluCode)
  {
    PLUMastViewModel? pluMastViewModel = await _hotButtonsService.GetPLUMastByPLUCode(pluCode);
    return Ok(pluMastViewModel);
  }

  public async Task<IActionResult> GetBottleReturnDataByProductCode(string productCode)
  {
    HotButtonsViewModel? hotButtonsViewModel = await _hotButtonsService.GetBottleReturnDataByProductCode(productCode);
    return Ok(hotButtonsViewModel);
  }

  public async Task<IActionResult> GetStockCodesForHotButtons()
  {
    bool sellInactivePolicy = _policyService.GetPolicy(PolicyName.SELL_INACT);
    IEnumerable<StockCodesForModalViewModel>? stockCodes = await _cardProductRestrictionService.GetStockCodesForProductRestriction(sellInactivePolicy, true);
    return Ok(stockCodes);
  }
}
