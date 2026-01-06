using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Dtos
{
    public class Policies
    {
        public string? BaseCurr { get; set; }
        public string? CBonusName { get; set; }
        public string? CBonusTend { get; set; }
        public bool CashBonus { get; set; }
        public string? CcMode { get; set; }
        public bool CombineCr { get; set; }
        public bool CombineFleet { get; set; }
        public bool CountCash { get; set; }
        public string? CountType { get; set; }
        public int CloseBDays { get; set; }
        public bool CloseIncluded { get; set; }
        public bool EodGroup { get; set; }
        public bool PrtTillClCol { get; set; }
        public bool ShowAllExempt { get; set; }
        public bool SupportEzi { get; set; }
        public bool Show_NRGT { get; set; }
        public bool ShowCodes { get; set; }
        public bool TaxRebate { get; set; }
        public bool TaxExempt { get; set; }
        public string? Te_Type { get; set; }
        public bool TidelSafe { get; set; }
        public int TillCDays { get; set; }
        public bool UseOvrPymnt { get; set; }
        public bool UseShifts { get; set; }
        //public string? BatchNumber { get; set; }
        //public string? TerminalId { get; set; }
    }
}
