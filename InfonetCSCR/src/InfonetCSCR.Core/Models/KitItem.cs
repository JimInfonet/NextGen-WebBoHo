namespace InfonetCSCR.Core.Models;

public class KitItem
{
  public string KitCode { get; set; } = default!;
  public string StockCode { get; set; } = default!;
  public double? Quantity { get; set; }
}
