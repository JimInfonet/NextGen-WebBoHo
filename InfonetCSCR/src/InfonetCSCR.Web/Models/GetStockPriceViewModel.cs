namespace InfonetCSCR.Web.Models;

public class GetStockPriceViewModel : StockPriceModel
{
  public int PricesInUse { get; set; }
  public string? PriceType { get; set; }
}
