namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class StoreShiftsController : ControllerBase
{
  private readonly IStoreShiftsService _storeShiftsService;

  public StoreShiftsController(IStoreShiftsService storeShiftsService)
  {
    _storeShiftsService = storeShiftsService;
  }

  // GET api/StoreShifts/GetStoreShifts
  [HttpGet]
  [Route("GetStoreShifts")]
  public async Task<IActionResult> GetStoreShifts()
  {
    IList<StoreShift> storeShifts = await _storeShiftsService.GetStoreShifts();
    return Ok(storeShifts);
  }

  // POST api/StoreShifts/AddStoreShift
  [HttpPost]
  [Route("AddStoreShift")]
  public async Task<IActionResult> AddStoreShift([FromBody] StoreShiftDto storeShifts)
  {
    bool isAdded = await _storeShiftsService.AddStoreShift(storeShifts);
    return Ok(isAdded);
  }

  // POST api/StoreShifts/DeleteStoreShift
  [HttpPost]
  [Route("DeleteStoreShift")]
  public async Task<IActionResult> DeleteStoreShift([FromBody] int shiftNumber)
  {
    bool isDeleted = await _storeShiftsService.DeleteStoreShift(shiftNumber);
    return Ok(isDeleted);
  }

  // POST api/StoreShifts/UpdateStoreShifts
  [HttpPost]
  [Route("UpdateStoreShifts")]
  public async Task<IActionResult> UpdateStoreShifts([FromBody] IList<StoreShiftDto> storeShifts, bool isUpdateAll)
  {
    bool isUpdated = await _storeShiftsService.UpdateStoreShifts(storeShifts, isUpdateAll);
    return Ok(isUpdated);
  }
}
