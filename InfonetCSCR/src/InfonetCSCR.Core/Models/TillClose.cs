using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class TillClose
    {
        public string StoreID { get; set; }
        public long Sequence { get; set; }
        public string? Tender { get; set; }
        public long Count { get; set; }
        public decimal? Entered { get; set; }
        public decimal? System { get; set; }
        public decimal? Difference { get; set; }
    }
}
