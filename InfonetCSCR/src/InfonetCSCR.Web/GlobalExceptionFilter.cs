namespace InfonetCSCR.Web;

public class GlobalExceptionFilter : IExceptionFilter
{
  private readonly Serilog.ILogger _logger;

  public GlobalExceptionFilter()
  {
    _logger = Log.Logger;
  }

  public void OnException(ExceptionContext context)
  {
    _logger.Error(context.Exception.Message);
    ObjectResult result = new(context.Exception.Message) { StatusCode = (int)HttpStatusCode.InternalServerError };

    if (context.Exception is HttpRequestException ex)
    {
      result = new ObjectResult(ex.Message)
      {
        StatusCode = (int?)ex.StatusCode ?? (int?)HttpStatusCode.InternalServerError
      };
    }

    context.Result = result;
    context.ExceptionHandled = true;
  }
}
