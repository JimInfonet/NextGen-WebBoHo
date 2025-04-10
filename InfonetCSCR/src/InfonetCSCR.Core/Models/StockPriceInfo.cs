namespace InfonetCSCR.Core.Models;

public class StockPriceInfo
{
  public string? StoreId { get; set; } = default!;
  public string Stock_Code { get; set; } = default!;
  public short Price_Number { get; set; } = default!;
  public string VendorId { get; set; } = default!;
  public short OrderNo { get; set; } = default!;
  public decimal Price { get; set; } = default!;
  public decimal? Margin { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public DateTime? UpdateDate { get; set; }
}
