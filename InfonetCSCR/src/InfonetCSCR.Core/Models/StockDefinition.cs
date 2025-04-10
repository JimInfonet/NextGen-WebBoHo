namespace InfonetCSCR.Core.Models;

public class StockDefinition
{
  public int BatchId { get; set; } = default!;
  public string ProductId { get; set; } = default!;
  public string? Description { get; set; }
  public string? StoreId { get; set; }
  public string? PriceGroupId { get; set; }
  public string? RegionId { get; set; }
  public DateTime? TransDate { get; set; }
  public string? TransUser { get; set; }
  public string? TransFlag { get; set; }
  public string? TransType { get; set; }
  public string? VendorId { get; set; }
}
