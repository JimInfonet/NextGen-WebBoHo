namespace InfonetCSCR.Core.Dtos;

public class DetermineInOrOutDto
{
  public int NumRec { get; set; }
  public bool CheckedOut { get; set; }
  public bool CheckOutPreviousDay { get; set; }
}
