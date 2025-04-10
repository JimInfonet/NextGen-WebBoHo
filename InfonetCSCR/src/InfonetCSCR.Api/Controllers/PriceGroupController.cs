namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class PriceGroupController : ControllerBase
{
  private readonly IPriceGroupService _priceGroupService;

  public PriceGroupController(IPriceGroupService priceGroupService)
  {
    _priceGroupService = priceGroupService;
  }

  // GET api/PriceGroup/GetAllPriceGroups
  [HttpGet]
  [Route("GetAllPriceGroups")]
  public async Task<IActionResult> GetAllPriceGroups()
  {
    IEnumerable<PriceGroup>? priceGroups = await _priceGroupService.GetPriceGroupsListAsync();
    return Ok(priceGroups.ToList());
  }

  // POST api/PriceGroup/AddPriceGroup
  [HttpPost]
  [Route("AddPriceGroup")]
  public async Task<IActionResult> AddPriceGroup([FromBody] PriceGroup priceGroup)
  {
    bool isAdded = await _priceGroupService.AddPriceGroupAsync(priceGroup);
    return Ok(isAdded);
  }

  // POST api/PriceGroup/DeletePriceGroup
  [HttpPost]
  [Route("DeletePriceGroup")]
  public async Task<IActionResult> DeletePriceGroup([FromBody] string priceGroupId)
  {
    bool isDeleted = await _priceGroupService.DeletePriceGroupAsync(priceGroupId);
    return Ok(isDeleted);
  }

  // GET api/PriceGroup/GetStoreBySelectedPriceGroup
  [HttpGet]
  [Route("GetStoreBySelectedPriceGroup")]
  public async Task<IActionResult> GetStoreBySelectedPriceGroup(string priceGroupId)
  {
    var stores = await _priceGroupService.GetStoreBySelectedPriceGroupAsync(priceGroupId);
    return Ok(stores);
  }

  // GET api/PriceGroup/GetStoreToLinkBySelectedPriceGroup
  [HttpGet]
  [Route("GetStoreToLinkBySelectedPriceGroup")]
  public async Task<IActionResult> GetStoreToLinkBySelectedPriceGroup(string priceGroupId)
  {
    var stores = await _priceGroupService.GetStoreToLinkBySelectedPriceGroupAsync(priceGroupId);
    return Ok(stores);
  }

  // POST api/PriceGroup/AddStoreToThePriceGroup
  [HttpPost]
  [Route("AddStoreToThePriceGroup")]
  public async Task<IActionResult> AddStoreToThePriceGroup([FromBody] StoreLink store)
  {
    bool isAdded = await _priceGroupService.AddStoreToThePriceGroupAsync(store);
    return Ok(isAdded);
  }

  // POST api/PriceGroup/DeleteStoreForSelectedPriceGroup
  [HttpPost]
  [Route("DeleteStoreForSelectedPriceGroup")]
  public async Task<IActionResult> DeleteStoreForSelectedPriceGroup([FromBody] StoreLink store)
  {
    bool isDeleted = await _priceGroupService.DeleteStoreForSelectedPriceGroupAsync(store);
    return Ok(isDeleted);
  }
}
