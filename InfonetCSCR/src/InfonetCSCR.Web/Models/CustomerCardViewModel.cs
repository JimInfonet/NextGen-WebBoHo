namespace InfonetCSCR.Web.Models;

public class CustomerCardViewModel
{
  public bool? AllowRedemption { get; set; }
  [MaxLength(30)]
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public string CardName { get; set; } = default!;
  [MaxLength(30)]
  public string CardNumber { get; set; } = default!;
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public string CardStatus { get; set; } = default!;
  public string? Code { get; set; }
  [Range(-32767, 32767, ErrorMessage = "creditLimitRange")]
  public decimal? CreditLimit { get; set; }
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public DateTime? ExpDate { get; set; }
  public bool? IsEditMode { get; set; }
  [MaxLength(10)]
  [RegularExpression(@"^(?:\d{0}|\d{4,10})$", ErrorMessage = "pinHasToBe4To10NumericValue")]
  public string? PIN { get; set; } = default!;
  public decimal Balance { get; set; } = default!;
  public string? ProfileID { get; set; }
  [MaxLength(16)]
  [RegularExpression(@"^([-+]?\d+(\.\d*)?|[-+]?\.\d+)$", ErrorMessage = "theValueMustBeANumber")]
  public string? TECardNumber { get; set; }
  public string? Sequence { get; set; }
  public bool? IsSyncCustInfo { get; set; }
  public bool? IsArCustomer { get; set; }

  #region checkboxes
  public bool AllowRedemptionCheckbox
  {
    get => this.AllowRedemption.GetValueOrDefault();
    set { this.AllowRedemption = value; }
  }
  #endregion
}
