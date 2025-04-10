namespace InfonetCSCR.Web.Models;

public class VendorPartDtoViewModel
{
  public string? PartNo { get; set; }
  public string? VendorId { get; set; }
  public string? StockCode { get; set; }
  public string? Description { get; set; }
  public double? ReOrderQuantity { get; set; }
  public double? MaximumQuantity { get; set; }
  public double? InStock { get; set; }
}
