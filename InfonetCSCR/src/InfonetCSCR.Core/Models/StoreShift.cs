namespace InfonetCSCR.Core.Models;

public class StoreShift
{
  public int ShiftNumber { get; set; } = default!;
  public DateTime StartTime { get; set; } = default!;
  public bool? ActiveStatus { get; set; }
  public bool? CurrentDay { get; set; }
}
