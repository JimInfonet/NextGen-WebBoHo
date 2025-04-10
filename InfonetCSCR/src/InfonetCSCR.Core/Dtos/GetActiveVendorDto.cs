namespace InfonetCSCR.Core.Dtos;

public class GetActiveVendorDto
{
  public string StoreId { get; set; }
  public string StockCode { get; set; }
  public short OrderNo { get; set; }
  public string? ShowByOption { get; set; }
  public string? VendorId { get; set; }
}
