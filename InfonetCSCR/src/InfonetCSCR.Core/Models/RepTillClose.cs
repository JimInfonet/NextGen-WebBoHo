using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class RepTillClose
    {
        public int Close_Num { get; set; }
        public String? Open_Date { get; set; }
        public String? Open_Time { get; set; }
        public String? Close_Date { get; set; }
        public String? Close_Time { get; set; }
        public int Till_Num { get; set; }
        public String? ShiftDate { get; set; }
        public int ShiftNumber { get; set; }
    }
}
