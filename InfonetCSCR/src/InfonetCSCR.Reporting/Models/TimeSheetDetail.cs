using System;

namespace InfonetCSCR.Reporting.Models
{
  public class TimesheetDetail
  {
    public int ReportId { get; set; }
    public string UserCode { get; set; }
    public DateTime ShiftDate { get; set; }
    public string DayOfWeek { get; set; }
    public string TimeIn { get; set; }
    public string TimeOut { get; set; }
    public double Hours { get; set; }
    public double Shortages { get; set; }
    public string? Comment { get; set; }
    public bool Stat { get; set; }
    public double? Commission { get; set; }
    public bool Training { get; set; }
  }
}