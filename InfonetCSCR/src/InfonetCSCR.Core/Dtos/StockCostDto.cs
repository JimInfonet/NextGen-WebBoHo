namespace InfonetCSCR.Core.Dtos;

public class StockCostDto
{
  public string StockCode { get; set; } = default!;
  public string VendorId { get; set; } = default!;
  public decimal? Cost { get; set; }
  public int? ActiveVendor { get; set; }
}
