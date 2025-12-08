using InfonetCSCR.Core.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Abstractions.Repositories
{
    public interface IRepCloseBatchRepository
    {
        //public Task<IEnumerable<RepCloseBatch>> GetRepBatchCloses();
        public Task<IEnumerable<RepCloseBatch>> GetRepCloseBatches(DateTime date1, DateTime date2);
        public Task<string?> GetReportById(string id);
        public Task<DataTable> GenerateReportData(string SaleNo, string CloseBatch, string TerminalID);
    }
}
