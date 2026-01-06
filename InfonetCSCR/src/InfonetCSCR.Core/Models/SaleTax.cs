namespace InfonetCSCR.Core.Models
{
    public class SaleTax
    {
        public string? Tax_Name { get; set; }
        public decimal Taxes { get; set; }
        public decimal Included { get; set; }
        public decimal Rebate { get; set; }
    }
}
