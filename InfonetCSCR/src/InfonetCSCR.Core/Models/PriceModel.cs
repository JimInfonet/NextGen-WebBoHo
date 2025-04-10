namespace InfonetCSCR.Core.Models;

public class PriceModel : StockPriceModel
{
  public IEnumerable<StockPrice>? Prices { get; set; }
  public int PricesInUse { get; set; }
}
