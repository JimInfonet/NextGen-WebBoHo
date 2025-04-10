namespace InfonetCSCR.Core.Dtos;

public class CostChangeReportDto
{
  public int BatchId { get; set; } = default!;
  public string ProductId { get; set; } = default!;
  public string? Description { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public Decimal? OldCost { get; set; }
  public Decimal? NewCost { get; set; }
  public string StoreId { get; set; } = default!;
  public string? VendorId { get; set; }
  public Int16? OrderNo { get; set; }
  public DateTime? TransDate { get; set; }
  public string? TransUser { get; set; }
  public string? TransFlag { get; set; }
  public string Name { get; set; } = default!;
  public string? StoreDesc { get; set; }
}
