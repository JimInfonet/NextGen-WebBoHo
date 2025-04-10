namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class GenericController : ControllerBase
{
  private readonly IGenericService _genericService;

  public GenericController(IGenericService genericService)
  {
    _genericService = genericService;
  }

  // GET api/Generic/GetAllGenerics
  [HttpGet]
  [Route("GetAllGenerics")]
  public async Task<IActionResult> GetAllGenerics()
  {
    IList<Generic> generics = await _genericService.GetAllGenerics();
    return Ok(generics);
  }

  // POST api/Generic/AddGeneric
  [HttpPost]
  [Route("AddGeneric")]
  public async Task<IActionResult> AddGeneric([FromBody] Generic generic)
  {
    bool isAdded = await _genericService.AddGeneric(generic);
    return Ok(isAdded);
  }

  // POST api/Generic/UpdateGeneric
  [HttpPost]
  [Route("UpdateGeneric")]
  public async Task<IActionResult> UpdateGeneric([FromBody] Generic generic)
  {
    bool isUpdated = await _genericService.UpdateGeneric(generic);
    return Ok(isUpdated);
  }

  // POST api/Generic/DeleteGeneric
  [HttpPost]
  [Route("DeleteGeneric")]
  public async Task<IActionResult> DeleteGeneric([FromBody] string genericId)
  {
    bool isDeleted = await _genericService.DeleteGeneric(genericId);
    return Ok(isDeleted);
  }
}
