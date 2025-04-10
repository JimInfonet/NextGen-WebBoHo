namespace InfonetCSCR.Core.Dtos;

public class TimeSheetReportDto
{
  public IEnumerable<TimeSheetMainReportDto> timeSheetMainReportData { get; set; } = default!;
  public IEnumerable<ShortageDto> shortageData { get; set; } = default!;
  public IEnumerable<TimesheetDetail> timesheetDetailsData { get; set; } = default!;
}
