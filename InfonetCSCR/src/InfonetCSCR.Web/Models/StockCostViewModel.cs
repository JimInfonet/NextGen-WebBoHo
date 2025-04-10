namespace InfonetCSCR.Web.Models;

public class StockCostViewModel
{
  public string StockCode { get; set; }
  public string StoreId { get; set; }
  public string VendorId { get; set; }
  public decimal? Cost { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public int? ActiveVendor { get; set; }
  public int OrderNo { get; set; }
  public DateTime? UpdateDate { get; set; }
  public string? Description { get; set; }
}
