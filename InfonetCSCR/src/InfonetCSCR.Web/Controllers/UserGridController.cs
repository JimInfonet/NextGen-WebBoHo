namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class UserGridController : Controller
{
  private const string _timeSheetAndHaveUserIdPolicy = "timeSheetsAndHaveUserIdPolicy";
  private const string _loggedInUserSecurityLevel = "loggedInUserSecurityLevel";
  private const string _UCHGPass = "U_CHGPASS";
  private readonly IUserGridService _userGridService;

  public UserGridController(IUserGridService userGridService)
  {
    _userGridService = userGridService;
  }

  public IActionResult Index(string? actionMode, bool? showToaster)
  {
    ViewData[_timeSheetAndHaveUserIdPolicy] = _userGridService.GetTimeSheetsAndHaveUserIdPolicy();
    ViewData[_loggedInUserSecurityLevel] = _userGridService.GetSecurityLevelForLoggedInUser();
    ViewData[_UCHGPass] = _userGridService.GetUCHGPassPolicy();
    return View();
  }

  public async Task<IActionResult> GetAllUserDetails()
  {
    IEnumerable<UserDetailsViewModel> userDetails = await _userGridService.GetAllUserDetails();
    return Ok(userDetails);
  }

  public async Task<IActionResult> GetGroupInfo(string userCode)
  {
    IEnumerable<GroupInfoViewModel>? groupInfo = await _userGridService.GetGroupInfo(userCode);
    return Ok(groupInfo);
  }

  public async Task<IActionResult> GetGroupCodes()
  {
    IEnumerable<string>? groupCodes = await _userGridService.GetGroupCodes();
    return Ok(groupCodes);
  }

  public async Task<IActionResult> UpdateGroupInfo(string user, string? group)
  {
    await _userGridService.UpdateGroupInfo(user, group);
    return Ok(null);
  }
  
  public async Task<IActionResult> DeleteUIGRecord(string user, string? group)
  {
    bool isDeleted = await _userGridService.DeleteUIGRecord(user, group);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetUserCodes()
  {
    IEnumerable<string>? userCodes = await _userGridService.GetUserCodes();
    return Ok(userCodes);
  }
  
  public async Task<IActionResult> ChangeUserPassword([FromBody] UserDetailsViewModel userDetailsViewModel)
  {
    await _userGridService.ChangeUserPassword(userDetailsViewModel);
    return Ok(null);
  }

  public async Task<IActionResult> GetMaxUserID()
  {
    return Ok(await _userGridService.GetMaxUserIDAsync());
  }
}
