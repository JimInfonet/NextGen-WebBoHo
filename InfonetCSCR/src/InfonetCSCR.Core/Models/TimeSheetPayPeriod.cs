namespace InfonetCSCR.Core.Models;

public class TimeSheetPayPeriod
{
  public DateTime? PayDate { get; set; }
  public DateTime? PeriodStarting { get; set; }
  public DateTime? PeriodEnding { get; set; }
  public int? InitialReportId { get; set; } = 1;
  public int? InitialEmpId { get; set; } = 1;
}
