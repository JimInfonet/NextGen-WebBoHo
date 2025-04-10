namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class UserGroupFormController : Controller
{
  private const string _isUserGroupAlreadyExists = "isUserGroupAlreadyExists";
  private const string _createGroup = "createGroup";
  private readonly IUserGroupFormService _userGroupFormService;

  public UserGroupFormController(IUserGroupFormService userGroupFormService)
  {
    _userGroupFormService = userGroupFormService;
  }

  public async Task<IActionResult> Index()
  {
    return View();
  }

  public async Task<IActionResult> GetSecurityLevels()
  {
    IEnumerable<SecurityLevelViewModel>? securityLevels = await _userGroupFormService.GetSecurityLevels();
    return Ok(securityLevels);
  }

  public async Task<IActionResult> SaveUserGroup(UserGroupViewModel userGroupViewModel)
  {
    var userGroupFromDb = await _userGroupFormService.GetUserGroup(userGroupViewModel.Code);
    if (userGroupFromDb != null && userGroupFromDb.Code.ToUpper() == userGroupViewModel.Code.ToUpper())
    {
      ViewData[_isUserGroupAlreadyExists] = true;
      return View("Index", userGroupViewModel);
    }

    await _userGroupFormService.AddUserGroup(userGroupViewModel);
    return RedirectToAction("Index", "UserGrid", new { actionMode = _createGroup, showToaster = true });
  }
}
