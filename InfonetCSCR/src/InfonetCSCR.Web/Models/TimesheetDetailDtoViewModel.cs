namespace InfonetCSCR.Web.Models;

public class TimesheetDetailDtoViewModel
{
  public int ReportId { get; set; }
  public string UserCode { get; set; }
  public DateTime ShiftDate { get; set; }
  public string DayOfWeek { get; set; }
  public TimeSpan TimeIn { get; set; }
  public TimeSpan TimeOut { get; set; }
  public double Hours { get; set; }
  public double Shortages { get; set; }
  public string? Comment { get; set; }
  public bool Stat { get; set; }
  public double? Commission { get; set; }
  public bool Training { get; set; }
}
