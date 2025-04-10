namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class RegionController : ControllerBase
{
  private readonly IRegionService _regionService;

  public RegionController(IRegionService regionService)
  {
    _regionService = regionService;
  }

  [HttpGet]
  [Route("GetRegion")]
  public async Task<IActionResult> GetRegion()
  {
    return Ok(await _regionService.GetRegion());
  }

  // POST api/Region/AddRegion
  [HttpPost]
  [Route("AddRegion")]
  public async Task<IActionResult> AddRegion([FromBody] Region region)
  {
    bool isAdded = await _regionService.AddRegion(region);
    return Ok(isAdded);
  }

  // POST api/Region/DeleteRegion
  [HttpPost]
  [Route("DeleteRegion")]
  public async Task<IActionResult> DeleteRegion([FromBody] string regionId)
  {
    bool isDeleted = await _regionService.DeleteRegion(regionId);
    return Ok(isDeleted);
  }

  // GET api/Region/GetStoreToLinkBySelectedRegion
  [HttpGet]
  [Route("GetStoreToLinkBySelectedRegion")]
  public async Task<IActionResult> GetStoreToLinkBySelectedRegion(string regionId)
  {
    IList<Store>? stores = await _regionService.GetStoreToLinkBySelectedRegion(regionId);
    return Ok(stores);
  }

  // POST api/Region/AddStoreToTheRegion
  [HttpPost]
  [Route("AddStoreToTheRegion")]
  public async Task<IActionResult> AddStoreToTheRegion([FromBody] StoreLink store)
  {
    bool isAdded = await _regionService.AddStoreToTheRegion(store);
    return Ok(isAdded);
  }

  // POST api/Region/DeleteStoreForSelectedRegion
  [HttpPost]
  [Route("DeleteStoreForSelectedRegion")]
  public async Task<IActionResult> DeleteStoreForSelectedRegion([FromBody] StoreLink store)
  {
    bool isDeleted = await _regionService.DeleteStoreForSelectedRegion(store);
    return Ok(isDeleted);
  }
}
