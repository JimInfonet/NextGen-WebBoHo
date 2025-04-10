namespace InfonetCSCR.Core.Models;

public class StockCostsList
{
  public IEnumerable<StockCosts>? Costs { get; set; }
  public List<string>? StoreIDs { get; set; }
}
