namespace InfonetCSCR.Core.Models;

public class EmpCommission
{
  public string? EmployeeId { get; set; } 
  public string? StockCode { get; set; } 
  public string? Dept { get; set; } 
  public string? SubDept { get; set; } 
  public string? SubDetail { get; set; } 
  public string? Name { get; set; } 
  public double? Commission { get; set; } 
  public double? CommissionByDol { get; set; } 
  public bool? Apply { get; set; } 
}
