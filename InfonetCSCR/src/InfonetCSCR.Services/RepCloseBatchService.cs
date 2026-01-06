using System.Data;

namespace InfonetCSCR.Services
{
    public class RepCloseBatchService : IRepCloseBatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationConfig _config;
        private readonly IPolicyService _policy;

        public RepCloseBatchService(IApplicationConfig config,
                                    IPolicyService policy)
        {
            _config = config;
            _policy = policy;
            this._unitOfWork = new UnitOfWork(_config, _config.CSCMasterConnectionString);
        }
        
        public async Task<IEnumerable<RepCloseBatch>> GetRepCloseBatches(DateTime date1, DateTime date2)
        {
            IEnumerable<RepCloseBatch> repCloseBatches = await _unitOfWork.RepCloseBatch.GetRepCloseBatches(date1, date2);
            _unitOfWork.Commit();

            return repCloseBatches;
        }
        public async Task<string?> GetReportById(string id)
        {
            var report = await _unitOfWork.RepCloseBatch.GetReportById(id);
            _unitOfWork.Commit();

            return report;
        }
        public async Task<DataTable> GenerateReportData(string ID, string BatchNumber, string TerminalId)
        {
            var repData = await _unitOfWork.RepCloseBatch.GenerateReportData(ID, BatchNumber, TerminalId);
            _unitOfWork.Commit();

            return repData;
        }
    }
}
