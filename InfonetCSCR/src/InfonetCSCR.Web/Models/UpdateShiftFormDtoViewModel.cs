namespace InfonetCSCR.Web.Models;

public class UpdateShiftFormDtoViewModel : TimesheetDetailDtoViewModel
{
  public int PolicyRoundTime { get; set; }
  public string CellType { get; set; }
  public TimeSpan? NewTimeIn { get; set; }
}
