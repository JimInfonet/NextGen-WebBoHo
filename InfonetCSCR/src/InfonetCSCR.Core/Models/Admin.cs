namespace InfonetCSCR.Core.Models;

public class Admin
{
  public int Id { get; set; } = default!;
  public string Name { get; set; } = default!;
  public string DisplayName { get; set; } = default!;
  public int DataTypeFK { get; set; } = default!;
  public string Value { get; set; } = default!;
}
