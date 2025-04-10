namespace InfonetCSCR.Core.Dtos;

public class CommodityTransferReportDto
{
  public int BatchId { get; set; } = default!;
  public string Dept { get; set; } = default!;
  public string SubDept { get; set; } = default!;
  public string SubDetail { get; set; } = default!;
  public string? Description { get; set; }
  public string StoreId { get; set; } = default!;
  public DateTime? TransDate { get; set; }
  public string? TransUser { get; set; }
  public string? TransFlag { get; set; }
  public string? TransType { get; set; }
  public string? StoreDesc { get; set; }
  public string? UserName { get; set; }
}
