using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class TeSaleList
    {
        public string? Stock_Code { get; set; }
        public string? Descript { get; set; }
        public string? Dept { get; set; }
        public string? Dept_Name { get; set; }
        public decimal Volume { get; set; }
        public decimal Sales { get; set; }
        public decimal Discount { get; set; }
    }
}
