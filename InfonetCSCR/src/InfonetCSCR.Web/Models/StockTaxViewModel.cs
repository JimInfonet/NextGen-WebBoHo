namespace InfonetCSCR.Web.Models;

public class StockTaxViewModel
{
  public string? StockCode { get; set; }
  public string? TaxName { get; set; }
  public string? TaxCode { get; set; }
  public decimal? TaxRate { get; set; }
}
