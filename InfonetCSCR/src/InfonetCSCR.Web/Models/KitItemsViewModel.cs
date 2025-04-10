namespace InfonetCSCR.Web.Models;

public class KitItemsViewModel
{
  public string StockCode { get; set; } = default!;
  public string? KitCode { get; set; }
  public string? Description { get; set; }
  public decimal? Quantity { get; set; }
}
