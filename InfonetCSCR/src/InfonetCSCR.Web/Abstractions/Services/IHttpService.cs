namespace InfonetCSCR.Web.Abstractions.Services;

public interface IHttpService
{
  Task<HttpResponseMessage> GetAsync(string url);
  Task<T?> GetAndParseAsync<T>(string url);
  Task<HttpResponseMessage> PostAsync(string url, object? model = null);
  Task<T?> PostAndParseAsync<T>(string url, object? model = null);
  Task<HttpResponseMessage> DeleteAsync(string url);
  Task<T?> DeleteAndParseAsync<T>(string url);
  Task<bool> RefreshToken();
}
