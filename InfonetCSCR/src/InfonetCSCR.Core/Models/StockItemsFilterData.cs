namespace InfonetCSCR.Core.Models;

public class StockItemsFilterData
{
  public int totalItems { get; set; }
  public IEnumerable<StockItem>? stockItems { get; set; }
}
