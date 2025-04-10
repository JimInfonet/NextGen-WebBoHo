namespace InfonetCSCR.Web.Models;

public class DataTransferRequestResponseViewModel
{
  public bool Dts {  get; set; }
  public IDictionary<string, object>? TableData { get; set; }
  public IEnumerable<StoreOptionViewModel>? StoreOptions { get; set; }
}
