namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class ReasonsController : ControllerBase
{
  private readonly IReasonsService _reasonsService;

  public ReasonsController(IReasonsService reasonsService)
  {
    _reasonsService = reasonsService;
  }

  // GET api/Reasons/GetReasons
  [HttpGet]
  [Route("GetReasons")]
  public async Task<IActionResult> GetReasons(string reasonType)
  {
    IList<Reason> reasons = await _reasonsService.GetReasons(reasonType);
    return Ok(reasons);
  }

  [HttpPost]
  [Route("ReasonAlreadyExists")]
  public async Task<IActionResult> ReasonAlreadyExists([FromBody] Reason reason)
  {
    bool alreadyExists = await _reasonsService.ReasonAlreadyExists(reason);
    return Ok(alreadyExists);
  }

  // POST api/Reasons/AddReason
  [HttpPost]
  [Route("AddReason")]
  public async Task<IActionResult> AddReason([FromBody] Reason reason)
  {
    bool isAdded = await _reasonsService.AddReason(reason);
    return Ok(isAdded);
  }

  // POST api/Reasons/UpdateReason
  [HttpPost]
  [Route("UpdateReason")]
  public async Task<IActionResult> UpdateReason([FromBody] Reason reason)
  {
    bool isUpdated = await _reasonsService.UpdateReason(reason);
    return Ok(isUpdated);
  }

  // POST api/Reasons/DeleteReason
  [HttpPost]
  [Route("DeleteReason")]
  public async Task<IActionResult> DeleteReason([FromBody] Reason reason)
  {
    bool isDeleted = await _reasonsService.DeleteReason(reason);
    return Ok(isDeleted);
  }
}
