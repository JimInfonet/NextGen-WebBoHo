namespace InfonetCSCR.Web.Models;

public class CheckInCheckOutPoliciesDtoViewModel
{
  public bool Timesheets { get; set; }
  public bool HaveUserId { get; set; }
  public string Version { get; set; }
  public bool Hour24Store { get; set; }
  public string PayPeriod { get; set; }
  public bool UseProvHoliday { get; set; }
  public int Roundtime { get; set; }
  public int OvertimeH { get; set; }
  public int OvertimeS { get; set; }
  public bool PyFile { get; set; }
  public bool TSSignpad { get; set; }
  public bool ViewReports { get; set; }
  public bool EditPostRP { get; set; }
  public bool ChangeCheck { get; set; }

}

