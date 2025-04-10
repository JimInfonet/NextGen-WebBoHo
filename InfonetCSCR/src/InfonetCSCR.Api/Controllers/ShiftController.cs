namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class ShiftController : ControllerBase
{
  private readonly IShiftService _shiftService;

  public ShiftController(IShiftService shiftService)
  {
    _shiftService = shiftService;
  }

  // GET api/Shift/GetShiftIdAndDateByShiftDate
  [HttpGet]
  [Route("GetShiftIdAndDateByShiftDate")]
  public async Task<IActionResult> GetShiftIdAndDateByShiftDate(string shiftDate)
  {
    IEnumerable<ShiftRecHeaderDto>? shiftRecHeaders = await _shiftService.GetShiftIdAndDateByShiftDate(shiftDate);
    return Ok(shiftRecHeaders);
  }

  // GET api/Shift/GetShiftNumberByShiftDate
  [HttpGet]
  [Route("GetShiftNumberByShiftDate")]
  public async Task<IActionResult> GetShiftNumberByShiftDate(string? shiftDate)
  {
    IList<string>? shiftRecHeaders = await _shiftService.GetShiftNumberByShiftDate(shiftDate ?? "");
    return Ok(shiftRecHeaders);
  }
}