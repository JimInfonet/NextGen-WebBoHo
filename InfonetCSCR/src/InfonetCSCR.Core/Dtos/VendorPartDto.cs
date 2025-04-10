namespace InfonetCSCR.Core.Dtos;

public class VendorPartDto
{
  public string? PartNo { get; set; }
  public string? VendorId { get; set; }
  public string? StockCode { get; set; }
  public string? Description { get; set; }
  public decimal? ReOrderQuantity { get; set; }
  public decimal? MaximumQuantity { get; set; }
  public double? InStock { get; set; }
}
