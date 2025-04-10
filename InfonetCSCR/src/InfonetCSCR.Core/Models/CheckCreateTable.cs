namespace InfonetCSCR.Core.Models;

public class CheckCreateTable
{
  public string? DatabaseName { get; set; }
  public string? MasterTableName { get; set; }
  public string? RelatedTables { get; set; }
  public bool Dts { get; set; }
}
