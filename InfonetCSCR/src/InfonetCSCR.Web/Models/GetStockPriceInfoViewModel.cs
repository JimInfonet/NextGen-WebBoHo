namespace InfonetCSCR.Web.Models;
public class GetStockPriceInfoViewModel
{
  public string StoreId { get; set; }
  public string StockCode { get; set; }
  public short PriceNumber { get; set; }
  public string VendorId { get; set; }
  public short OrderNo { get; set; }
}
