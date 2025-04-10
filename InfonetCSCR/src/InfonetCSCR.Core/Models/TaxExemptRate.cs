namespace InfonetCSCR.Core.Models;

public class TaxExemptRate
{
  public int TaxCode { get; set; } = default!;
  public double TaxRate { get; set; } = default!;
  public string? TaxDescirption { get; set; }
  public string? RateType { get; set; }
  public string? ProductCode { get; set; }
  public int? BaseQuantity { get; set; }
  public int? BaseCode { get; set; }
  public double? SurCharge { get; set; }
}
