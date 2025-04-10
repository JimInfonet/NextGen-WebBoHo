namespace InfonetCSCR.Web.Models;

public class StockCostInfoDto
{
  public string? StockCode { get; set; }
  public string? StoreId { get; set; }
  public short OrderNo { get; set; }
  public decimal? Cost { get; set; }
  public string? VendorId { get; set; }
  public string? Name { get; set; }
  public bool? ActiveVendor { get; set; }
}
