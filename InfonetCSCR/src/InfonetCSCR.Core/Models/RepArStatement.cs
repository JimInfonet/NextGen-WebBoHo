using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class RepArStatement
    {
        public int Close_Num { get; set; }
        public DateTime Open_Date { get; set; }
        public DateTime Open_Time { get; set; }
        public DateTime Close_Date { get; set; }
        public DateTime Close_Time { get; set; }
        public int Till_Num { get; set; }
        public DateTime ShiftDate { get; set; }
        public int ShiftNumber { get; set; }
    }
}
