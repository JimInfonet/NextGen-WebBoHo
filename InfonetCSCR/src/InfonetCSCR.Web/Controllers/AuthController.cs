namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class AuthController : Controller
{
  private readonly IHttpService _http;
  private readonly IStringLocalizer<AuthController> _localizer;
  private readonly IApplicationConfig _config;
  private readonly IDistributedCache _cache;
  private readonly Serilog.ILogger _logger;

  public AuthController(
        IHttpService http,
        IDistributedCache cache,
        IStringLocalizer<AuthController> localizer,
        IApplicationConfig config)
  {
    _http = http;
    _localizer = localizer;
    _cache = cache;
    _config = config;
    _logger = Log.Logger;
  }

  [AllowAnonymous]
  public IActionResult Index()
  {
    if (User.Identity != null && User.Identity.IsAuthenticated)
    {
      return RedirectToAction("Index", "Home");
    }
    return View();
  }

  [HttpPost]
  [AllowAnonymous]
  public async Task<IActionResult> Login([FromBody] UserViewModel user)
  {
    string url = "auth/AuthenticateUser";
    HttpResponseMessage response = null;
    try
    {
      response = await _http.PostAsync(url, user);
    }
    catch (HttpRequestException ex)
    {
      _logger.Error(ex, ex.Message);
      ModelState.Clear();
      return Json(new { Status = "error", Message = _localizer["UserValidationError"] });
    }

    if (!response.IsSuccessStatusCode)
      return View("Index");

    var responseData = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    var userWithToken = JsonSerializer.Deserialize<UserWithToken>(responseData, options);
    TokenModel token = userWithToken!.Token;
    UserDetailsViewModel userDetails = userWithToken.UserDetails;
    
    if (token is null)
    {
      ModelState.Clear();
      return Json(new { Status = "error", Message = _localizer["UserValidationError"] });
    }

    HttpContext.Session.SetString(SessionKeys.Token, token.JwtToken ?? string.Empty);
    HttpContext.Session.SetString(SessionKeys.RefreshToken, token.RefreshToken ?? string.Empty);
    HttpContext.Session.SetString(SessionKeys.UserCode, userDetails.Code);
    HttpContext.Session.SetString(SessionKeys.UserName, userDetails.Name!);
    HttpContext.Session.SetString(SessionKeys.UserDetails, JsonSerializer.Serialize(userDetails));

    HttpContext.Response.Cookies.Append(SessionKeys.Token, token.JwtToken ?? string.Empty, new CookieOptions
    {
      Expires = DateTimeOffset.Now.AddMinutes(_config.CookieExpiryTimeInMinute),
      Secure = _config.UseHttps,
    });
    HttpContext.Response.Cookies.Append(SessionKeys.RefreshToken, token.RefreshToken ?? string.Empty, new CookieOptions
    {
      Expires = DateTimeOffset.Now.AddMinutes(_config.CookieExpiryTimeInMinute),
      Secure = _config.UseHttps,
    });
    HttpContext.Response.Cookies.Append(SessionKeys.UserCode, userDetails.Code, new CookieOptions
    {
      Expires = DateTimeOffset.Now.AddMinutes(_config.CookieExpiryTimeInMinute),
      Secure = _config.UseHttps,
    });
    HttpContext.Response.Cookies.Append(SessionKeys.UserName, userDetails.Name!, new CookieOptions
    {
      Expires = DateTimeOffset.Now.AddMinutes(_config.CookieExpiryTimeInMinute),
      Secure = _config.UseHttps,
    });
    HttpContext.Response.Cookies.Append(SessionKeys.UserDetails, JsonSerializer.Serialize(userDetails), new CookieOptions
    {
      Expires = DateTimeOffset.Now.AddMinutes(_config.CookieExpiryTimeInMinute),
      Secure = _config.UseHttps,
    });

    var claims = new List<Claim>
    {
      new(ClaimTypes.Name, user.Code)
    };
    var identity = new ClaimsIdentity(
      claims,
      CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await HttpContext.SignInAsync(principal);

    return Json(new { Status = "success", Message = "Login Successful." });
  }

  [HttpPost]
  public async Task<IActionResult> Logout()
  {
    HttpContext.Session.Remove(SessionKeys.Token);
    HttpContext.Session.Remove(SessionKeys.RefreshToken);
    HttpContext.Session.Remove(SessionKeys.UserCode);
    HttpContext.Session.Remove(SessionKeys.UserName);
    HttpContext.Session.Remove(SessionKeys.UserDetails);

    HttpContext.Response.Cookies.Delete(SessionKeys.Token);
    HttpContext.Response.Cookies.Delete(SessionKeys.RefreshToken);
    HttpContext.Response.Cookies.Delete(SessionKeys.UserCode);
    HttpContext.Response.Cookies.Delete(SessionKeys.UserName);
    HttpContext.Response.Cookies.Delete(SessionKeys.UserDetails);

    await _cache.RemoveCacheAsync(CacheKeys.CompanyList);
    await _cache.RemoveCacheAsync(CacheKeys.ClassList);
    await _cache.RemoveCacheAsync(CacheKeys.CanbeList);
    await _cache.RemoveCacheAsync(CacheKeys.SettingsList);
    await _cache.RemoveCacheAsync(CacheKeys.SideBarItemsPolicies);

    await HttpContext.SignOutAsync();

    return RedirectToAction("Index");
  }

  public async Task<IActionResult> GetUserDetails(string userCode)
  {
    string url = $"Auth/GetUserDetails?userCode={userCode}";
    var userDetails = await _http.GetAndParseAsync<UserDetailsViewModel>(url);
    return Ok(userDetails);
  }

  public async Task<IActionResult> GetUserNameByCode(string code)
  {
    string url = $"auth/GetUserNameByCode?code={code}";
    var userName = await _http.GetAndParseAsync<string>(url);
    return Ok(userName);
  }

  [AllowAnonymous]
  public bool IsAuthenticated()
  {
    return User.Identity != null && User.Identity.IsAuthenticated;
  }

  public async Task<IActionResult> GetUserId(string userCode)
  {
    string url = "Auth/GetUserId";
    int userId = await _http.PostAndParseAsync<int>(url, userCode);
    return Ok(userId);
  }
}
