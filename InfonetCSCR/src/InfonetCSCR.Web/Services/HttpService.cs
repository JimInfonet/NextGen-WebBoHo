namespace InfonetCSCR.Web.Services;

public class HttpService : IHttpService
{
  private readonly IApiService _apiService;
  private readonly IHttpContextAccessor _contextAccessor;
  private readonly IApplicationConfig _config;

  public HttpService(IApiService apiClient, IHttpContextAccessor contextAccessor, IApplicationConfig config)
  {
    _apiService = apiClient;
    _contextAccessor = contextAccessor;
    _config = config;
  }

  public async Task<HttpResponseMessage> GetAsync(string url)
  {
    var httpClient = _apiService.GetApiClient();
    var response = await httpClient.GetAsync(url);

    if (response.StatusCode == HttpStatusCode.Unauthorized)
    {
      bool isRefreshed = await RefreshToken();
      if (isRefreshed)
      {
        httpClient = _apiService.GetApiClient();
        response = await httpClient.GetAsync(url);
      }
    }
    if (response.StatusCode == HttpStatusCode.Unauthorized) Logout();

    if (response.IsSuccessStatusCode) return response;

    throw new HttpRequestException($"Request failed GET {url} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
  }

  public async Task<T?> GetAndParseAsync<T>(string url)
  {
    var response = await GetAsync(url);
    T? data;

    if (response.IsSuccessStatusCode)
    {
      if (response.StatusCode == HttpStatusCode.NoContent) return default;

      if (response.Content.Headers.ContentType?.MediaType == "text/plain")
      {
        var text = await response.Content.ReadAsStringAsync();
        data = (T?)(object)text;
      }
      else
      {
        data = await response.Content.ReadFromJsonAsync<T>();
      }

      return data;
    }

    throw new HttpRequestException($"Request failed GET {url} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
  }

  public async Task<HttpResponseMessage> PostAsync(string url, object? model = null)
  {
    var httpClient = _apiService.GetApiClient();

    var payload = CreateContent(model);
    var response = await httpClient.PostAsync(url, payload);

    if (response.StatusCode == HttpStatusCode.Unauthorized)
    {
      bool isRefreshed = await RefreshToken();
      if (isRefreshed)
      {
        httpClient = _apiService.GetApiClient();
        response = await httpClient.PostAsync(url, payload);
      }
    }
    if (response.StatusCode == HttpStatusCode.Unauthorized) Logout();

    if (response.IsSuccessStatusCode) return response;

    throw new HttpRequestException($"Request failed POST {url} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
  }

  public async Task<T?> PostAndParseAsync<T>(string url, object? model = null)
  {
    var response = await PostAsync(url, model);
    if (response.IsSuccessStatusCode)
    {
      if (response.StatusCode == HttpStatusCode.NoContent) return default;

      T? Data = await response.Content.ReadFromJsonAsync<T>();
      return Data;
    }

    throw new HttpRequestException($"Request failed POST {url} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
  }

  public async Task<HttpResponseMessage> DeleteAsync(string url)
  {
    var httpClient = _apiService.GetApiClient();
    var response = await httpClient.DeleteAsync(url);

    if (response.StatusCode == HttpStatusCode.Unauthorized)
    {
      bool isRefreshed = await RefreshToken();
      if (isRefreshed)
      {
        httpClient = _apiService.GetApiClient();
        response = await httpClient.DeleteAsync(url);
      }
    }
    if (response.StatusCode == HttpStatusCode.Unauthorized) Logout();

    if (response.IsSuccessStatusCode) return response;

    throw new HttpRequestException($"Request failed DELETE {url} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
  }

  public async Task<T?> DeleteAndParseAsync<T>(string url)
  {
    var response = await DeleteAsync(url);
    if (response.IsSuccessStatusCode)
    {
      if (response.StatusCode == HttpStatusCode.NoContent) return default;

      T? Data = await response.Content.ReadFromJsonAsync<T>();
      return Data;
    }

    throw new HttpRequestException($"Request failed DELETE {url} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
  }

  private HttpContent CreateContent(object? model)
  {
    var content = JsonSerializer.SerializeToUtf8Bytes(model);
    var byteContent = new ByteArrayContent(content);
    byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
    return byteContent;
  }

  public async Task<bool> RefreshToken()
  {
    TokenModel token = new()
    {
      JwtToken = _contextAccessor.HttpContext?.Request.Cookies[SessionKeys.Token],
      RefreshToken = _contextAccessor.HttpContext?.Request.Cookies[SessionKeys.RefreshToken]
    };
    string tokenString = JsonSerializer.Serialize(token);
    var tokenJson = new StringContent(tokenString, Encoding.UTF8, "application/json");

    string? userCode = _contextAccessor.HttpContext?.Session.GetString(SessionKeys.UserCode);
    string url = $"Token/refresh?userCode={userCode}";

    var apiClient = _apiService.GetApiClient();
    var response = await apiClient.PostAsync(url, tokenJson);

    if (response.IsSuccessStatusCode)
    {
      if (response.StatusCode == HttpStatusCode.Unauthorized) return false;

      var responseData = await response.Content.ReadAsStringAsync();
      var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
      token = JsonSerializer.Deserialize<TokenModel>(responseData, options);
      if (token is null) return false;
     
      _contextAccessor.HttpContext?.Session.SetString(SessionKeys.Token, token.JwtToken!);
      _contextAccessor.HttpContext?.Session.SetString(SessionKeys.RefreshToken, token.RefreshToken!);

      SetCookieValue(SessionKeys.Token, token.JwtToken!);
      SetCookieValue(SessionKeys.RefreshToken, token.RefreshToken!);
      SetCookieValue(SessionKeys.UserCode, _contextAccessor.HttpContext?.Request.Cookies[SessionKeys.UserCode]);
      SetCookieValue(SessionKeys.UserName, _contextAccessor.HttpContext?.Request.Cookies[SessionKeys.UserName]);
      SetCookieValue(SessionKeys.UserDetails, _contextAccessor.HttpContext?.Request.Cookies[SessionKeys.UserDetails]);

      return true;
    }

    return false;
  }

  public void Logout()
  {
    _contextAccessor.HttpContext?.Session.Remove("Token");
    _contextAccessor.HttpContext?.Session.Remove("RefreshToken");
    _contextAccessor.HttpContext?.Session.Remove("UserCode");
    _contextAccessor.HttpContext?.Session.Remove("UserName");
    _contextAccessor.HttpContext?.Session.Remove("UserDetails");

    _contextAccessor.HttpContext?.Response.Cookies.Delete("Token");
    _contextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
    _contextAccessor.HttpContext?.Response.Cookies.Delete("UserCode");
    _contextAccessor.HttpContext?.Response.Cookies.Delete("UserName");
    _contextAccessor.HttpContext?.Response.Cookies.Delete("UserDetails");
    _contextAccessor.HttpContext?.SignOutAsync();
  }

  private void SetCookieValue(string key, string? value)
  {
    if (value == null) return;

    _contextAccessor.HttpContext?.Response.Cookies.Delete(key);
    _contextAccessor.HttpContext?.Response.Cookies.Append(key, value, new CookieOptions
    {
      Expires = DateTimeOffset.Now.AddMinutes(_config.CookieExpiryTimeInMinute),
      Secure = _config.UseHttps,
    });
  }
}
