namespace InfonetCSCR.Web.Models;

public class CheckInCheckOutUserDtoViewModel
{
  public string UserName { get; set; }
  public DateTime ShiftDate { get; set; }
  public bool? CheckedOut { get; set;}
}
