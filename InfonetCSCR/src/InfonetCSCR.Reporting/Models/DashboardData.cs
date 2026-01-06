using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InfonetCSCR.Reporting.Models
{
    public class DashboardData
    {
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal SaleAmt { get; set; }
        public int Transactions { get; set; }
    }
}