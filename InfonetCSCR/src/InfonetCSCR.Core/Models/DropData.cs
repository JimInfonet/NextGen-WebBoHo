using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class DropData
    {
        public string Tender_Name { get; set; } = null!;
        public decimal? Drop_Amt { get; set; }
        public decimal? Drop_Conv { get; set; }
    }
}
