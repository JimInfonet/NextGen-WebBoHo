namespace InfonetCSCR.Core.Models;

public class ElectronicJournalSubreport
{
  public int SaleNo { get; set; } = default!;
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
  public double? InvoiceDiscount { get; set; }
  public string? CardNumber { get; set; }
  public string? TendName { get; set; }
}
