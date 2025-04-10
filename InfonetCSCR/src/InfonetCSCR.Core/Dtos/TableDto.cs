namespace InfonetCSCR.Core.Dtos;

public class TableDto
{
  public string? DatabaseName { get; set; }
  public string? TableName { get; set; }
  public IEnumerable<string>? PrimaryKeys { get; set; }
  public IEnumerable<string>? Stores { get; set; }
  public IEnumerable<IDictionary<string, object>>? Values { get; set; }
  public bool IsMasterTable { get; set; }
  public bool Dts {  get; set; }
  public IEnumerable<TableDto>? RelatedTables { get; set; }
  public string? UserCode { get; set; }
  public char? Flag { get; set; }
}
