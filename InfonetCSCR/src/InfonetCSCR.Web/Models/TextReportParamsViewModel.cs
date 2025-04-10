namespace InfonetCSCR.Web.Models;

public class TextReportParamsViewModel
{
  public string? ReportName { get; set; }
  public string? StoreID { get; set; }
  public string? StoreName { get; set; }
  public DateTime? ReadingDate { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public DateTime? SaleDate { get; set; }
  public string? TerminalID { get; set; }
  public string? StartCard { get; set; }
  public string? EndCard { get; set; }
  public string? StartCustomer { get; set; }
  public string? EndCustomer { get; set; }
  public string? StartEmployee { get; set; }
  public string? EndEmployee { get; set; }
  public string? StartDepartment { get; set; }
  public string? EndDepartment { get; set; }
  public bool IsSummaryByGrade { get; set; }
  public int? Period { get; set; }
  public int? NullDept { get; set; }
}
