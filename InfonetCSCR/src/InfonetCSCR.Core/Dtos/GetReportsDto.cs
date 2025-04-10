namespace InfonetCSCR.Core.Dtos;

public class GetReportsDto
{
  public CheckInCheckOutPoliciesDto Policies { get; set;}
  public string UserCode { get; set;}
  public bool IsViewCurrent { get; set;}
}
