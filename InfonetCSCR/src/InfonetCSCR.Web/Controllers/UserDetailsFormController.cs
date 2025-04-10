namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class UserDetailsFormController : Controller
{
  private const string _timeSheetAndHaveUserIdPolicy = "timeSheetsAndHaveUserIdPolicy";
  private const string _initialEmployeeId = "initialEmployeeId";
  private const string _isUpdate = "isUpdate";
  private const string _IsUserAlreadyExists = "IsUserAlreadyExists";
  private const string _create = "create";
  private const string _update = "update";
  private readonly IUserDetailsFormService _userDetailsFormService;

  private static bool _isUpdateUserDetails = false;

  public UserDetailsFormController(IUserDetailsFormService userDetailsFormService)
  {
    _userDetailsFormService = userDetailsFormService;
  }

  public async Task<IActionResult> Index(string? code, string? actionMode, string? operationType, string? showToaster)
  {
    _isUpdateUserDetails = !string.IsNullOrEmpty(code);
    ViewData[_timeSheetAndHaveUserIdPolicy] = _userDetailsFormService.GetTimeSheetsAndHaveUserIdPolicy();
    ViewData[_initialEmployeeId] = await _userDetailsFormService.GetInitialEmployeeId();
    ViewData[_isUpdate] = _isUpdateUserDetails;

    if (_isUpdateUserDetails)
    {
      var userDetails = await _userDetailsFormService.GetUserDetails(code);
      return View(userDetails);
    }

    return View();
  }

  public async Task<IActionResult> SaveUserDetails(UserDetailsViewModel userDetailsViewModel)
  {
    ViewData[_isUpdate] = _isUpdateUserDetails;
    ViewData[_timeSheetAndHaveUserIdPolicy] = _userDetailsFormService.GetTimeSheetsAndHaveUserIdPolicy();
    ViewData[_initialEmployeeId] = await _userDetailsFormService.GetInitialEmployeeId();

    if (!_isUpdateUserDetails)
    {
      var userDetails = await _userDetailsFormService.GetUserDetails(userDetailsViewModel.Code);
      if (userDetails != null && userDetails.Code.ToUpper() == userDetailsViewModel.Code.ToUpper())
      {
        ViewData[_IsUserAlreadyExists] = true;
        return View("Index", userDetailsViewModel);
      }
    }

    await _userDetailsFormService.SaveUserDetails(userDetailsViewModel, _isUpdateUserDetails);
    return RedirectToAction("Index", new
    {
      code = userDetailsViewModel.Code,
      operationType = _isUpdateUserDetails ? _update : _create,
      showToaster = true
    });
  }

  public async Task<IActionResult> DeleteUserDetails([FromBody] string code)
  {
    await _userDetailsFormService.DeleteUserDetails(code);
    return NoContent();
  }
}
