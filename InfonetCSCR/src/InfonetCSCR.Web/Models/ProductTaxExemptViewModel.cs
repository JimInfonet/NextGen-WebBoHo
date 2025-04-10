namespace InfonetCSCR.Web.Models;

public class ProductTaxExemptViewModel
{
  public string ProductKey { get; set; } = default!;
  public string? UpcCode { get; set; }
  [Required(ErrorMessage = "taxExemptCategoryRequired")]
  public int CategoryFK { get; set; }
  [Required(ErrorMessage = "taxFreePriceRequired")]
  [Range(-214748.3647, 214748.3647, ErrorMessage = "taxFreePriceRange")]
  public double TaxFreePrice { get; set; }
  [Required(ErrorMessage = "unitsPerPkgRequired")]
  [Range(1, 999999, ErrorMessage = "unitsPerPkgRange")]
  public int UnitsPerPkg { get; set; }
  public string EmplID { get; set; } = default!;
  public string? LastUpdateTime { get; set; }
  public int? TaxCode { get; set; }
  [MaxLength(3, ErrorMessage = "productCodeMaxLength")]
  public string? ProductCode { get; set; }
  public decimal? InStock { get; set; }
  public decimal? Available { get; set; }
  public decimal? Hold { get; set; }
  public decimal? Waste { get; set; }
  public decimal? ReorderQuantity { get; set; }
  public string? LastSaleTime { get; set; }
  [Required(ErrorMessage = "quantityPerPkgRequired")]
  [Range(1, 32767, ErrorMessage = "quantityPerPkgRange")]
  public int? QuantityPerPkg { get; set; }
  [Required(ErrorMessage = "baseUnitQuantityRequired")]
  [Range(1, 32767, ErrorMessage = "baseUnitQuantityRange")]
  public int? BaseUnitQty { get; set; }
  public decimal? MaxQty { get; set; }
  public string? TEVendor { get; set; }
  public string? Description { get; set; }
}
