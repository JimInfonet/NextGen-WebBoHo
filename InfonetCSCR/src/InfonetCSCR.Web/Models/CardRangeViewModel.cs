namespace InfonetCSCR.Web.Models;

public class CardRangeViewModel
{
  public int CardCode { get; set; } = default!;
  public string LowerLimit { get; set; } = default!;
  public string? UpperLimit { get; set; }
}
