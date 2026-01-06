using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class RepCloseBatch
    {
        public string? ID { get; set; }
        public string? BatchNumber { get; set; }
        public string? TerminalID { get; set; }
        public string? BatchDate { get; set; }
        public string? BatchTime { get; set; }
        //public string? Report { get; set; }
    }
}
