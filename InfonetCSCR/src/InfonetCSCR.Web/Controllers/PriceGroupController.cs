namespace InfonetCSCR.Web.Controllers;

[Authorize]

public class PriceGroupController : Controller
{
  private readonly IPriceGroupService _priceGroupService;

  public PriceGroupController(IPriceGroupService priceGroupService)
  {
    _priceGroupService = priceGroupService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> GetAllPriceGroups()
  {
    IList<PriceGroupViewModel>? priceGroups = await _priceGroupService.GetAllPriceGroupsAsync();
    return Ok(priceGroups);
  }

  public async Task<IActionResult> SavePriceGroup([FromBody] PriceGroupViewModel priceGroup)
  {
    bool isAdded = await _priceGroupService.AddPriceGroupAsync(priceGroup);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeletePriceGroup(string priceGroupId)
  {
    var isDeleted = await _priceGroupService.DeletePriceGroupAsync(priceGroupId);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetStoreBySelectedPriceGroup(string priceGroupId)
  {
    var stores = await _priceGroupService.GetStoreBySelectedPriceGroupAsync(priceGroupId);
    return Ok(stores);
  }

  public async Task<IActionResult> GetStoreToLinkBySelectedPriceGroup(string priceGroupId)
  {
    var stores = await _priceGroupService.GetStoreToLinkBySelectedPriceGroupAsync(priceGroupId);
    return Ok(stores);
  }

  public async Task<IActionResult> AddStoreToThePriceGroup([FromBody] StoreLinkViewModel store)
  {
    bool isAdded = await _priceGroupService.AddStoreToThePriceGroupAsync(store);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeleteStoreForSelectedPriceGroup([FromBody] StoreLinkViewModel store)
  {
    bool isDeleted = await _priceGroupService.DeleteStoreForSelectedPriceGroupAsync(store);
    return Ok(isDeleted);
  }
}
