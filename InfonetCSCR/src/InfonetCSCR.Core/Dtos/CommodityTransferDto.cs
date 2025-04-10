namespace InfonetCSCR.Core.Dtos;
public class CommodityTransferDto
{
  public int BatchId { get; set; } = default!;
  public string Dept { get; set; } = default!;
  public string SubDept { get; set; } = default!;
  public string SubDetail { get; set; } = default!;
  public string? Description { get; set; }
  public string StoreID { get; set; } = default!;
  public string? TransType { get; set; }
}