namespace InfonetCSCR.Web.Models;

public class PurchaseDetailViewModel
{
  public long PurchaseNo { get; set; } = default!;
  public int LineNumber { get; set; } = default!;
  public string? PartNumber { get; set; }
  public string? Description { get; set; }
  public string? UnitOfMeasure { get; set; }
  public int? UnitQuantity { get; set; }
  public double? OrderedQuantity { get; set; }
  public double? ReceivedQuantity { get; set; }
  public double? Cost { get; set; }
  public double? ExtCost { get; set; }
  public bool? ApplyGST { get; set; }
  public double? GST { get; set; }
  public string? StockCode { get; set; }
  public string? Department { get; set; }
  public string? SubDepartment { get; set; }
  public bool? Posted { get; set; }
  public bool? CostChanged { get; set; }
  public double? CurrentStock { get; set; }
  public bool? PackQuantityChanged { get; set; }
  public string? ProductVendor { get; set; }
  public string? SubDetail { get; set; }
  public double? Charges { get; set; }
  public double? CalculateDiscount { get; set; }
  public double? PoCost { get; set; }
  public string? Date { get; set; }
  public decimal? Amount { get; set; }
}
