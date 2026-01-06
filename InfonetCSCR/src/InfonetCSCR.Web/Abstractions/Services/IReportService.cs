using InfonetCSCR.Core.Models;

namespace InfonetCSCR.Web.Abstractions.Services
{
    public interface IReportService
    {
        public Task<ReportFilter> GetReportFilter();
    }
}
