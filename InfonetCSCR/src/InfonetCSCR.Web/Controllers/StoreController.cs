namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class StoreController : Controller
{
  private readonly IHttpService _http;
  private readonly IStoreService _storeService;

  public StoreController(IHttpService http, IStoreService storeService)
  {
    _http = http;
    _storeService = storeService;
  }

  public IActionResult Index()
  {
    ViewData["isStoreAdded"] = TempData["isStoreAdded"];
    ViewData["isStoreCopied"] = TempData["isStoreCopied"];
    ViewData["isStoreUpdated"] = TempData["isStoreUpdated"];
    return View();
  }

  public async Task<IActionResult> StoreForm(string? storeId)
  {
    bool supportWarehousePolicy = _storeService.GetPolicies();
    ViewData["supportWarehousePolicy"] = supportWarehousePolicy;

    if (storeId == null)
    {
      return View();
    }

    var store = await _storeService.GetStoreByStoreId(storeId);
    return View(store);
  }

  public async Task<IActionResult> GetStoreIdsByVendorIds([FromBody] List<string> vendorIds)
  {
    var url = "Store/GetStoreIdsByVendorIds";
    var storeIds = await _http.PostAndParseAsync<IEnumerable<StoreViewModel>>(url, vendorIds);
    return Ok(storeIds);
  }

  public async Task<IActionResult> GetStoresWithRegionAndPriceGroup()
  {
    var url = "Store/GetStoresWithRegionAndPriceGroup";
    var stores = await _http.GetAndParseAsync<IEnumerable<StoresWithRegionAndPriceGroupViewModel>>(url);
    return Ok(stores);
  }

  public IActionResult GetPolicies()
  {
    return Json(_storeService.GetPolicies());
  }

  [HttpGet]
  public async Task<IActionResult> GetAllStoresList()
  {
    var stores = await _storeService.GetAllStoresList();
    return Ok(stores);
  }

  public async Task<IActionResult> SaveStore(StoreViewModel store)
  {
    bool isAddMode = (store.IsAction == "add");
    bool isEditMode = (store.IsAction == "edit");
    bool isCopyMode = (store.IsAction == "copy");

    if (isAddMode || isCopyMode)
    {
      StoreViewModel isStore = await _storeService.GetStoreByStoreId(store.Store_ID);
      bool isAlreadyExist = (isStore != null);

      if (isAlreadyExist)
      {
        ViewData["supportWarehousePolicy"] = _storeService.GetPolicies();
        ViewData["isAlreadyExist"] = isAlreadyExist.ToString();
        return View("StoreForm");
      }

      bool isAdded = await _storeService.AddStore(store);
      ViewData[isCopyMode ? "isStoreCopied" : "isStoreAdded"] = isAdded.ToString().ToLower();
      TempData[isCopyMode ? "isStoreCopied" : "isStoreAdded"] = isAdded.ToString().ToLower();
      return isAdded ? RedirectToAction("Index") : View("StoreForm");
    }

    if (isEditMode)
    {
      bool isUpdated = await _storeService.UpdateStore(store);
      ViewData["isStoreUpdated"] = isUpdated.ToString().ToLower();
      TempData["isStoreUpdated"] = isUpdated.ToString().ToLower();
      return isUpdated ? RedirectToAction("Index") : View("StoreForm");
    }

    return View("StoreForm");
  }

  public async Task<IActionResult> DeleteStore(string storeId)
  {
    bool isDeleted = await _storeService.DeleteStore(storeId);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetStoreBySelectedRegion(string regionId)
  {
    IList<StoreViewModel>? stores = await _storeService.GetStoreBySelectedRegion(regionId);
    return Ok(stores);
  }
}
