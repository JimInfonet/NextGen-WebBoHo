namespace InfonetCSCR.Core.Models;

public class StockTax
{
  public string? StockCode { get; set; }
  public string? TaxName { get; set; }
  public string? TaxCode { get; set; }
  public decimal? TaxRate { get; set; }
}
