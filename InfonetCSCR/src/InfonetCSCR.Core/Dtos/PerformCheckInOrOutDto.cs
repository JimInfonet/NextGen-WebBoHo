namespace InfonetCSCR.Core.Dtos;

public class PerformCheckInOrOutDto
{
  public CheckInCheckOutPoliciesDto Policies { get; set; }
  public int NumRec { get; set; }
  public string UserCode { get; set; }
  public string CommentFieldValue { get; set; }
  public bool CheckOutPreviousDay { get; set; }
  public bool IsCheckIn { get; set; }
}
