namespace InfonetCSCR.Web.Models;

public class StockRebatesViewModel : StockPriceModel
{
  public int RebateNo { get; set; }
  public decimal? Rebate { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
}
