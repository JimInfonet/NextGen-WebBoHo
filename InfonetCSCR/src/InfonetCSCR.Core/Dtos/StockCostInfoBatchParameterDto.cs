namespace InfonetCSCR.Core.Dtos;

public class StockCostInfoBatchParameterDto
{
    public string? VendorId { get; set; }
    public IList<string>? StockCodes { get; set; }
    public int? OrderNo { get; set; }
    public string? StoreID { get; set; }
}
