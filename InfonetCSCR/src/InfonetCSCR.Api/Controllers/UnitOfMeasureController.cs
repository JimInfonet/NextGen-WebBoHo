namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class UnitOfMeasureController : ControllerBase
{
  private readonly IUnitOfMeasureService _unitOfMeasureService;

  public UnitOfMeasureController(IUnitOfMeasureService unitOfMeasureService)
  {
    _unitOfMeasureService = unitOfMeasureService;
  }

  // GET api/UnitOfMeasure/GetAllUnitOfMeasures
  [HttpGet]
  [Route("GetAllUnitOfMeasures")]
  public async Task<IActionResult> GetAllUnitOfMeasures()
  {
    IList<UnitOfMeasure> unitOfMeasures = await _unitOfMeasureService.GetAllUnitOfMeasures();
    return Ok(unitOfMeasures);
  }

  // POST api/UnitOfMeasure/AddUnitOfMeasure
  [HttpPost]
  [Route("AddUnitOfMeasure")]
  public async Task<IActionResult> AddUnitOfMeasure([FromBody] UnitOfMeasure unitOfMeasure)
  {
    bool isAdded = await _unitOfMeasureService.AddUnitOfMeasure(unitOfMeasure);
    return Ok(isAdded);
  }

  // POST api/UnitOfMeasure/UpdateUnitOfMeasure
  [HttpPost]
  [Route("UpdateUnitOfMeasure")]
  public async Task<IActionResult> UpdateUnitOfMeasure([FromBody] UnitOfMeasure unitOfMeasure)
  {
    bool isUpdated = await _unitOfMeasureService.UpdateUnitOfMeasure(unitOfMeasure);
    return Ok(isUpdated);
  }

  // POST api/UnitOfMeasure/DeleteUnitOfMeasure
  [HttpPost]
  [Route("DeleteUnitOfMeasure")]
  public async Task<IActionResult> DeleteUnitOfMeasure([FromBody] string unitOfMeasureId)
  {
    bool isDeleted = await _unitOfMeasureService.DeleteUnitOfMeasure(unitOfMeasureId);
    return Ok(isDeleted);
  }
}
