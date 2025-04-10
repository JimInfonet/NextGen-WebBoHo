namespace InfonetCSCR.Core.Models;

public class StockItemMinimal
{
  public string StockCode { get; set; } = default!;
  public string? Description { get; set; }
  public string? Dept { get; set; }
  public string? SubDept { get; set; }
  public string? SubDetail { get; set; }
  public string? TransferType { get; set; } = "A";
  public string? VendorId { get; set; }
}
