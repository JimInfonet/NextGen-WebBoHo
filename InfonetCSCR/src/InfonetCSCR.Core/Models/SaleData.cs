namespace InfonetCSCR.Core.Models;

public class SaleData
{
  public Company? Company { get; set; }
  public DateTime? ShiftDate { get; set; }
  public string? Dept { get; set; }
  public string? Sub_Dept { get; set; }
  public string? Stock { get; set; }
  public Decimal? Volume { get; set; }
  public Decimal? Sales { get; set; }
  public Decimal? ExemptedTax { get; set; }
  public bool? TaxExempt { get; set; }
  public int? Gradeid { get; set; }
  public Decimal? Cost { get; set; }
  public bool? Member { get; set; }
}
