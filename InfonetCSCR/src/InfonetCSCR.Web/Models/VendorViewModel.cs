namespace InfonetCSCR.Web.Models;

public class VendorViewModel
{
  [Required(ErrorMessage = "enterMaxTenCharacter")]
  [MaxLength(10)]
  public string Code { get; set; } = default!;
  public string? Address { get; set; }
  public string? City { get; set; }
  public string? Province { get; set; }
  public string? Postal { get; set; }
  public string? Phone { get; set; }
  public string? Fax { get; set; }
  public string? CustAccount { get; set; }

  [Required(ErrorMessage = "enterMaxThirtyCharacter")]
  [MaxLength(30)]
  public string Name { get; set; } = default!;
  public string? Email { get; set; }
  public string? Country { get; set; }
  public string? ContactName1 { get; set; }
  public string? ContactName2 { get; set; }
  public string? TaxNumber { get; set; }
  public bool? IsTEVendor { get; set; }
  public bool? Active { get; set; }

  [RegularExpression(@"^[+-]?\d*\.?\d+$", ErrorMessage = "theValueMustBeANumber")]
  public float? OrderFreq { get; set; }

  [RegularExpression(@"^[+-]?\d*\.?\d+$", ErrorMessage = "theValueMustBeANumber")]
  public float? PoDiscount { get; set; }
  public bool? PoCostTaxInclude { get; set; }

  #region checkboxes
  public bool IsTEVendorCheckBox
  {
    get => this.IsTEVendor.GetValueOrDefault();
    set { this.IsTEVendor = value; }
  }

  public bool ActiveCheckBox
  {
    get => this.Active.GetValueOrDefault();
    set { this.Active = value; }
  }

  public bool PoCostTaxIncludeCheckBox
  {
    get => this.PoCostTaxInclude.GetValueOrDefault();
    set { this.PoCostTaxInclude = value; }
  }
  #endregion
}
