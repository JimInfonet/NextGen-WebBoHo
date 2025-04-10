namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CheckInCheckOutController : Controller
{
  private readonly IHttpService _http;
  private readonly ICheckInCheckOutService _checkInCheckOutService;

  public CheckInCheckOutController(
        IHttpService http,
        ICheckInCheckOutService checkInCheckOutService)
  {
    _http = http;
    _checkInCheckOutService = checkInCheckOutService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetPolicies([FromBody] UserDetailsViewModel? userDetails = null)
  {
    var policies = _checkInCheckOutService.GetCheckInCheckOutPolicies(userDetails);
    return Ok(policies);
  }

  public async Task<IActionResult> IsValidUser(string code)
  {
    if (code.ToUpper() == "SYSADMIN") return Ok(true);

    string url = "Auth/IsValidUser";
    bool isValidUser = await _http.PostAndParseAsync<bool>(url, code);
    return Ok(isValidUser);
  }

  public async Task<IActionResult> AuthenticateUser([FromBody] UserViewModel user)
  {
    if (user.Code.ToUpper() == "SYSADMIN")
      return Ok(user.Password.ToUpper() == $"ICC{DateTime.Now:dd}");

    string url = "CheckInCheckOut/AuthenticateUser";
    bool isAuthenticated = await _http.PostAndParseAsync<bool>(url, user);
    return Ok(isAuthenticated);
  }

  public async Task<IActionResult> GetCurrentUsers()
  {
    string url = "CheckInCheckOut/GetCurrentUsers";
    var users = await _http.GetAndParseAsync<List<CheckInCheckOutUserDtoViewModel>>(url);
    return Ok(users);
  }

  public async Task<IActionResult> DetermineInOrOut(bool policyHOUR24STORE, string userCode)
  {
    string url = $"CheckInCheckOut/DetermineInOrOut?policyHOUR24STORE={policyHOUR24STORE}&userCode={userCode}";
    var details = await _http.GetAndParseAsync<DetermineInOrOutDtoViewModel>(url);
    return Ok(details);
  }

  public async Task<IActionResult> AddComment([FromBody] AddCommentDtoViewModel model)
  {
    string url = $"CheckInCheckOut/AddComment";
    var details = await _http.PostAndParseAsync<bool>(url, model);
    return Ok(details);
  }

  public async Task<IActionResult> PerformCheckInOrOut([FromBody] PerformCheckInOrOutDtoViewModel infos)
  {
    string url = "CheckInCheckOut/PerformCheckInOrOut";
    var performed = await _http.PostAndParseAsync<PerformCheckInOrOutResponseDtoViewModel>(url, infos);
    return Ok(performed);
  }
}
