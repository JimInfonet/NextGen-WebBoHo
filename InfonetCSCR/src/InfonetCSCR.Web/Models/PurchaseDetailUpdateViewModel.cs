namespace InfonetCSCR.Web.Models;

public class PurchaseDetailUpdateViewModel
{
  public string? VendorId { get; set; }
  public PurchaseDetailViewModel? PurchaseDetail { get; set; }
  public PurchasingAndReceivingPoliciesViewModel? Policies { get; set; }
}
