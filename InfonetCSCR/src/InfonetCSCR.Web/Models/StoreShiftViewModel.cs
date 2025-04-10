namespace InfonetCSCR.Web.Models;

public class StoreShiftViewModel
{
  public int ShiftNumber { get; set; } = default!;
  public string StartTime { get; set; } = default!;
  public bool? ActiveStatus { get; set; }
  public bool? CurrentDay { get; set; }
}
