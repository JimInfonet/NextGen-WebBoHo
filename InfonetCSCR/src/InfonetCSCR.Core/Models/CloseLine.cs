using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public partial class CloseLine
    {
        public int Sequence { get; set; }
        public string? TENDNAME { get; set; }
        public string? TENDCLASS { get; set; }
        public int? TENDCOUNT { get; set; }
        public decimal AMOUNT { get; set; }
        public decimal SYSTEM { get; set; }
        public decimal BALANCE { get; set;}
        public Decimal Exchange_Rate { get; set; }
        public decimal CONV_AMT { get; set; }
        public decimal CONV_SYS { get; set; }
        public decimal CONV_BAL { get; set; }
    }
}
