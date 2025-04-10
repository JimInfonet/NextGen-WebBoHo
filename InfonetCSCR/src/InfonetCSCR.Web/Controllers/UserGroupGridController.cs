namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class UserGroupGridController : Controller
{
  private readonly IUserGroupGridService _userGroupGridService;

  public UserGroupGridController(IUserGroupGridService userGroupGridService)
  {
    _userGroupGridService = userGroupGridService;
  }

  public async Task<IActionResult> GetAllUserGroups()
  {
    IEnumerable<UserGroupViewModel>? userGroups = await _userGroupGridService.GetAllUserGroups();
    return Ok(userGroups);
  }

  public async Task<IActionResult> RemoveUserGroup([FromBody] string groupCode)
  {
    bool isDeleted = await _userGroupGridService.RemoveUserGroup(groupCode);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllUsersByGroup(string groupCode)
  {
    IEnumerable<UserDetailsViewModel>? groupMembers = await _userGroupGridService.GetAllUsersByGroup(groupCode);
    return Ok(groupMembers);
  }
}
