namespace InfonetCSCR.Core.Dtos;

public class StockMstDto
{
  public string StockCode { get; set; } = default!;
  public string VendorId { get; set; } = default!;
  public decimal? Cost { get; set; }
  public bool? ActiveVendor { get; set; }
}
