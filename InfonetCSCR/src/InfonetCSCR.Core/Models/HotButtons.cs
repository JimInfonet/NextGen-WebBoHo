namespace InfonetCSCR.Core.Models;

public class HotButtons
{
  public int ButtonId { get; set; } = default!;
  public string? ProductCode { get; set; }
  public string? Description1 { get; set; }
  public string? Description2 { get; set; }
  public decimal? DefaultQuantity { get; set; }
  public int? ColorCode { get; set; }
  public decimal? BottlePrice { get; set; }
}
