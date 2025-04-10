namespace InfonetCSCR.Web.Models;

public class StockItemsResultModel
{
  public int totalItems { get; set; }
  public IEnumerable<StockItemViewModel>? stockItems { get; set; }
}
