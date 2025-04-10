namespace InfonetCSCR.Web.Models;

public class StockItemMinimalFilterDtoViewModel
{
  public int TotalItems { get; set; }
  public IEnumerable<StockItemMinimalViewModel> StockItems { get; set; }
}
