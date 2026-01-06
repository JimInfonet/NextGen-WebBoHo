using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class FuelSale
    {
        public string? Till { get; set; }
        public int? Transactions { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Pays { get; set; }
    }
}
