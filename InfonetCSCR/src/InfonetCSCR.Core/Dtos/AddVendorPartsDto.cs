namespace InfonetCSCR.Core.Dtos;

public class AddVendorPartsDto
{
  public PurchasingAndReceivingPoliciesDto Policies { get; set; }
  public List<VendorPartDetailsDto> VendorParts { get; set; }
  public string VendorId { get; set; }
  public string PurchaseNo { get; set; }
}
