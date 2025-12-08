using System.Data;

namespace InfonetCSCR.Core.Abstractions.Services
{
    public interface IReportService
    {
        public Task<ReportFilter> GetReportFilter();

        //public DataTable GetDataTable(string reportName, string date1, string date2, string sdate1, string sdate2, string dept1, string dept2,
        //     string subdept1, string subdept2, string subdetail1, string subdetail2,
        //     string stock1, string stock2, string class1, string class2, string cust1, string cust2, string emp1, string emp2,
        //     string vendor1, string vendor2, string till1, string till2, string tax1, string tax2, string orderno, string qtyoh);
    }
}

