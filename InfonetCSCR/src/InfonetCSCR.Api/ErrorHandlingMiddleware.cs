namespace InfonetCSCR.Api.MiddleWare;

public class ErrorHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly Serilog.ILogger _logger;

  public ErrorHandlingMiddleware(RequestDelegate next)
  {
    _next = next;
    _logger = Log.Logger;
  }

  public async Task Invoke(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.Error(ex, ex.Message);
      await HandleExceptionAsync(context, ex);
    }
  }

  private static Task HandleExceptionAsync(HttpContext context, Exception ex)
  {
    var statusCode = HttpStatusCode.InternalServerError;
    var result = JsonSerializer.Serialize(new { error = ex.Message });
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)statusCode;
    return context.Response.WriteAsync(result);
  }
}
