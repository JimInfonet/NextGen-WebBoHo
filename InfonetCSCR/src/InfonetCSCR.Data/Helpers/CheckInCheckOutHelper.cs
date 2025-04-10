namespace InfonetCSCR.Data.Helpers;

public static class CheckInCheckOutHelper
{
  public static TimesheetDetailDto ConvertToTimeSheetDetailDto(TimesheetDetail? model)
  {
    if (model == null) return new TimesheetDetailDto { };

    TimesheetDetailDto convertedDto = new TimesheetDetailDto
    {
      ReportId = model.ReportId,
      UserCode = model.UserCode,
      ShiftDate = model.ShiftDate,
      DayOfWeek = model.DayOfWeek,
      TimeIn = TimeSpan.Parse(model.TimeIn),
      TimeOut = TimeSpan.Parse(model.TimeOut),
      Hours = model.Hours,
      Shortages = model.Shortages,
      Comment = model.Comment,
      Stat = model.Stat,
      Commission = model.Commission,
      Training = model.Training,
    };

    return convertedDto;
  }
}
