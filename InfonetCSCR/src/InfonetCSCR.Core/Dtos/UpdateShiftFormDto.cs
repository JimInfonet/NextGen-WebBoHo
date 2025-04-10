namespace InfonetCSCR.Core.Dtos;

public class UpdateShiftFormDto : TimesheetDetailDto
{
  public int PolicyRoundTime { get; set; }
  public string CellType { get; set; }
  public TimeSpan? NewTimeIn { get; set; }
}
