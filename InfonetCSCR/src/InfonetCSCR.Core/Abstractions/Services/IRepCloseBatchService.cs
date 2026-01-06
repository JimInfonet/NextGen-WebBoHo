using System.Data;

namespace InfonetCSCR.Core.Abstractions.Services
{
    public interface IRepCloseBatchService
    {
        public Task<IEnumerable<RepCloseBatch>> GetRepCloseBatches(DateTime date1, DateTime date2);
        public Task<string?> GetReportById(string id);
        
        public Task<DataTable> GenerateReportData(string ID, string BatchNumber, string TerminalId);
        //public Task<DataTable> GenerateReportData(Policies model);
    }
}
