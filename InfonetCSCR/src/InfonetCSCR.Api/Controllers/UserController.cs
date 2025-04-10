namespace InfonetCSCR.Api.Controllers;

[Route("api/User")]
[ApiController]
[Authorize]

public class UserController : ControllerBase
{
  public readonly IUserService _userService;

  public UserController(IUserService userService)
  {
    _userService = userService;
  }

  // GET api/User/GetAllUserDetails
  [HttpGet]
  [Route("GetAllUserDetails")]
  public async Task<IActionResult> GetAllUserDetails(bool timeSheetsAndHaveUserIdPolicy)
  {
    IEnumerable<UserDetails> userDetails = await _userService.GetAllUserDetails(timeSheetsAndHaveUserIdPolicy);
    return Ok(userDetails);
  }

  // GET api/User/GetUserDetails
  [HttpGet]
  [Route("GetUserDetails")]
  public async Task<IActionResult> GetUserDetails(string code)
  {
    UserDetails userDetails = await _userService.GetUserDetails(code, false);
    return Ok(userDetails);
  }

  // POST api/User/AddUserDetails
  [HttpPost]
  [Route("AddUserDetails")]
  public async Task<IActionResult> AddUserDetails([FromBody] UserDetails userDetails, bool timeSheetsAndHaveUserIdPolicy)
  {
    bool isAdded = await _userService.AddUserDetails(userDetails, timeSheetsAndHaveUserIdPolicy);
    return Ok(isAdded);
  }

  // POST api/User/UpdateUserDetails
  [HttpPost]
  [Route("UpdateUserDetails")]
  public async Task<IActionResult> UpdateUserDetails([FromBody] UserDetails userDetails)
  {
    bool isUpdated = await _userService.UpdateUserDetails(userDetails);
    return Ok(isUpdated);
  }

  // POST api/User/DeleteUserDetails
  [HttpPost]
  [Route("DeleteUserDetails")]
  public async Task<IActionResult> DeleteUserDetails([FromBody] string groupID)
  {
    bool isDeleted = await _userService.DeleteUserDetails(groupID);
    return Ok(isDeleted);
  }

  // GET api/User/GetInitialEmployeeId
  [HttpGet]
  [Route("GetInitialEmployeeId")]
  public async Task<IActionResult> GetInitialEmployeeId()
  {
    int initialEmployeeId = await _userService.GetInitialEmployeeId();
    return Ok(initialEmployeeId);
  }

  // GET api/User/GetGroupInfo
  [HttpGet]
  [Route("GetGroupInfo")]
  public async Task<IActionResult> GetGroupInfo(string userCode)
  {
    var groupInfo = await _userService.GetGroupInfo(userCode);
    return Ok(groupInfo);
  }

  // GET api/User/GetGroupCodesBySecurityLevel
  [HttpGet]
  [Route("GetGroupCodesBySecurityLevel")]
  public async Task<IActionResult> GetGroupCodesBySecurityLevel(int securityLevel)
  {
    var groupCodes = await _userService.GetGroupCodesBySecurityLevel(securityLevel);
    return Ok(groupCodes);
  }

  // POST api/User/AddUIGRecord
  [HttpPost]
  [Route("AddUIGRecord")]
  public async Task<IActionResult> AddUIGRecord([FromBody] Uig uig)
  {
    bool isAdded = await _userService.AddUIGRecord(uig);
    return Ok(isAdded);
  }

  // POST api/User/DeleteUIGRecord
  [HttpPost]
  [Route("DeleteUIGRecord")]
  public async Task<IActionResult> DeleteUIGRecord([FromBody] Uig uig, int securityLevelForLoggedInUser)
  {
    bool isDeleted = await _userService.DeleteUIGRecord(uig, securityLevelForLoggedInUser);
    return Ok(isDeleted);
  }

  // POST api/User/GetUserCodes
  [HttpPost]
  [Route("GetUserCodes")]
  public async Task<IActionResult> GetUserCodes([FromBody] UserDetails userDetails)
  {
    var userCodes = await _userService.GetUserCodes(userDetails);
    return Ok(userCodes);
  }

  // POST api/User/ChangeUserPassword
  [HttpPost]
  [Route("ChangeUserPassword")]
  public async Task<IActionResult> ChangeUserPassword([FromBody] UserDetails userDetails)
  {
    await _userService.ChangeUserPassword(userDetails);
    return Ok(null);
  }

  // GET api/User/GetMaxUserID
  [HttpGet]
  [Route("GetMaxUserID")]
  public async Task<IActionResult> GetMaxUserID()
  {
    return Ok(await _userService.GetMaxUserIDAsync());
  }
}
