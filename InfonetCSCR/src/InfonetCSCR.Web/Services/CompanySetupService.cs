using InfonetCSCR.Core.Models;

namespace InfonetCSCR.Web.Services;

public class CompanySetupService : ICompanySetupService
{
    private readonly IApplicationConfig _config;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CompanySetupService> _logger;
    private readonly IHttpService _http;


    public CompanySetupService(IApplicationConfig config,
      IDistributedCache cache,
      IHttpService http,
      ILogger<CompanySetupService> logger)
    {
        _config = config;
        _cache = cache;
        _http = http;
        _logger = logger;
    }

    public async Task<Company?> GetCompanyAsync()
    {
        Company? company = null;
        string url = @$"CompanySetup/GetCompany";
        var response = await _http.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            company = JsonSerializer.Deserialize<Company?>(json);
        }
        else
        {
            _logger.LogError(Convert.ToString(response.StatusCode));
        }

        return company;
    }

    public async Task<string> GetCompanyNameAsync()
    {
        string? companyName = await _cache.GetCacheAsync<string>(CacheKeys.CompanyName);

        if (companyName == null)
        {
            await SetCompanyNameAsync();
            companyName = await _cache.GetCacheAsync<string>(CacheKeys.CompanyName);
        }

        return companyName;
    }

    private async Task<bool> SetCompanyNameAsync()
    {
        string? companyName = null;
        string url = @$"CompanySetup/GetCompanyName";
        var response = await _http.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            companyName = await response.Content.ReadAsStringAsync();
            await _cache.SetCacheAsync(CacheKeys.CompanyName, companyName);
        }
        else
        {
            _logger.LogError(Convert.ToString(response.StatusCode));
        }

        return true;
    }
}
