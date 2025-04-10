namespace InfonetCSCR.Api.Controllers;

[Route("api/CheckInCheckOut")]
[ApiController]
[Authorize]
public class CheckInCheckOutController : Controller
{
  private readonly ICheckInCheckOutService _checkInCheckOutService;
  private readonly IAuthService _authService;

  public CheckInCheckOutController(
    ICheckInCheckOutService checkInCheckOutService,
    IAuthService authService)
  {
    _checkInCheckOutService = checkInCheckOutService;
    _authService = authService;
  }

  // POST api/CheckInCheckOut/AuthenticateUser
  [HttpPost]
  [Route("AuthenticateUser")]
  public async Task<IActionResult> AuthenticateUser([FromBody] User user)
  {
    bool isAuthenticated = await _authService.ValidateUser(user);
    return Ok(isAuthenticated);
  }

  // GET api/CheckInCheckOut/GetCurrentUsers
  [HttpGet]
  [Route("GetCurrentUsers")]
  public async Task<IActionResult> GetCurrentUsers()
  {
    IEnumerable<CheckInCheckOutUserDto> users = await _checkInCheckOutService.GetCurrentUsers();
    return Ok(users);
  }

  // GET api/CheckInCheckOut/DetermineInOrOut
  [HttpGet]
  [Route("DetermineInOrOut")]
  public async Task<IActionResult> DetermineInOrOut(bool policyHOUR24STORE, string userCode)
  {
    DetermineInOrOutDto details = await _checkInCheckOutService.DetermineInOrOut(policyHOUR24STORE, userCode);
    return Ok(details);
  }

  // POST api/CheckInCheckOut/AddComment
  [HttpPost]
  [Route("AddComment")]
  public async Task<IActionResult> AddComment(AddCommentDto model)
  {
    bool added = await _checkInCheckOutService.AddComment(model);
    return Ok(added);
  }

  // POST api/CheckInCheckOut/PerformCheckInOrOut
  [HttpPost]
  [Route("PerformCheckInOrOut")]
  public async Task<IActionResult> PerformCheckInOrOut(PerformCheckInOrOutDto infos)
  {
    PerformCheckInOrOutResponseDto res = await _checkInCheckOutService.PerformCheckInOrOut(infos);
    return Ok(res);
  }
}
