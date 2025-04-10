namespace InfonetCSCR.Core.Dtos;

public class StockCostAndRecLineDto
{
  public string StockCode { get; set; } = default!;
  public string VendorId { get; set; } = default!;
  public decimal? Cost { get; set; }
  public int? ActiveVendor { get; set; }
  public int? RecNumber { get; set; }
}
