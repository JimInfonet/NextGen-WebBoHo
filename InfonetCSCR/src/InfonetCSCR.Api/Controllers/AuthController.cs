namespace InfonetCSCR.Api.Controllers;

[Route("api/auth")]
[ApiController]
[Authorize]

public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;
  private readonly IUserService _userService;
  private readonly IConfiguration _configuration;

  public AuthController(IAuthService authService,
    IUserService userService,
    IConfiguration configuration)
  {
    _authService = authService;
    _userService = userService;
    _configuration = configuration;
  }

  // POST api/Auth/AuthenticateUser
  [HttpPost]
  [Route("AuthenticateUser")]
  [AllowAnonymous]
  public async Task<IActionResult> AuthenticateUser([FromBody] User user)
  {
    bool loggedIn = await _authService.ValidateUser(user);

    if (loggedIn)
    {
      UserDetails userDetails = await _userService.GetUserDetails(user.Code);
      userDetails.Password = null;

      var issuer = _configuration["Jwt:Issuer"];
      var audience = _configuration["Jwt:Audience"];
      var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
      int jwtExpireTimeInMinutes = Int32.Parse(_configuration["Jwt:JwtTokenLifetimeInMinutes"]);
      string jwt = AuthenticationHelper.CreateToken(user.Code, issuer, audience, key, jwtExpireTimeInMinutes);
      string refreshToken = AuthenticationHelper.GenerateRefreshToken();

      RefreshToken token = new RefreshToken()
      {
        Token = refreshToken,
        ExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenLifetimeInDays"])),
        UserCode = user.Code,
      };

      bool isTokenStored = await _authService.StoreRefreshToken(token);

      if (!isTokenStored) throw new Exception("Failed to store refresh token");

      return Ok(new
      {
        UserDetails = userDetails,
        Token = new
        {
          JwtToken = jwt,
          RefreshToken = refreshToken
        }
      });
    }

    return Unauthorized();
  }

  // GET api/Auth/GetUserDetails
  [HttpGet]
  [Route("GetUserDetails")]
  public async Task<IActionResult> GetUserDetails(string userCode)
  {
    UserDetails userDetails = await _userService.GetUserDetails(userCode);
    userDetails.Password = null;
    return Ok(userDetails);
  }

  // GET api/Auth/GetUserNameByCode
  [HttpGet]
  [Route("GetUserNameByCode")]
  public async Task<IActionResult> GetUserNameByCode(string code)
  {
    string userName = await _authService.GetUserNameByCode(code);
    return Ok(userName);
  }

  // POST api/Auth/GetUserId
  [HttpPost]
  [Route("GetUserId")]
  public async Task<IActionResult> GetUserId([FromBody] string userCode)
  {
    int userId = await _userService.GetUserId(userCode);
    return Ok(userId);
  }

  // POST api/Auth/IsValidUser
  [HttpPost]
  [Route("IsValidUser")]
  public async Task<IActionResult> IsValidUser([FromBody] string userCode)
  {
    var user = await _userService.GetUserByCode(userCode);
    return Ok(user != null);
  }
}
