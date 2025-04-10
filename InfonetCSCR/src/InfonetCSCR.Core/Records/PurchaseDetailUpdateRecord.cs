namespace InfonetCSCR.Core.Records;

public record PurchaseDetailUpdateRecord
{
  public string? VendorId { get; set; }
  public PurchaseDetail? PurchaseDetail { get; set; }
  public PurchasingAndReceivingPoliciesDto? Policies { get; set; }
}
