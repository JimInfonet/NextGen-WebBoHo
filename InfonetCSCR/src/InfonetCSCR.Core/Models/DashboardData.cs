using System.ComponentModel.DataAnnotations;

namespace InfonetCSCR.Core.Models
{
    public class DashboardData
    {
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal SaleAmt { get; set; }
        public int Transactions { get; set; }
    }
}