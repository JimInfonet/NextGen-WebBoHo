namespace InfonetCSCR.Core.Models;

public class GroupButton
{
  public int ButtonId { get; set; } = default!;
  public string? ProductCode { get; set; }
  public string? Description1 { get; set; }
  public string? Description2 { get; set; }
  public int? ButtonType { get; set; }
}
