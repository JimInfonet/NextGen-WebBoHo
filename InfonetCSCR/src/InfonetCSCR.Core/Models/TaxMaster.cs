namespace InfonetCSCR.Core.Models;

public class TaxMaster
{
  public string Name { get; set; } = default!;
  public bool? Active { get; set; }
  public short? Order { get; set; }
  public string? Description { get; set; }
  public string? Apply { get; set; }
  public char? Default { get; set; }
}
