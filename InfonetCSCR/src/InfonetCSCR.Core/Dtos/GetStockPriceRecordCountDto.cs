namespace InfonetCSCR.Core.Dtos;

public class GetStockPriceRecordCountDto
{
  public string StockCode { get; set; }
  public string StoreId { get; set; }
  public int OrderNo { get; set; }
  public string VendorId { get; set; }
}
