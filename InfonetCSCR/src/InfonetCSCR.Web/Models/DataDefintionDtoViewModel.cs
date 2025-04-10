namespace InfonetCSCR.Web.Models;

public class DataDefintionDtoViewModel
{
  public IEnumerable<DataDefinitionMinimalViewModel>? DataDefinitions { get; set; }
  public IEnumerable<string>? StoreIds { get; set; }
  public IEnumerable<string>? PriceGroupIds { get; set; }
  public IEnumerable<string>? RegionIds { get; set; }
  public IEnumerable<string>? VendorIds { get; set; }
  public IEnumerable<string>? DefCodes { get; set; }
  public bool? IsAllStoreSelected { get; set; }
  public bool? IsAllProductSelected { get; set; }
  public string? TransferUser { get; set; }
  public string? DefinitionType { get; set; }
  public string? ShowByOption { get; set; }
}
