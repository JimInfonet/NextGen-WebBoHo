namespace InfonetCSCR.Web.Models;

public class PositiveCardViewModel
{
  public string CardNumber { get; set; } = default!;
  public bool Active { get; set; } = default!;
  public string? PIN { get; set; }
}
