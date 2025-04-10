namespace InfonetCSCR.Web.Models;

public class PerformCheckInOrOutResponseDtoViewModel
{
  public string? HeaderText { get; set; }
  public string? WarningMessage { get; set; }
  public string? SuccessMessage { get; set; }
  public bool? CommentNeeded { get; set; }
  public bool Success { get; set; }
}
