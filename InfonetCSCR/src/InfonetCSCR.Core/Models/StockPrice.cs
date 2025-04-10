namespace InfonetCSCR.Core.Models;

public class StockPrice : StockPriceModel
{
  public int PriceNumber { get; set; }
  public decimal Price { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public decimal? Margin { get; set; }
  public DateTime? UpdateDate { get; set; }
}
