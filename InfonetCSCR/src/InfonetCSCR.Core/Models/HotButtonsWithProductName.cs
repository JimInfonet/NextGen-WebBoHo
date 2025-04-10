namespace InfonetCSCR.Core.Models;

public class HotButtonsWithProductName
{
  public int ButtonId { get; set; } = default!;
  public string? Product { get; set; }
  public string? ProductName { get; set; }
  public string? Caption1 { get; set; }
  public string? Caption2 { get; set; }
}
