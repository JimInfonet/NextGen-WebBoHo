namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class GroupController : ControllerBase
{
  private readonly IGroupService _groupService;

  public GroupController(IGroupService groupService)
  {
    _groupService = groupService;
  }

  // GET api/Group/GetAllGroups
  [HttpGet]
  [Route("GetAllGroups")]
  public async Task<IActionResult> GetAllGroups()
  {
    IList<FamilyGroupJoinFamily> groups = await _groupService.GetAllGroups();
    return Ok(groups);
  }

  // POST api/Group/AddGroup
  [HttpPost]
  [Route("AddGroup")]
  public async Task<IActionResult> AddGroup([FromBody] FamilyGroupRecord group)
  {
    bool isAdded = await _groupService.AddGroup(group);
    return Ok(isAdded);
  }

  // POST api/Group/DeleteGroup
  [HttpPost]
  [Route("DeleteGroup")]
  public async Task<IActionResult> DeleteGroup([FromBody] FamilyGroupJoinFamily group)
  {
    bool isDeleted = await _groupService.DeleteGroup(group);
    return Ok(isDeleted);
  }

  // Get api/Group/GetAllFamilyGroups
  [HttpGet]
  [Route("GetAllFamilyGroups")]
  public async Task<IActionResult> GetAllFamilyGroups()
  {
    IList<FamilyGroup> groups = await _groupService.GetAllFamilyGroups();
    return Ok(groups);
  }
}
