namespace InfonetCSCR.Web.Models;

public class GetReportsDtoViewModel
{
  public CheckInCheckOutPoliciesDtoViewModel Policies { get; set; }
  public string UserCode { get; set; }
  public bool IsViewCurrent { get; set; }
}
