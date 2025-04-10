namespace InfonetCSCR.Web.Models;

public class GetStockCostInfoViewModel
{
  public string StoreId { get; set; }
  public string StockCode { get; set; }
  public short OrderNo { get; set; }
  public string? VendorId { get; set; }
}
