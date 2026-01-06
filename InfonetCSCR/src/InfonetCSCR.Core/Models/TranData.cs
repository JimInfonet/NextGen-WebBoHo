using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class TranData
    {
        public string TTYPE { get; set; } = null!;
        public int Transactions { get; set; }
        public decimal Amount { get; set; }
        public decimal Pays { get; set; }
    }
}
