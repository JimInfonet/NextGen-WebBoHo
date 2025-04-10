namespace InfonetCSCR.Core.Dtos;

public class TimesheetHeaderWithUserDetailsDto : TimeSheetHeader
{
  public string? UserName { get; set; }
  public int? UserId { get; set; }
  public string? Type { get; set; }
  public DateTime? DateOfBirth { get; set; }
}
