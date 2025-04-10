using System;

namespace InfonetCSCR.Reporting.Models
{
  public class TimeSheetMainReportDto
  {
    public long ReportID { get; set; }
    public string UserCode { get; set; }
    public DateTime PayDate { get; set; }
    public DateTime PeriodStarting { get; set; }
    public DateTime PeriodEnding { get; set; }
    public decimal UniformPurchases { get; set; }
    public DateTime ShiftDate { get; set; }
    public string DayOfWeek { get; set; }
    public string TimeIn { get; set; }
    public string TimeOut { get; set; }
    public decimal Hours { get; set; }
    public decimal Shortages { get; set; }
    public string Comment { get; set; }
    public bool Stat { get; set; }
    public decimal? Commission { get; set; }
    public bool Training { get; set; }
    public string UserName { get; set; }
    public long? UserId { get; set; }
    public string Type { get; set; }
    public DateTime? SemiMonthlyGroupingColumn { get; set; }
    public int? BiWeeklyGroupingColumn { get; set; }
  }
}