namespace InfonetCSCR.Core.Models;

public class Product : StockMaster
{
  public string? Branch { get; set; }
  public decimal? MaxQty { get; set; }
  public decimal? ReOrderQty { get; set; }
  public decimal? AvailableQty { get; set; }
  public decimal? MaxQtyTE { get; set; }
  public decimal? ReOrderQtyTE { get; set; }
  public decimal? AvailableQtyTE { get; set; }
}
