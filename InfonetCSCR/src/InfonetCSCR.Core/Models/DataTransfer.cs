namespace InfonetCSCR.Core.Models;

public class DataTransfer
{
  public string? DatabaseName { get; set; }
  public string? TableName { get; set; }
  public List<string>? Keys { get; set; }
  public List<string>? Descriptions { get; set; }
  public List<string>? References { get; set; }
  public List<DataTransfer>? RelatedTables { get; set; }
}
