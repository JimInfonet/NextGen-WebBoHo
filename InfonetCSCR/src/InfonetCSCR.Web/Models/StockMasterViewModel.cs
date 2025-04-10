namespace InfonetCSCR.Web.Models;

public class StockMasterViewModel
{
  public string StockCode { get; set; } = default!;
  public string? Description { get; set; }
  public string? StockType { get; set; }
  public int? ProductDiscount { get; set; }
  public decimal? StandardCost { get; set; }
  public decimal? AverageCost { get; set; }
  public decimal? AverageUnits { get; set; }
  public decimal? LateCost { get; set; }
  public string? Department { get; set; }
  public string? DepartmentName { get; set; }
  public string? SubDepartment { get; set; }
  public string? SubDepartmentName { get; set; }
  public string? SubDetail { get; set; }
  public string? SubDetailName { get; set; }
  public bool? Serial { get; set; }
  public bool? SBYWGHT { get; set; }
  public string? Vendor { get; set; }
  public string? PriceType { get; set; }
  public string? PriceUnits { get; set; }
  public DateTime? PriceFrom { get; set; }
  public DateTime? PriceTo { get; set; }
  public string? VendorNumber { get; set; }
  public bool? Availability { get; set; }
  public string? Brand { get; set; }
  public string? Characteristic { get; set; }
  public DateTime? CreationDate { get; set; }
  public string? Family { get; set; }
  public string? Format { get; set; }
  public string? Generic { get; set; }
  public bool? Label { get; set; }
  public string? UnitOfMeasure { get; set; }
  public DateTime? UpdateDate { get; set; }
  public string? OrderUnit { get; set; }
  public int? Packing { get; set; }
  public bool? Transfer { get; set; }
  public bool? EligibleLoyalty { get; set; }
  public bool? EligibleFuelRebate { get; set; }
  public decimal? FuelRebate { get; set; }
  public bool? QualTaxRebate { get; set; }
  public bool? EligibleTaxRebate { get; set; }
  public bool? EligibleTaxExemption { get; set; }
}
