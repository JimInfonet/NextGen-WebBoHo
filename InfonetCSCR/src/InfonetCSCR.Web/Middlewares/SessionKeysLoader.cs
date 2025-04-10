namespace InfonetCSCR.Web.Middlewares;

public class SessionKeysLoader
{
  private readonly RequestDelegate _next;

  public SessionKeysLoader(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
    {
      if (!context.Session.Keys.Any())
      {
        context.Session.SetString(SessionKeys.Token, context.Request.Cookies[SessionKeys.Token] ?? string.Empty);
        context.Session.SetString(SessionKeys.RefreshToken, context.Request.Cookies[SessionKeys.RefreshToken] ?? string.Empty);
        context.Session.SetString(SessionKeys.UserCode, context.Request.Cookies[SessionKeys.UserCode] ?? string.Empty);
        context.Session.SetString(SessionKeys.UserName, context.Request.Cookies[SessionKeys.UserName] ?? string.Empty);
        context.Session.SetString(SessionKeys.UserDetails, context.Request.Cookies[SessionKeys.UserDetails] ?? string.Empty);
      }

      string[] keys = {SessionKeys.Token, SessionKeys.RefreshToken
        , SessionKeys.UserCode, SessionKeys.UserName, SessionKeys.UserDetails };

      if (IsAnySessionValueMissing(context, keys) || IsAnyCookieValueMissing(context, keys))
      {
        await context.SignOutAsync();
        context.Response.Redirect("/Auth");
        return;
      }
    }

    await _next(context);
  }

  private static bool IsAnySessionValueMissing(HttpContext context, string[] keys)
  {
    return keys.Any(key => string.IsNullOrEmpty(context.Session.GetString(key)));
  }

  private static bool IsAnyCookieValueMissing(HttpContext context, string[] keys)
  {
    return keys.Any(key => string.IsNullOrEmpty(context.Request.Cookies[key]));
  }
}

public static class SessionKeysLoaderExtension
{
  public static IApplicationBuilder useSessionKeysLoader(
      this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<SessionKeysLoader>();
  }
}
