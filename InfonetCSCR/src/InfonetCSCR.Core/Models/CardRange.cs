namespace InfonetCSCR.Core.Models;

public class CardRange
{
  public int CardCode { get; set; } = default!;
  public string LowerLimit { get; set; } = default!;
  public string? UpperLimit { get; set; }
}
