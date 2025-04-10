namespace InfonetCSCR.Core.Models;

public class StockCosts : StockPriceModel
{
  public decimal? Cost { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public bool? ActiveVendor { get; set; }
  public string? ActiveVendorCode { get; set; }
  public DateTime? UpdateDate { get; set; }
}
