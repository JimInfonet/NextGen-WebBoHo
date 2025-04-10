namespace InfonetCSCR.Web.Models;

public class StockPriceInfoDto
{
  public string StoreId { get; set; }
  public string StockCode { get; set; }
  public int PriceNumber { get; set; }
  public string VendorId { get; set; }
  public int OrderNo { get; set; }
  public decimal? Price { get; set; }
  public decimal? Margin { get; set; }
}
