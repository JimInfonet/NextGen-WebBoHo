namespace InfonetCSCR.Web.Controllers;

public class StoreShiftsController : Controller
{
  private readonly IStoreShiftsService _storeShiftsService;

  public StoreShiftsController(IStoreShiftsService storeShiftsService)
  {
    _storeShiftsService = storeShiftsService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetPolicies()
  {
    return Json(_storeShiftsService.GetPolicies());
  }

  [HttpGet]
  public async Task<IActionResult> GetStoreShifts()
  {
    IList<StoreShiftViewModel>? storeShifts = await _storeShiftsService.GetStoreShifts();
    return Ok(storeShifts);
  }

  public async Task<IActionResult> AddStoreShift([FromBody] StoreShiftViewModel storeShift)
  {
    bool isAdded = await _storeShiftsService.AddStoreShift(storeShift);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeleteStoreShift(int shiftNumber)
  {
    bool isDeleted = await _storeShiftsService.DeleteStoreShift(shiftNumber);
    return Ok(isDeleted);
  }
  
  public async Task<IActionResult> UpdateStoreShifts([FromBody] IList<StoreShiftViewModel> storeShifts, bool isUpdateAll)
  {
    bool isUpdated = await _storeShiftsService.UpdateStoreShifts(storeShifts, isUpdateAll);
    return Ok(isUpdated);
  }
}
