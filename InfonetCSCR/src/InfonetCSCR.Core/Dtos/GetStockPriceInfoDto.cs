namespace InfonetCSCR.Core.Dtos;

public class GetStockPriceInfoDto
{
    public string? StoreId { get; set; }
    public string StockCode { get; set; }
    public int PriceNumber { get; set; }
    public string VendorId { get; set; }
    public int OrderNo { get; set; }
}
