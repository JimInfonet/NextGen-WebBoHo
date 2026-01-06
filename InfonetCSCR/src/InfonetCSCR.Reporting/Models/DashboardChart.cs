using System.Collections.Generic;

namespace InfonetCSCR.Reporting.Models
{
    public class Sale
    {
        public int SaleHour { get; set; }
        public string Dept { get; set; }
        public decimal SaleAmt { get; set; }
    }

    public class SalesChartData
    {
        public string Label { get; set; }
        public List<decimal> Data { get; set; }
    }

    public class TopCatChartData
    {
        public List<string> Labels { get; set; }
        public List<decimal> Data { get; set; }
    }

    public class DashboardChart
    {
        public List<SalesChartData> SalesChartDatas { get; set; }
        public TopCatChartData TopCatChartData { get; set; }
    }
}