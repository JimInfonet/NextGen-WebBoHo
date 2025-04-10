namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TokenController : ControllerBase
{
  private readonly IConfiguration _configuration;
  private readonly ITokenService _tokenService;

  public TokenController(
    IConfiguration configuration,
    ITokenService tokenService)
  {
    _configuration = configuration;
    _tokenService = tokenService;
  }

  [HttpPost]
  [Route("refresh")]
  public async Task<IActionResult> Refresh([FromBody] TokenModel tokenModel, string userCode)
  {
    var issuer = _configuration["Jwt:Issuer"];
    var audience = _configuration["Jwt:Audience"];
    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
    int jwtExpireTimeInMinute = Int32.Parse(_configuration["Jwt:JwtTokenLifetimeInMinutes"]);
    int refreshTokenLifetimeInDays = Int32.Parse(_configuration["Jwt:RefreshTokenLifetimeInDays"]);

    TokenModel newTokens = await _tokenService.RefreshToken(tokenModel, userCode, issuer, audience, key, jwtExpireTimeInMinute, refreshTokenLifetimeInDays);

    return Ok(newTokens);
  }

  [HttpPost, Authorize]
  [Route("revoke")]
  public async Task<IActionResult> Revoke(string userCode)
  {
    bool isTokenDeleted = await _tokenService.DeleteRefreshToken(userCode);
    return Ok(new { IsTokenDeleted = isTokenDeleted });
  }
}
