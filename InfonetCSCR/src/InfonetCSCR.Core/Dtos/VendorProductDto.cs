namespace InfonetCSCR.Core.Dtos;

public class VendorProductDto
{
  public string? StockCode { get; set; }
  public bool? ActiveStock { get; set; }
  public string? Description { get; set; }
  public string? UnitofMeasure { get; set; }
  public string? OrderUnit { get; set; }
  public int? Packing { get; set; }
  public string? Department { get; set; }
  public string? SubDepartment { get; set; }
  public string? SubDetail { get; set; }
  public string? PartNo { get; set; }
  public int? Index { get; set; }
}
