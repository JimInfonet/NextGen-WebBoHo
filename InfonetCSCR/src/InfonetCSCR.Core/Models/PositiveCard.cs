namespace InfonetCSCR.Core.Models;

public class PositiveCard
{
  public string CardNumber { get; set; } = default!;
  public bool Active { get; set; } = default!;
  public string? PIN { get; set; }
}
