namespace InfonetCSCR.Core.Records;

public class PurchaseHeaderUpdateRecord
{
  public PurchaseHeader? PurchaseHeader { get; set; }
  public bool? IsEditTotals { get; set; }
  public string? VendorId { get; set; }
}
