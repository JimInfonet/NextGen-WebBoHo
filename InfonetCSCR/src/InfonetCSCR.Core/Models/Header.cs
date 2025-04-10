namespace InfonetCSCR.Core.Models;

public class Header
{
  public byte ImportNumber { get; set; }
  public string? ImportDescription { get; set; }
  public string? ImportType { get; set; }
  public string? Delimiter { get; set; }
  public byte? FieldCount { get; set; }
  public byte? Purpose { get; set; }
}
