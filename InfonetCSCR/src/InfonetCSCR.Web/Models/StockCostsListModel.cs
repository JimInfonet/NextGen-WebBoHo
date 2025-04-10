namespace InfonetCSCR.Web.Models;

public class StockCostsListModel
{
  public IEnumerable<StockCostsViewModel>? Costs { get; set; }
  public List<string>? StoreIDs { get; set; }
}
