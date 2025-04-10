namespace InfonetCSCR.Core.Dtos;

public class TaxExemptDto
{
  public string? Department { get; set; }
  public string? SubDepartment { get; set; }
  public string? Detail { get; set; }
  public int? TaxType { get; set; }
  public int? UnitsPerPkg { get; set; }
  public string? Category { get; set; }
  public int? QuantityPerPkg { get; set; }
  public int? BaseUnitQty { get; set; }
  public string? ProductCode { get; set; }
  public string? EmployeeId { get; set; }
  public double TaxFreePrice { get; set; }
}
