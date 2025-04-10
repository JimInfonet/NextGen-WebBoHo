namespace InfonetCSCR.Web.Models;

public class ProductSearchParameterViewModel
{
  public IEnumerable<string>? StoreIds { get; set; }
  public IEnumerable<string>? RegionIds { get; set; }
  public IEnumerable<string>? PriceGroupIds { get; set; }
  public IEnumerable<string>? VendorIds { get; set; }
  public int? Length { get; set; } //Do not rename. Used for pagination to define number of items per page
  public int? Start { get; set; } //Do not rename. Used for pagination to define number of items per page
  public bool? IsAllProductSelected { get; set; }
}
