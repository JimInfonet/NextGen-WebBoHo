namespace InfonetCSCR.Web.Models;

public class CostViewModel : StockPriceModel
{
  public decimal cost { get; set; }
  public bool? ActiveVendor { get; set; }
  public string? ActiveVendorCode { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
}
