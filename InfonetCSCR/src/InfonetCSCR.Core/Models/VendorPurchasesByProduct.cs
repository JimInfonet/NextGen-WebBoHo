namespace InfonetCSCR.Core.Models;

public class VendorPurchasesByProduct
{
  public string? VendorName { get; set; }
  public DateTime? PostedDate { get; set; }
  public double? ReceivedQty { get; set; }
  public double? Cost { get; set; }
  public double? ExtCost { get; set; }
}