namespace InfonetCSCR.Web.Models;

public class PerformCheckInOrOutDtoViewModel
{
  public CheckInCheckOutPoliciesDtoViewModel Policies { get; set; }
  public int NumRec { get; set; }
  public string UserCode { get; set; }
  public string CommentFieldValue { get; set; }
  public bool CheckOutPreviousDay { get; set; }
  public bool IsCheckIn { get; set; }
}
