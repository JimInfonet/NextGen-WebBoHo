using System.Globalization;
using System.Reflection;
using System.Text;
using System.Transactions;

namespace InfonetCSCR.Core.Models
{
    public class Till_Close
    {
        //public IEnumerable<Close_Line>? CloseLines { get; set; }
        private IEnumerable<Close_Line>? _closeLines;
        public IEnumerable<Close_Line>? CloseLines
        {
            get { return _closeLines; }
            set 
            {
                if (value != null)
                { 
                    foreach( var cl in value)
                    {
                        cl.Exchange_Rate = 1;
                    }

                    _closeLines = value;
                }
                else
                {
                    // Handle invalid input or throw an exception
                    throw new ArgumentException("Object can't be null");
                }
            } 
        }

        public long Close_Num { get; set; }
        public DateOnly? Close_Date { get; set; }
        public TimeOnly? Close_Time { get; set; }
        public DateOnly? Open_Date { get; set; }
        public TimeOnly? Open_Time { get; set; }
        public DateOnly? ShiftDate { get; set; }
        public string? User { get; set; }
        public string? UserName { get; set; }
        public short? Till_Number { get; set; }
        public int? ShiftNumber { get; set; }
        public decimal Float { get; set; }
        public decimal OverPay { get; set; }
        public decimal Draw { get; set; }
        public decimal Drop { get; set; }
        public decimal Payments { get; set; }
        public decimal ARPay { get; set; }
        public decimal Credits_Issued { get; set; }
        public bool AllShifts { get; set; }
        public bool Complete { get; set; }
        public decimal BonusFloat { get; set; }
        public decimal BonusDraw { get; set; }
        public decimal BonusDrop { get; set; }
        public decimal BonusGiveAway { get; set; }
        public int GroupNumber { get; set; }
        public string? GroupType { get; set; }
        public decimal PayOuts { get; set; }
        public decimal Penny_Adj { get; set; }
    }
}
