namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class RegionController : Controller
{
  private readonly IHttpService _http;
  private readonly IRegionService _regionService;

  public RegionController(IHttpService http, IRegionService regionService)
  {
    _http = http;
    _regionService = regionService;
  }

  public IActionResult Index()
  {
    return View();
  }

  [HttpGet]
  public async Task<IActionResult> GetRegion()
  {
    var url = "Region/GetRegion";
    var regions = await _http.GetAndParseAsync<List<RegionViewModel>>(url);
    return Ok(regions);
  }

  public async Task<IActionResult> SaveRegion([FromBody] RegionViewModel region)
  {
    bool isAdded = await _regionService.AddRegion(region);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeleteRegion(string regionId)
  {
    bool isDeleted = await _regionService.DeleteRegion(regionId);
    return Ok(isDeleted);
  }
  
  public async Task<IActionResult> GetStoreToLinkBySelectedRegion(string regionId)
  {
    IList<StoreViewModel>? stores = await _regionService.GetStoreToLinkBySelectedRegion(regionId);
    return Ok(stores);
  }

  public async Task<IActionResult> AddStoreToTheRegion([FromBody] StoreLinkViewModel store)
  {
    bool isAdded = await _regionService.AddStoreToTheRegion(store);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeleteStoreForSelectedRegion([FromBody] StoreLinkViewModel store)
  {
    bool isDeleted = await _regionService.DeleteStoreForSelectedRegion(store);
    return Ok(isDeleted);
  }
}
