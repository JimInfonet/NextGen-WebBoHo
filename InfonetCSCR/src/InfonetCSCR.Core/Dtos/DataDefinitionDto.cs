
namespace InfonetCSCR.Core.Dtos;

public class DataDefinitionDto
{
  public IEnumerable<DataDefinitionMinimalDto>? DataDefinitions { get; set; }
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
