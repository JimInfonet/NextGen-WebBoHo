namespace InfonetCSCR.Web.Models;

public class ExcelReportGeneratorViewModel
{
  public int? ReportNo { get; set; }
  public DateTime? SelectedDate { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public string? Department { get; set; }
  public string? DepartmentName { get; set; }
  public string? GroupId { get; set; }
  public string? StoreId { get; set; }
  public string? MemberNo { get; set; }
  public string? TaxExemptType { get; set; }
  public string? VersionPolicy { get; set; }
}
