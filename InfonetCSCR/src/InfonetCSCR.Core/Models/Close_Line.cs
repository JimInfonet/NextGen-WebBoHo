using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class Close_Line
    {
        public int Sequence { get; set; }
        public string? Tender_Name { get; set; }
        public string? Short_Name { get; set; }
        public string? Tender_Class { get; set; }
        public int? Tender_Count { get; set; }
        public decimal Entered { get; set; }
        public decimal System { get; set; }
        public decimal Balance { get; set;}
        public Decimal Exchange_Rate { get; set; }
        public decimal Converted_Entered { get; set; }
        public decimal Converted_System { get; set; }
        public decimal Converted_Balance { get; set; }
    }
}
