namespace InfonetCSCR.Core.Dtos;

public class StockCostInfoDto
{
  public string? StoreId { get; set; }
  public decimal? Cost { get; set; }
  public string? VendorId { get; set; }
  public string? Name { get; set; }
  public bool? ActiveVendor { get; set; }
  public string StockCode { get; set; }
  public short OrderNo { get; set; }
}
