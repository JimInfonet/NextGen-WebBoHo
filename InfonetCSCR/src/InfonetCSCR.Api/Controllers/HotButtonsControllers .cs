namespace InfonetCSCR.Api.Controllers;

[Route("api/HotButtons")]
[ApiController]
[Authorize]

public class HotButtonsController : ControllerBase
{
  private readonly IHotButtonsService _hotButtonsService;

  public HotButtonsController(IHotButtonsService hotButtonsService)
  {
    _hotButtonsService = hotButtonsService;
  }

  // GET api/HotButtons/GetHotButtonsWithProductName
  [HttpGet]
  [Route("GetHotButtonsWithProductName")]
  public async Task<IActionResult> GetHotButtonsWithProductName()
  {
    IEnumerable<HotButtonsWithProductName> hotButtons = await _hotButtonsService.GetHotButtonsWithProductName();
    return Ok(hotButtons);
  }

  // POST api/HotButtons/GetHotButtonsByButtonIdRange
  [HttpPost]
  [Route("GetHotButtonsByButtonIdRange")]
  public async Task<IActionResult> GetHotButtonsByButtonIdRange(HotButtonsIdRangeRecord hotButtonsIdRangeRecord)
  {
    IList<HotButtons> hotButtons = await _hotButtonsService.GetHotButtonsByButtonIdRange(hotButtonsIdRangeRecord);
    return Ok(hotButtons);
  }

  // POST api/HotButtons/InsertHotButtons
  [HttpPost]
  [Route("InsertHotButtons")]
  public async Task<IActionResult> InsertHotButtons(HotButtons hotButtons)
  {
    bool isAdded = await _hotButtonsService.InsertHotButtons(hotButtons);
    return Ok(isAdded);
  }

  // POST api/HotButtons/UpdateHotButtonsColor
  [HttpPost]
  [Route("UpdateHotButtonsColor")]
  public async Task<IActionResult> UpdateHotButtonsColor(HotButtons hotButtons)
  {
    bool isColorUpdated = await _hotButtonsService.UpdateHotButtonsColor(hotButtons);
    return Ok(isColorUpdated);
  }

  // POST api/HotButtons/DeleteHotButton
  [HttpPost]
  [Route("DeleteHotButton")]
  public async Task<IActionResult> DeleteHotButton(HotButtons hotButtons)
  {
    bool isDeleted = await _hotButtonsService.DeleteHotButton(hotButtons);
    return Ok(isDeleted);
  }

  // GET api/HotButtons/GetBottleReturnDataByProductCode/{productCode}
  [HttpGet]
  [Route("GetBottleReturnDataByProductCode/{productCode}")]
  public async Task<IActionResult> GetBottleReturnDataByProductCode(string productCode)
  {
    HotButtons hotButton = await _hotButtonsService.GetBottleReturnDataByProductCode(productCode);
    return Ok(hotButton);
  }
}
