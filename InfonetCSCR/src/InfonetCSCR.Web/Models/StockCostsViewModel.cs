namespace InfonetCSCR.Web.Models;

public class StockCostsViewModel : StockPriceModel
{
  public decimal? Cost { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public bool? ActiveVendor { get; set; }
  public DateTime? UpdateDate { get; set; }
}
