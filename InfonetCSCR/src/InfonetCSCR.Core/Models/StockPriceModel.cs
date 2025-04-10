namespace InfonetCSCR.Core.Models;

public class StockPriceModel
{
  public string? StockCode { get; set; }
  public string? VendorID { get; set; }
  public int OrderNo { get; set; }
  public string? StoreID { get; set; }
  public List<string>? StoreIDs { get; set; }
  public List<string>? VendorIDs { get; set; }
}
