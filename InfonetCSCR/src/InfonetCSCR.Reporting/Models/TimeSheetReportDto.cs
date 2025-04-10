using System.Collections.Generic;

namespace InfonetCSCR.Reporting.Models
{
  public class TimeSheetReportDto
  {
    public IEnumerable<TimeSheetMainReportDto> TimeSheetMainReportData { get; set; }
    public IEnumerable<ShortageDto> ShortageData { get; set; }
    public IEnumerable<TimesheetDetail> TimesheetDetailsData { get; set; }
    public double PriorOS { get; set; }
    public int OvertimeHPolicy { get; set; }
    public int OvertimeSPolicy { get; set; }
  }
}