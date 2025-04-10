namespace InfonetCSCR.Core.Dtos;
public class CostChangeTransferDto
{
  public int BatchId { get; set; } = default!;
  public string Product { get; set; } = default!;
  public string? Description { get; set; }
  public decimal? NewCost { get; set; }
  public string StoreId { get; set; } = default!;
  public string? VendorId { get; set; }
  public Int16 OrderNo { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public bool? ActiveVendor { get; set; }
  public bool? PriceChangeWithCost { get; set; }
}