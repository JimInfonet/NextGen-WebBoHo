namespace InfonetCSCR.Web.Models;

public class SearchCriteriaViewModel
{
  public string StockCodeSearchText { get; set; } = default!;
  public string SelectVendor { get; set; } = default!;
  public string DescriptionSearchText { get; set; } = default!;
  public string AlternateCodeSearchText { get; set; } = default!;
  public string KeywordSearchText { get; set; } = default!;
  public string SelectPriceType { get; set; } = default!;
  public string VendorPartSearchText { get; set; } = default!;
  public string SaleItemsOnly { get; set; } = default!;
  public string ScalableItemsOnly { get; set; } = default!;
  public string TaxExemptItemsOnly { get; set; } = default!;
  public string InactiveItemsOnly { get; set; } = default!;
  public string SelectDepartment { get; set; } = default!;
  public string SelectSubDepartment { get; set; } = default!;
  public string SelectSubDetail { get; set; } = default!;
  public bool ShowDetails { get; set; } = false;
  public bool VendorPartSelected { get; set; } = false;
  public int Length { get; set; } //Do not rename. Used for pagination to define number of items per page
  public int Start { get; set; }  //Do not rename. Used for pagination to define index of the first item
}
