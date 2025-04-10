namespace InfonetCSCR.Core.Dtos;

public class CreateCostRecordDto
{
  public string StoreId { get; set; }
  public string ProductId { get; set; }
  public short OrderNo { get; set; }
  public string VendorId { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public string? ShowByOption { get; set; }
}
