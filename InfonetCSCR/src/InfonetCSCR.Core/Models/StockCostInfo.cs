namespace InfonetCSCR.Core.Models;

public class StockCostInfo
{
  public string? StockCode { get; set; }
  public string? StoreId { get; set; }
  public decimal? Cost { get; set; }
  public string? VendorId { get; set; }
  public string? Name { get; set; }
  public bool? ActiveVendor { get; set; }
}
