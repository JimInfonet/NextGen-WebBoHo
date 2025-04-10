namespace InfonetCSCR.Web.Models;

public class StockViewModel
{
  public string StockCode { get; set; } = default!;
  public string? VendorPart { get; set; }
  public string? Description { get; set; }
}
