namespace InfonetCSCR.Web.Controllers;

public class TillsController : Controller
{
  private readonly ITillsService _tillsService;

  public TillsController(ITillsService tillsService)
  {
    _tillsService = tillsService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetPolicies()
  {
    return Json(_tillsService.GetPolicies());
  }

  public async Task<IActionResult> GetPumpFeatures()
  {
    string pumpFeatures = await _tillsService.GetPumpFeatures();
    return Json(pumpFeatures);
  }

  public async Task<IActionResult> GetMaxTill(int tillNo)
  {
    int mTill = await _tillsService.GetMaxTill(tillNo);
    return Ok(mTill);
  }

  public async Task<IActionResult> GetTills(int maxTill)
  {
    IList<TillViewModel>? tills = await _tillsService.GetTills(maxTill);
    return Ok(tills);
  }

  public async Task<IActionResult> AddTill([FromBody] TillViewModel till)
  {
    bool isAdded = await _tillsService.AddTill(till);
    return Ok(isAdded);
  }

  public async Task<IActionResult> GetTillsCount(int tillNo)
  {
    int tillCount = await _tillsService.GetTillsCount(tillNo);
    return Ok(tillCount);
  }
  public async Task<IActionResult> DeleteTill(int tillNo)
  {
    bool isDeleted = await _tillsService.DeleteTill(tillNo);
    return Ok(isDeleted);
  }
  
  public async Task<IActionResult> UpdateTillByFlipActiveOrFlipProcess([FromBody] TillViewModel till)
  {
    bool isUpdated = await _tillsService.UpdateTillByFlipActiveOrFlipProcess(till);
    return Ok(isUpdated);
  }
}
