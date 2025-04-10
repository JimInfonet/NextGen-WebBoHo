namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class EODGroupController : ControllerBase
{
  private readonly IEODGroupService _eodGroupService;

  public EODGroupController(IEODGroupService eodGroupService)
  {
    _eodGroupService = eodGroupService;
  }

  // GET api/EODGroup/CheckGroupIdExists/{eodGroupId}
  [HttpGet]
  [Route("CheckGroupIdExists/{eodGroupId}")]
  public async Task<IActionResult> CheckGroupIdExists(string eodGroupId)
  {
    bool isGroupIdExists = await _eodGroupService.CheckGroupIdExists(eodGroupId);
    return Ok(isGroupIdExists);
  }

  // GET api/EODGroup/GetGroups
  [HttpGet]
  [Route("GetGroups")]
  public async Task<IActionResult> GetGroups()
  {
    IEnumerable<EodGroup> eodGroups = await _eodGroupService.GetEODGroups();
    return Ok(eodGroups);
  }

  // POST api/EODGroup/AddEODGroup
  [HttpPost]
  [Route("AddEODGroup")]
  public async Task<IActionResult> AddEODGroup(EodGroup eodGroup)
  {
    bool isEodGroupAdded = await _eodGroupService.AddEODGroup(eodGroup);
    return Ok(isEodGroupAdded);
  }

  // POST api/EODGroup/DeleteEodGroupById/{isDeptExistsForEodGroupId}/{eodGroupId}
  [HttpPost]
  [Route("DeleteEodGroupById/{isDeptExistsForEodGroupId}/{eodGroupId}")]
  public async Task<IActionResult> DeleteEodGroupById(bool isDeptExistsForEodGroupId, string eodGroupId)
  {
    bool result = await _eodGroupService.DeleteEodGroupById(isDeptExistsForEodGroupId, eodGroupId);
    return Ok(result);
  }

  // POST api/EODGroup/UpdateEODGroupByDept/{eodGroupId}/{dept}
  [HttpPost]
  [Route("UpdateEODGroupByDept/{eodGroupId}/{dept}")]
  public async Task<IActionResult> UpdateEODGroupByDept(string eodGroupId, string dept)
  {
    bool result = await _eodGroupService.UpdateEODGroupByDept(eodGroupId, dept);
    return Ok(result);
  }
}
