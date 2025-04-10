namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CashDrawsController : Controller
{
  private readonly ICashDrawsService _cashDrawsService;

  public CashDrawsController(ICashDrawsService cashDrawsService)
  {
    _cashDrawsService = cashDrawsService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> GetAllDrawTypesData()
  {
    IList<DrawTypesViewModel>? drawTypes = await _cashDrawsService.GetAllDrawTypesData();
    return Ok(drawTypes);
  }

  public async Task<IActionResult> InsertDrawType([FromBody] DrawTypesViewModel drawTypes)
  {
    bool isAdded = await _cashDrawsService.InsertDrawType(drawTypes);
    return Ok(isAdded);
  }

  public async Task<IActionResult> UpdateDrawType([FromBody] DrawTypesViewModel drawTypes)
  {
    bool isUpdated = await _cashDrawsService.UpdateDrawType(drawTypes);
    return Ok(isUpdated);
  }

  public async Task<IActionResult> DeleteDrawType([FromBody] DrawTypesViewModel drawTypes)
  {
    bool isDeleted = await _cashDrawsService.DeleteDrawType(drawTypes);
    return Ok(isDeleted);
  }
}
