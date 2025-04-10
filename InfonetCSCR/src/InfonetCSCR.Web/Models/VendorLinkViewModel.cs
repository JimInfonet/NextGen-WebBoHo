namespace InfonetCSCR.Web.Models;

public class VendorLinkViewModel
{
  public string VendorId { get; set; } = default!;
  public string? RegionId { get; set; } = string.Empty;
  public string? StoreId { get; set; } = string.Empty;
}
