namespace InfonetCSCR.Core.Dtos;

public class StockCostInfoBatchParameterBackOfficeDto
{
    public string? VendorId { get; set; }
    public IList<string>? StockCodes { get; set; }
    public int? OrderNo { get; set; }
}
