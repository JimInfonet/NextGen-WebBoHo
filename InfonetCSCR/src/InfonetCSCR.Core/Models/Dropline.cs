using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class DropLine
    {
        public byte Till_Num { get; set; }
        public DateTime DropDate { get; set; }
        public string Tender_Name { get; set; } = null!;
        public float? Exchange_Rate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Conv_Amount { get; set; }
        public int? Close_Num { get; set; }
        public int DropId { get; set; }
    }
}
