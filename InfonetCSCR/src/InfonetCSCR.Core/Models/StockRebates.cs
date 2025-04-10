namespace InfonetCSCR.Core.Models;

public class StockRebates : StockPriceModel
{
  public int RebateNo { get; set; }
  public decimal? Rebate { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
}
