namespace InfonetCSCR.Web.Models;

public class AddVendorPartsDtoViewModel
{
  public PurchasingAndReceivingPoliciesViewModel Policies { get; set; }
  public List<VendorPartDetailsDtoViewModel> VendorParts { get; set; }
  public string VendorId { get; set; }
  public string PurchaseNo { get; set; }
}
