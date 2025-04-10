namespace InfonetCSCR.Core.Dtos;

public class TimeSheetMainReportDto
{
  public long ReportID { get; set; }
  public string UserCode { get; set; } = default!;
  public DateTime PayDate { get; set; }
  public DateTime PeriodStarting { get; set; }
  public DateTime PeriodEnding { get; set; }
  public decimal UniformPurchases { get; set; }
  public DateTime ShiftDate { get; set; }
  public string DayOfWeek { get; set; } = default!;
  public string TimeIn { get; set; } = default!;
  public string TimeOut { get; set; } = default!;
  public decimal Hours { get; set; }
  public decimal Shortages { get; set; }
  public string Comment { get; set; } = default!;
  public bool Stat { get; set; }
  public decimal? Commission { get; set; }
  public bool Training { get; set; }
  public string UserName { get; set; } = default!;
  public long? UserId { get; set; }
  public string Type { get; set; } = default!;
  public DateTime? SemiMonthlyGroupingColumn { get; set; }
  public int? BiWeeklyGroupingColumn { get; set; }
}
