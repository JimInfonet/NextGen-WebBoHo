namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class RestrictionsController : ControllerBase
{
  private readonly IRestrictionsService _restrictionsService;

  public RestrictionsController(IRestrictionsService restrictionsService)
  {
    _restrictionsService = restrictionsService;
  }

  // GET api/Restrictions/GetRestrictions
  [HttpGet]
  [Route("GetRestrictions")]
  public async Task<IActionResult> GetRestrictions()
  {
    IList<Restriction> restrictions = await _restrictionsService.GetRestrictions();
    return Ok(restrictions);
  }

  // POST api/Restrictions/RestrictionAlreadyExists
  [HttpPost]
  [Route("RestrictionAlreadyExists")]
  public async Task<IActionResult> RestrictionAlreadyExists([FromBody] int code)
  {
    bool alreadyExists = await _restrictionsService.RestrictionAlreadyExists(code);
    return Ok(alreadyExists);
  }


  // POST api/Restrictions/AddRestriction
  [HttpPost]
  [Route("AddRestriction")]
  public async Task<IActionResult> AddRestriction([FromBody] Restriction restriction)
  {
    bool isAdded = await _restrictionsService.AddRestriction(restriction);
    return Ok(isAdded);
  }

  // POST api/Restrictions/UpdateRestriction
  [HttpPost]
  [Route("UpdateRestriction")]
  public async Task<IActionResult> UpdateRestriction([FromBody] Restriction restriction)
  {
    bool isUpdated = await _restrictionsService.UpdateRestriction(restriction);
    return Ok(isUpdated);
  }

  // POST api/Restrictions/DeleteRestriction
  [HttpPost]
  [Route("DeleteRestriction")]
  public async Task<IActionResult> DeleteRestriction([FromBody] int code)
  {
    bool isDeleted = await _restrictionsService.DeleteRestriction(code);
    return Ok(isDeleted);
  }
}
