namespace InfonetCSCR.Core.Models;

public class Reason
{
  public string Code { get; set; } = default!;
  public string? Description { get; set; }
  public char Type { get; set; } = default!;
}
