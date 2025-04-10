namespace InfonetCSCR.Core.Models;

public class Restriction
{
  public int Code { get; set; } = default!;
  public string? Description { get; set; }
  public int? AgeLimit { get; set; }
}
