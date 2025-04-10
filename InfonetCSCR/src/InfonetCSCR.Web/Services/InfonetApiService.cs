namespace InfonetCSCR.Web.Services;

public class InfonetApiService : IApiService
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly IHttpContextAccessor _contextAccessor;

  public InfonetApiService(IHttpClientFactory httpClientFactory,
    IHttpContextAccessor contextAccessor)
  {
    _httpClientFactory = httpClientFactory;
    _contextAccessor = contextAccessor;
  }

  public HttpClient GetApiClient()
  {
    var httpClient = _httpClientFactory.CreateClient("InfonetApi");
    var token = _contextAccessor.HttpContext?.Session?.GetString("Token");

    if (!string.IsNullOrEmpty(token))
    {
      httpClient.DefaultRequestHeaders.Authorization
          = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.ToString());
    }
    return httpClient;
  }
}
