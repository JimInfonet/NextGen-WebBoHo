namespace InfonetCSCR.Core.Models;

public class CostChangeTransfer
{
  public int BatchId { get; set; }
  public string ProductId { get; set; }
  public string? Description { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public decimal? OldCost { get; set; }
  public decimal? NewCost { get; set; }
  public string StoreId { get; set; }
  public string? PriceGroupId { get; set; }
  public string? RegionId { get; set; }
  public string? VendorId { get; set; }
  public short? OrderNo { get; set; }
  public DateTime? TransDate { get; set; }
  public string? TransUser { get; set; }
  public string? TransFlag { get; set; }
  public bool? ActiveVendor { get; set; }
  public bool? PriceChangeWithCost { get; set; }
}
