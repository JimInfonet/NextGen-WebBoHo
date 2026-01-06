using InfonetCSCR.Core.Dtos;
using InfonetCSCR.Web.Models;
using System.Data;

namespace InfonetCSCR.Web.Abstractions.Services
{
    public interface IRepCloseBatchService
    {
        public RepCloseBatchPoliciesViewModel GetPolicies();
        public Task<IEnumerable<RepCloseBatchViewModel>> GetRepCloseBatches(DateTime date1, DateTime date2);
        public Task<string?> GetReportById(string id);
        public Task<DataTable> GenerateReportData(string BatchID, string BatchNumber, string TerminalId);
        public byte[] GetExportToExcel();
    }
}
