using InfonetCSCR.Core.Dtos;

namespace InfonetCSCR.Web.Abstractions.Services
{
    public interface IRepArStatService
    {
        CustomerPoliciesViewModel GetPolicies();
        public Task<IEnumerable<CustomersDto>> GetAllCustomers();
        public Task<ArStatementDto> GetRepArStat(string client, DateTime endDate);
        public Task<bool> GenMonthlyClosing(string client, DateTime endDate);

        //public Task<IEnumerable<int>> GetShiftNos(DateTime shiftdate);
        //public Task<String> GetRepByDates(GetRepArStatDto model);
        //public byte[] GetExportToExcel();
        //public IActionResult GetPrintReport(GetRepArStatDto model);
        //public Policies GetPolicies();
    }
}
