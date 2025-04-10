namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class KitsController : Controller
{
  private readonly IKitsService _kitsService;
  private readonly IPolicyService _policyService;

  public KitsController(IKitsService kitsService, IPolicyService policy)
  {
    _kitsService = kitsService;
    _policyService = policy;
  }

  public async Task<IActionResult> Index()
  {
    return View();
  }

  public async Task<IActionResult> GetKits()
  {
    var kits = await _kitsService.GetKits();
    return Ok(kits);
  }

  public async Task<IActionResult> GetKitItemsByKitCodeWithDesc(string stockCode)
  {
    var kits = await _kitsService.GetKitItemsByKitCodeWithDesc(stockCode);
    return Ok(kits);
  }

  public async Task<IActionResult> GetKitItems()
  {
    bool sellInactivePolicy = _policyService.GetPolicy(PolicyName.SELL_INACT);
    var kits = await _kitsService.GetKitItems(sellInactivePolicy);
    return Ok(kits);
  }

  public async Task<IActionResult> AddKitItem([FromBody] KitItemsViewModel kitItem)
  {
    bool isKitItemAdded = await _kitsService.AddKitItem(kitItem);
    return Ok(isKitItemAdded);
  }

  public async Task<IActionResult> IsKitItem(string stockCode)
  {
    var isKitItem = await _kitsService.IsKitItem(stockCode);
    return Ok(isKitItem);
  }

  public async Task<IActionResult> IsItemAlreadyInKit([FromBody] KitItemsViewModel kitItem)
  {
    var isItemAlreadyInKit = await _kitsService.IsItemAlreadyInKit(kitItem);
    return Ok(isItemAlreadyInKit);
  }

  public async Task<IActionResult> UpdateKitItem([FromBody] KitItemsViewModel kitItem)
  {
    var isKitItemUpdated = await _kitsService.UpdateKitItem(kitItem);
    return Ok(isKitItemUpdated);
  }

  public async Task<IActionResult> DeleteKitItem([FromBody] KitItemsViewModel kitItem)
  {
    var isKitItemAlreadyDeleted = await _kitsService.DeleteKitItem(kitItem);
    return Ok(isKitItemAlreadyDeleted);
  }
}
