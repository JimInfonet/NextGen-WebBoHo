using InfonetCSCR.Core.Models;

namespace InfonetCSCR.Web.Services
{
    public class ReportService : IReportService
    { 
        private readonly IHttpService _http;
        private readonly ILogger<ReportService> _logger;
        private readonly IApplicationConfig _config;
        private readonly IPolicyService _policy;

        public ReportService(IHttpService http, ILogger<ReportService> logger, IApplicationConfig applicationConfig, IPolicyService policy)
        {
            _http = http;
            _logger = logger;
            _config = applicationConfig;
            _policy = policy;
        }
        public async Task<ReportFilter> GetReportFilter()
        {
            string url = $"Report/GetReportFilter";
            ReportFilter? reportFilter = null;
            var response = await _http.GetAsync(url);

            if (response.ReasonPhrase == "OK")
            {
                reportFilter = await response.Content.ReadFromJsonAsync<ReportFilter>();
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));
            }

            return reportFilter;
        }
    }
}
