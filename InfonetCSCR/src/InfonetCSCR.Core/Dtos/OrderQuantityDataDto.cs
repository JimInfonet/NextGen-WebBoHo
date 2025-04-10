namespace InfonetCSCR.Core.Dtos;

public class OrderQuantityDataDto
{
  public string? StockCode { get; set; }
  public string? UnitOfMeasure { get; set; }
  public int? Packing { get; set; }
  public double? MinQuantity { get; set; }
  public double? MaxQuantity { get; set; }
  public double? ReordQuantity { get; set; }
  public double? InStock { get; set; }
  public string? VendorId { get; set; }
}
