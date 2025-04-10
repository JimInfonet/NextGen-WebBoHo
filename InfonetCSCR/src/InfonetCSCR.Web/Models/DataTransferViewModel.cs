namespace InfonetCSCR.Web.Models;

public class DataTransferViewModel
{
  public string? DatabaseName { get; set; }
  public string? TableName { get; set; }
  public IEnumerable<string>? PrimaryKeys { get; set; }
  public IEnumerable<string>? Descriptions { get; set; }
  public IEnumerable<string>? References { get; set; }
  public IEnumerable<DataTransferViewModel>? RelatedTables { get; set; }
}
