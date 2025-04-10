namespace InfonetCSCR.Web.Models;

public class AddReclineViewModel
{
  public decimal? Quantity { get; set; }
  public decimal? Cost { get; set; }
  public decimal? Invoice { get; set; }
  public string StockCode { get; set; } = default!;
  public string? Description { get; set; }
  public string? ExceptedTax { get; set; }
  public int? CostDec { get; set; }
  public string? CostType { get; set; }
  public int Purpose { get; set; }
  public bool IsQuantityUpdate { get; set; }
}
