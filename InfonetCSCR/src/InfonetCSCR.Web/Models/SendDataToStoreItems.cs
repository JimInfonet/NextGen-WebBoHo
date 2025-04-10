namespace InfonetCSCR.Web.Models;

public class SendDataToStoreItems
{
  public string? TableName { get; set; }
  public IEnumerable<string>? PrimaryColumns { get; set; }
  public IEnumerable<string>? DescriptionColumns { get; set; }
  public IEnumerable<IDictionary<string, object>> MasterTableItems { get; set; }
  public IEnumerable<IDictionary<string, object>> SelectedItems { get; set; }
  public IEnumerable<string> SelectedStoreIDs { get; set; }
  public IDictionary<string, string> LocalizedTitle { get; set; }
  public char? Flag { get; set; }
}
