
namespace InfonetCSCR.Reporting.Models
{
  public class ElectronicJournalDetailsReport
  {
    public int SaleNo { get; set; } = default!;
    public string ShiftDate { get; set; }
    public string SaleDate { get; set; }
    public string SaleTime { get; set; }
    public int Till { get; set; }
    public string Type { get; set; }
    public double Qty { get; set; }
    public string Customer { get; set; }
    public string Cashier { get; set; }
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public double Disc { get; set; }
    public double Price { get; set; }
    public double Amount { get; set; }
    public int? PumpID { get; set; }
    public int? DelSeq { get; set; }
    public double? InvoiceDiscount { get; set; }
    public string? TaxName { get; set; }
    public string? TaxCode { get; set; }
    public double? TaxAddedAmount { get; set; }
    public double? TaxIncludedAmount { get; set; }
    public double? AssocAmt { get; set; }
    public double? TendAmt { get; set; }
    public double? SaleAmt { get; set; }
    public double? ChangeAmt { get; set; }
    public double? OverPaymentAmt { get; set; }
    public double? AmtTend { get; set; }
    public double? AmtUsed { get; set; }
    public string? CardNumber { get; set; }
    public string? TendName { get; set; }
    public int? IsHidden { get; set; }
  }
}