namespace InfonetCSCR.Core.Dtos;

public class StoreShiftDto
{
  public int ShiftNumber { get; set; } = default!;
  public string StartTime { get; set; } = default!;
  public bool? ActiveStatus { get; set; }
  public bool? CurrentDay { get; set; }
}
