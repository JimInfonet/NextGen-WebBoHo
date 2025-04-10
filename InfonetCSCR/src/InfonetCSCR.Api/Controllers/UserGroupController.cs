namespace InfonetCSCR.Api.Controllers;

[Route("api/UserGroup")]
[ApiController]
[Authorize]

public class UserGroupController : ControllerBase
{
  public readonly IUserGroupService _userGroupService;

  public UserGroupController(IUserGroupService userGroupService)
  {
    _userGroupService = userGroupService;
  }

  // GET api/UserGroup/GetAllUserGroups
  [HttpGet]
  [Route("GetAllUserGroups")]
  public async Task<IActionResult> GetAllUserGroups()
  {
    IEnumerable<UserGroup>? userDetails = await _userGroupService.GetAllUserGroups();
    return Ok(userDetails);
  }

  // GET api/UserGroup/GetUserGroup
  [HttpGet]
  [Route("GetUserGroup")]
  public async Task<IActionResult> GetUserGroup(string groupCode)
  {
    UserGroup? userDetail = await _userGroupService.GetUserGroup(groupCode);
    return Ok(userDetail);
  }

  // POST api/UserGroup/AddUserGroup
  [HttpPost]
  [Route("AddUserGroup")]
  public async Task<IActionResult> AddUserGroup(UserGroup userGroup)
  {
    bool isAdded = await _userGroupService.AddUserGroup(userGroup);
    return Ok(isAdded);
  }

  // POST api/UserGroup/RemoveUserGroup
  [HttpPost]
  [Route("RemoveUserGroup")]
  public async Task<IActionResult> RemoveUserGroup([FromBody] string groupCode)
  {
    bool isDeleted = await _userGroupService.RemoveUserGroup(groupCode);
    return Ok(isDeleted);
  }

  // POST api/UserGroup/GetSecurityLevels
  [HttpPost]
  [Route("GetSecurityLevels")]
  public async Task<IActionResult> GetSecurityLevels([FromBody] UserDetails userDetails)
  {
    IEnumerable<SecurityLevel>? securityLevels = await _userGroupService.GetSecurityLevels(userDetails);
    return Ok(securityLevels);
  }

  // GET api/UserGroup/GetAllUsersByGroup
  [HttpGet]
  [Route("GetAllUsersByGroup")]
  public async Task<IActionResult> GetAllUsersByGroup(string groupCode)
  {
    IEnumerable<UserDetails>? groupMembers = await _userGroupService.GetAllUsersByGroup(groupCode);
    return Ok(groupMembers);
  }
}
