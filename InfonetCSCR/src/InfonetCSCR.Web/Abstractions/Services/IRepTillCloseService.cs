using InfonetCSCR.Core.Dtos;

namespace InfonetCSCR.Web.Abstractions.Services
{
    public interface IRepTillCloseService
    {
        //public Task<IEnumerable<RepTillCloseViewModel>> GetRepTillCloses();
        public Task<IEnumerable<RepTillCloseViewModel>> GetRepTillCloses(DateTime date1, DateTime date2);
        public Task<String> GetRepByShift(GetRepTillCloseDto model);
        public Task<IEnumerable<int>> GetShiftNos(DateTime shiftdate);
        public Task<String> GetRepByDates(GetRepTillCloseDto model);
        public Task<String> GetRepByTillNo(GetRepTillCloseDto model);
        public byte[] GetExportToExcel();
        public IActionResult GetPrintReport(GetRepTillCloseDto model);
        public Policies GetPolicies();
    }
}
