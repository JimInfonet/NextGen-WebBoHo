namespace InfonetCSCR.Web.Models;

public class PriceViewModel : StockPriceModel
{
  public IEnumerable<StockPriceViewModel>? Prices { get; set; }
  public int PricesInUse { get; set; }
}
