namespace InfonetCSCR.Core.Records;

public record PurchaseHeaderRecord
{
  public long PurchaseNo { get; set; }
  public string? VendorId { get; set; }
  public string? VendorName { get; set; }
  public string? UserCode { get; set; }
  public string? StoreId { get; set; }
  public bool IsAutomated { get; set; }
}
