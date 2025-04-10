namespace InfonetCSCR.Web.Models;

public class PriceChangeTransferViewModel
{
  public int BatchId { get; set; }
  public string ProductId { get; set; }
  public string? Description { get; set; }
  public string? PriceType { get; set; }
  public string StoreId { get; set; }
  public string? PriceGroupId { get; set; }
  public string? RegionId { get; set; }
  public DateTime? TransDate { get; set; }
  public string? TransUser { get; set; }
  public string? TransFlag { get; set; }
  public string VendorId { get; set; }
  public int? OrderNo { get; set; }
  public decimal? CurrentPrice { get; set; }
  public decimal? Cost { get; set; }
}
