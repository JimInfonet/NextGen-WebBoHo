namespace InfonetCSCR.Web.Services;

public class ApplicationPolicyService : IApplicationPolicyService
{
  private readonly ILogger<ApplicationPolicyService> _logger;
  private readonly IDistributedCache _cache;
  private readonly IHttpService _http;

  public ApplicationPolicyService(IHttpService http, IDistributedCache cache, ILogger<ApplicationPolicyService> logger)
  {
    _http = http;
    _cache = cache;
    _logger = logger;
  }

  public async Task<List<PolicyCompanyViewModel>> GetCompany()
  {
    var data = await _cache.GetCacheAsync<List<PolicyCompanyViewModel>>(CacheKeys.CompanyList);
    if (data != null && data.Any())
    {
      return data;
    }

    string url = "Policy/PolicyAllCompany";
    var response = await _http.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      data = await response.Content.ReadFromJsonAsync<List<PolicyCompanyViewModel>>();
    }
    else
    {
      _logger.LogError(Convert.ToString(response.StatusCode));
    }

    if (data != null && data.Any())
    {
      await _cache.SetCacheAsync(CacheKeys.CompanyList, data);
    }

    return data;
  }

  public async Task<List<PolicySettingsViewModel>> GetSettings()
  {
    var data = await _cache.GetCacheAsync<List<PolicySettingsViewModel>>(CacheKeys.SettingsList);
    if (data != null && data.Any())
    {
      return data;
    }

    string url = "Policy/PolicyAllSettings";
    var response = await _http.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      data = await response.Content.ReadFromJsonAsync<List<PolicySettingsViewModel>>();
    }
    else
    {
      _logger.LogError(Convert.ToString(response.StatusCode));
    }

    if (data != null && data.Any())
    {
      await _cache.SetCacheAsync(CacheKeys.SettingsList, data);
    }
    return data;
  }

  public async Task<List<PolicyClassViewModel>> GetClass()
  {
    var data = await _cache.GetCacheAsync<List<PolicyClassViewModel>>(CacheKeys.ClassList);
    if (data != null && data.Any())
    {
      return data;
    }

    string url = "Policy/PolicyAllClass";
    var response = await _http.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      data = await response.Content.ReadFromJsonAsync<List<PolicyClassViewModel>>();
    }
    else
    {
      _logger.LogError(Convert.ToString(response.StatusCode));
    }

    if (data != null && data.Any())
    {
      await _cache.SetCacheAsync(CacheKeys.ClassList, data);
    }

    return data;
  }

  public async Task<List<PolicyCanbeViewModel>> GetCanbe()
  {
    var data = await _cache.GetCacheAsync<List<PolicyCanbeViewModel>>(CacheKeys.CanbeList);
    if (data != null && data.Any())
    {
      return data;
    }

    string url = "Policy/PolicyAllCanbe";
    var response = await _http.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      data = await response.Content.ReadFromJsonAsync<List<PolicyCanbeViewModel>>();
    }
    else
    {
      _logger.LogError(Convert.ToString(response.StatusCode));
    }

    if (data != null && data.Any())
    {
      await _cache.SetCacheAsync(CacheKeys.CanbeList, data);
    }
    return data;
  }
}
