namespace InfonetCSCR.Core.Models;

public class ProductTaxExempt
{
  public string ProductKey { get; set; } = default!;
  public string? UpcCode { get; set; }
  public int CategoryFK { get; set; }
  public double TaxFreePrice { get; set; }
  public int UnitsPerPkg { get; set; }
  public string EmplID { get; set; } = default!;
  public string? LastUpdateTime { get; set; }
  public int? TaxCode { get; set; }
  public string? ProductCode { get; set; }
  public decimal? InStock { get; set; }
  public decimal? Available { get; set; }
  public decimal? Hold { get; set; }
  public decimal? Waste { get; set; }
  public decimal? ReorderQuantity { get; set; }
  public string? LastSaleTime { get; set; }
  public int? QuantityPerPkg { get; set; }
  public int? BaseUnitQty { get; set; }
  public decimal? MaxQty { get; set; }
  public string? TEVendor { get; set; }
}
