namespace InfonetCSCR.Web.Models;

public class RebateViewModel : StockPriceModel
{
  public decimal Rebate { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
}
