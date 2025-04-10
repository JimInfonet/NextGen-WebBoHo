namespace InfonetCSCR.Web.Models;

public class StockItemViewModel
{
  public string StockCode { get; set; } = default!;
  public string? Description { get; set; }
  public string? Format { get; set; }
  public string? UnitOfMeasure { get; set; }
  public string? Department { get; set; }
  public string? SubDepartment { get; set; }
  public string? Vendor { get; set; }
  public string? VendorPart { get; set; }
  public double? Cost { get; set; }
  public double Price { get; set; } = default!;
  public double? Available { get; set; }
  public bool? IsKit { get; set; }
  public bool? IsActive { get; set; }
  public int? TotalItems { get; set; }
}
