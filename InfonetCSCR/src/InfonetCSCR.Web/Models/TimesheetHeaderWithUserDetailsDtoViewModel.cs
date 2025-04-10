namespace InfonetCSCR.Web.Models;

public class TimesheetHeaderWithUserDetailsDtoViewModel : TimeSheetHeaderViewModel
{
  public string? UserName { get; set; }
  public int? UserId { get; set; }
  public string? Type { get; set; }
  public DateTime? DateOfBirth { get; set; }
}
