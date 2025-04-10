namespace InfonetCSCR.Web.Models;

public class ProductViewModel
{
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [StringLength(16)]
  public string StockCode { get; set; } = default!;
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [MaxLength(35)]
  public string? Description { get; set; }
  public string? StockType { get; set; }
  public int? ProductDiscount { get; set; }
  public decimal? StandardCost { get; set; }
  public decimal? AverageCost { get; set; }
  public decimal? AverageUnits { get; set; }
  public decimal? LateCost { get; set; }
  public string? Department { get; set; }
  public string? SubDepartment { get; set; }
  public string? SubDetail { get; set; }
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
  [MaxLength(6)]
  public string? Format { get; set; }
  public string? Generic { get; set; }
  public bool? Label { get; set; }
  public string? UnitOfMeasure { get; set; }
  public DateTime? UpdateDate { get; set; }
  public string? OrderUnit { get; set; }
  [RequiredIfPolicy(PolicyName.PACKCOST, ErrorMessage = "enterValidPackingNumber")]
  [MinPolicy(PolicyName.PACKCOST, 1, ErrorMessage = "inputaNumericValueGreaterThanZero")]
  [Range(0, 32767, ErrorMessage = "inputaNumericValueLessThanMax")]
  public int? Packing { get; set; }
  public bool? Transfer { get; set; }
  public bool? EligibleLoyalty { get; set; }
  public bool? EligibleFuelRebate { get; set; }
  public decimal? FuelRebate { get; set; }
  public bool? QualTaxRebate { get; set; }
  public bool? EligibleTaxRebate { get; set; }
  public bool? EligibleTaxExemption { get; set; }
  public string? Branch { get; set; }
  [Range(0, 99999)]
  public decimal? MaxQty { get; set; }
  [Range(0, 99999)]
  public decimal? ReOrderQty { get; set; }
  public decimal? AvailableQty { get; set; }
  [Range(0, 99999)]
  public decimal? MaxQtyTE { get; set; }
  [Range(0, 99999)]
  public decimal? ReOrderQtyTE { get; set; }
  public decimal? AvailableQtyTE { get; set; }

  #region checkboxes
  public bool AvailabilityCheckBox
  {
    get => this.Availability.GetValueOrDefault();
    set { this.Availability = value; }
  }
  public bool LabelCheckBox
  {
    get => this.Label.GetValueOrDefault();
    set { this.Label = value; }
  }
  public bool EligibleLoyaltyCheckBox
  {
    get => this.EligibleLoyalty.GetValueOrDefault();
    set { this.EligibleLoyalty = value; }
  }
  public bool EligibleTaxExemptionCheckBox
  {
    get => this.EligibleTaxExemption.GetValueOrDefault();
    set { this.EligibleTaxExemption = value; }
  }
  public bool EligibleFuelRebateCheckBox
  {
    get => this.EligibleFuelRebate.GetValueOrDefault();
    set { this.EligibleFuelRebate = value; }
  }
  public bool QualifiedTaxRebateCheckBox
  {
    get => this.QualTaxRebate.GetValueOrDefault();
    set { this.QualTaxRebate = value; }
  }
  public bool EligibleTaxRebateCheckBox
  {
    get => this.EligibleTaxRebate.GetValueOrDefault();
    set { this.EligibleTaxRebate = value; }
  }
  public bool SBYWGHTCheckBox
  {
    get => this.SBYWGHT.GetValueOrDefault();
    set { this.SBYWGHT = value; }
  }
  #endregion
}
