namespace InfonetCSCR.Core.Models;

public class StockCharge
{
  public string? StockCode { get; set; }
  public string? AssocCode { get; set; }
  public string? AssocDescription { get; set; }
  public decimal? AssocPrice { get; set; }
}
