namespace InfonetCSCR.Core.Dtos;

public class GetStockCostInfoByMultipleVendorBatchBackOfficeDto
{
  public List<(string stockCode, string vendorId)> StockCodesAndVendorIds{ get; set; }
  public int OrderNo { get; set; } = 0;
}
