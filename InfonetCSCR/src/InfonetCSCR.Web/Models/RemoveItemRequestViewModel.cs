namespace InfonetCSCR.Web.Models;

public class RemoveItemRequestViewModel
{
  public string? TableName { get; set; }
  public IEnumerable<IDictionary<string, object>>? Items { get; set; }
}
