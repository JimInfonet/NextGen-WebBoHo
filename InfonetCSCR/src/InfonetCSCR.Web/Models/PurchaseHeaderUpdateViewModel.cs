namespace InfonetCSCR.Web.Models;

public class PurchaseHeaderUpdateViewModel
{
  public PurchaseHeaderViewModel? PurchaseHeader { get; set; }
  public bool? IsEditTotals { get; set; }
  public string? VendorId { get; set; }
}
