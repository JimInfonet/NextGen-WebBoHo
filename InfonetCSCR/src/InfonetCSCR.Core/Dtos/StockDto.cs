namespace InfonetCSCR.Core.Dtos;

public class StockDto
{
  public string StockCode { get; set; } = default!;
  public string? VendorPart { get; set; }
  public string? Description { get; set; }
}
