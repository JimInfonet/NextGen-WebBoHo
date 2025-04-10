namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class FamilyController : ControllerBase
{
  private readonly IFamilyService _familyService;

  public FamilyController(IFamilyService familyService)
  {
    _familyService = familyService;
  }

  // GET api/Family/GetAllFamily
  [HttpGet]
  [Route("GetAllFamily")]
  public async Task<IActionResult> GetAllFamily()
  {
    IList<Family> families = await _familyService.GetAllFamily();
    return Ok(families);
  }

  // POST api/Family/AddFamily
  [HttpPost]
  [Route("AddFamily")]
  public async Task<IActionResult> AddFamily([FromBody] Family family)
  {
    bool isAdded = await _familyService.AddFamily(family);
    return Ok(isAdded);
  }

  // POST api/Family/UpdateFamily
  [HttpPost]
  [Route("UpdateFamily")]
  public async Task<IActionResult> UpdateFamily([FromBody] Family family)
  {
    bool isUpdated = await _familyService.UpdateFamily(family);
    return Ok(isUpdated);
  }

  // POST api/Family/DeleteFamily
  [HttpPost]
  [Route("DeleteFamily")]
  public async Task<IActionResult> DeleteFamily([FromBody] string familyId)
  {
    bool isDeleted = await _familyService.DeleteFamily(familyId);
    return Ok(isDeleted);
  }
}
