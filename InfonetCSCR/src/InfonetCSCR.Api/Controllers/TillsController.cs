namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class TillsController : ControllerBase
{
  private readonly ITillsService _tillsService;

  public TillsController(ITillsService tillsService)
  {
    _tillsService = tillsService;
  }

  // GET api/Tills/GetPumpFeatures
  [HttpGet]
  [Route("GetPumpFeatures")]
  public async Task<IActionResult> GetPumpFeatures()
  {
    string pumpFeatures = await _tillsService.GetPumpFeatures();
    return Ok(pumpFeatures);
  }

  // GET api/Tills/GetMaxTill
  [HttpGet]
  [Route("GetMaxTill")]
  public async Task<IActionResult> GetMaxTill(int tillNo)
  {
    int mTill = await _tillsService.GetMaxTill(tillNo);
    return Ok(mTill);
  }

  // GET api/Tills/GetTills
  [HttpGet]
  [Route("GetTills")]
  public async Task<IActionResult> GetTills(int maxTill, bool isCashBonusPolicy)
  {
    IList<Till> tills = await _tillsService.GetTills(maxTill, isCashBonusPolicy);
    return Ok(tills);
  }

  // POST api/Tills/AddTill
  [HttpPost]
  [Route("AddTill")]
  public async Task<IActionResult> AddTill([FromBody] Till till)
  {
    bool isAdded = await _tillsService.AddTill(till);
    return Ok(isAdded);
  }

  // GET api/Tills/GetTillsCount
  [HttpGet]
  [Route("GetTillsCount")]
  public async Task<IActionResult> GetTillsCount(int tillNo)
  {
    int tillCount = await _tillsService.GetTillsCount(tillNo);
    return Ok(tillCount);
  }

  // POST api/Tills/DeleteTill
  [HttpPost]
  [Route("DeleteTill")]
  public async Task<IActionResult> DeleteTill([FromBody] int tillNo)
  {
    bool isDeleted = await _tillsService.DeleteTill(tillNo);
    return Ok(isDeleted);
  }

  // POST api/Tills/UpdateTillByFlipActiveOrFlipProcess
  [HttpPost]
  [Route("UpdateTillByFlipActiveOrFlipProcess")]
  public async Task<IActionResult> UpdateTillByFlipActiveOrFlipProcess([FromBody] Till till)
  {
    bool isUpdated = await _tillsService.UpdateTillByFlipActiveOrFlipProcess(till);
    return Ok(isUpdated);
  }
}
