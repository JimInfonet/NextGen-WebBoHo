namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class CashDrawsController : ControllerBase
{
  private readonly ICashDrawsService _cashDrawsService;

  public CashDrawsController(ICashDrawsService cashDrawsService)
  {
    _cashDrawsService = cashDrawsService;
  }

  // GET api/CashDraws/GetAllDrawTypesData
  [HttpGet]
  [Route("GetAllDrawTypesData")]
  public async Task<IActionResult> GetAllDrawTypesData()
  {
    IList<DrawTypes> drawTypes = await _cashDrawsService.GetAllDrawTypesData();
    return Ok(drawTypes);
  }

  // POST api/CashDraws/InsertDrawType
  [HttpPost]
  [Route("InsertDrawType")]
  public async Task<IActionResult> InsertDrawType(DrawTypes drawTypes)
  {
    bool isAdded = await _cashDrawsService.InsertDrawType(drawTypes);
    return Ok(isAdded);
  }

  // POST api/CashDraws/UpdateDrawType
  [HttpPost]
  [Route("UpdateDrawType")]
  public async Task<IActionResult> UpdateDrawType(DrawTypes drawTypes)
  {
    bool isUpdated = await _cashDrawsService.UpdateDrawType(drawTypes);
    return Ok(isUpdated);
  }

  // POST api/CashDraws/DeleteDrawType
  [HttpPost]
  [Route("DeleteDrawType")]
  public async Task<IActionResult> DeleteDrawType(DrawTypes drawTypes)
  {
    bool isDeleted = await _cashDrawsService.DeleteDrawType(drawTypes);
    return Ok(isDeleted);
  }
}
