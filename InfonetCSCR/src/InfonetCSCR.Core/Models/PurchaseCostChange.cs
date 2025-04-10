namespace InfonetCSCR.Core.Models;

public class PurchaseCostChange
{
  public long PurchaseNo { get; set; }
  public string StockCode { get; set; } = default!;
  public string VendorID { get; set; } = default!;
  public int LineNum { get; set; }
  public decimal? NewCost { get; set; }
  public string TransDate { get; set; } = default!;
  public DateTime? PostedDate { get; set; }
  public string? PostedBy { get; set; }
}
