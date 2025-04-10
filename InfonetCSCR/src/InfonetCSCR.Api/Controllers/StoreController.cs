namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class StoreController : ControllerBase
{
  private readonly IStoreService _storeService;

  public StoreController(IStoreService storeService)
  {
    _storeService = storeService;
  }

  // POST api/Store/GetStoreIdsByVendorIds
  [HttpPost]
  [Route("GetStoreIdsByVendorIds")]
  public async Task<IActionResult> GetStoreIdsByVendorIds([FromBody] List<string> vendorIds)
  {
    return Ok(await _storeService.GetStoreIdsByVendorIds(vendorIds));
  }

  // GET api/Store/GetStoresWithRegionAndPriceGroup
  [HttpGet]
  [Route("GetStoresWithRegionAndPriceGroup")]
  public async Task<IActionResult> GetStoresWithRegionAndPriceGroup()
  {
    return Ok(await _storeService.GetStoresWithRegionAndPriceGroup());
  }
  
  // GET api/Store/GetAllStoresList
  [HttpGet]
  [Route("GetAllStoresList")]
  public async Task<IActionResult> GetAllStoresList()
  {
    return Ok(await _storeService.GetAllStoresListAsync());
  }

  // GET api/Store/GetStoreByStoreId
  [HttpGet]
  [Route("GetStoreByStoreId")]
  public async Task<IActionResult> GetStoreByStoreId(string storeId)
  {
    Store store = await _storeService.GetStoreByStoreId(storeId);
    return Ok(store);
  }

  // POST api/Store/AddStore
  [HttpPost]
  [Route("AddStore")]
  public async Task<IActionResult> AddStore(Store store, string? copyStoreId)
  {
    bool isAdded = await _storeService.AddStore(store, copyStoreId);
    return Ok(isAdded);
  }
  
  // POST api/Store/UpdateStore
  [HttpPost]
  [Route("UpdateStore")]
  public async Task<IActionResult> UpdateStore(Store store)
  {
    bool isUpdated = await _storeService.UpdateStore(store);
    return Ok(isUpdated);
  }

  // POST api/Store/DeleteStore
  [HttpPost]
  [Route("DeleteStore")]
  public async Task<IActionResult> DeleteStore([FromBody] string storeId)
  {
    bool isDeleted = await _storeService.DeleteStore(storeId);
    return Ok(isDeleted);
  }

  // GET api/Region/GetStoreBySelectedRegion
  [HttpGet]
  [Route("GetStoreBySelectedRegion")]
  public async Task<IActionResult> GetStoreBySelectedRegion(string regionId)
  {
    IList<Store>? stores = await _storeService.GetStoreBySelectedRegion(regionId);
    return Ok(stores);
  }
}
