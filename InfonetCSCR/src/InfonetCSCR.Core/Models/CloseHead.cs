using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class CloseHead
    {
        public int Close_Num { get; set; }
        public DateTime? Close_Date { get; set; }
        public DateTime? Close_Time { get; set; }
        public int? ShiftNumber { get; set; }
        public DateTime ShiftDate { get; set; }
        public DateTime? Open_Date { get; set; }
        public DateTime? Open_Time { get; set; }
        public DateTime? Till_Date { get; set; }
        public short? Till_Num { get; set; }
        public string? User { get; set; }
        public decimal? Float { get; set; }
        public decimal? Draw { get; set; }
        public decimal? Drop { get; set; }
        public decimal? ScIssued { get; set; }
        public bool? Transferred { get; set; }
        public decimal? BonusFloat { get; set; }
        public decimal? BonusDraw { get; set; }
        public decimal? BonusDrop { get; set; }
        public decimal? BonusGiveAway { get; set; }
        public int? GroupNumber { get; set; }
        public string? GroupType { get; set; }
        public int? Dip_Number { get; set; }

    }
}
