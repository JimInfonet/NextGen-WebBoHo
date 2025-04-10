namespace InfonetCSCR.Web.Models;

public class CustomerViewModel
{
  [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "whiteSpaceOnlyNotAllowed")]
  [Required(ErrorMessage = "codeRequired")]
  [MaxLength(10)]
  public string Code { get; set; } = default!;
  [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "whiteSpaceOnlyNotAllowed")]
  [Required(ErrorMessage = "nameRequired")]
  [MaxLength(50)]
  public string? Name { get; set; }
  [MaxLength(4)]
  public string? Category { get; set; }
  [MaxLength(30)]
  public string? Address1 { get; set; }
  [MaxLength(30)]
  public string? Address2 { get; set; }
  [MaxLength(30)]
  public string? City { get; set; }
  [MaxLength(25)]
  public string? Province { get; set; }
  [MaxLength(25)]
  public string? Country { get; set; }
  [MaxLength(15, ErrorMessage = "stringMaxLimit")]
  public string? PostalCode { get; set; }
  [MaxLength(3, ErrorMessage = "stringMaxLimit")]
  public string? AreaCode { get; set; }
  [MaxLength(14, ErrorMessage = "stringMaxLimit")]
  public string? Phone { get; set; }
  [MaxLength(14, ErrorMessage = "stringMaxLimit")]
  public string? OfficePhone { get; set; }
  [MaxLength(14, ErrorMessage = "stringMaxLimit")]
  public string? CellPhone { get; set; }
  [MaxLength(14, ErrorMessage = "stringMaxLimit")]
  public string? Fax { get; set; }
  [MaxLength(14, ErrorMessage = "stringMaxLimit")]
  public string? Toll { get; set; }
  [MaxLength(128)]
  [RegularExpression(@"^[\w\.-]+@[\w\.-]+\.\w+$", ErrorMessage = "invalidEmailFormat")]
  public string? Email { get; set; }
  [MaxLength(14, ErrorMessage = "stringMaxLimit")]
  public string? Contact1 { get; set; }
  [MaxLength(20, ErrorMessage = "stringMaxLimit")]
  public string? Contact2 { get; set; }
  [MaxLength(10, ErrorMessage = "stringMaxLimit")]
  public string? LoyaltyNumber { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public float? LoyaltyPoints { get; set; }
  public bool? ARCustomer { get; set; }
  public string? Type { get; set; }
  public float? Terms { get; set; }
  public int? DiscountDays { get; set; }
  public float? EarlyPayPercent { get; set; }
  [Range(0, 99, ErrorMessage = "discountRange")]
  [MaxLength(2, ErrorMessage = "discountRange")]
  public string? CustomerDiscount { get; set; }
  [Range(0, 9, ErrorMessage = "priceNumberRange")]
  [MaxLength(1, ErrorMessage = "priceNumberRange")]
  public string? PriceCode { get; set; }
  public decimal? HistoricalBalance { get; set; }
  public decimal? HistoricalPaid { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? CreditLimit { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? CurrentBalance { get; set; }
  public string? Status { get; set; }
  [MaxLength(10)]
  public string? ParentAccount { get; set; }
  public bool? UsePurchaseOrder { get; set; }
  [MaxLength(10)]
  public string? GroupID { get; set; }
  public bool? UseFuelRebate { get; set; }
  public bool? UseFuelRebateDiscount { get; set; }
  [MaxLength(250)]
  public string? Note { get; set; }
  public bool? TaxExempt { get; set; }
  [MaxLength(10)]
  public string? PlateNumber { get; set; }
  public bool? MultiUsePurchaseOrder { get; set; }
  public string? CardNumber { get; set; }
  public bool? IsSyncCustInfo { get; set; }
  public string? ProfileID { get; set; }
  [MaxLength(50)]
  [RegularExpression(@"^[\w\.-]+@[\w\.-]+\.\w+$", ErrorMessage = "invalidEmailFormat")]
  public string? StmtEmail { get; set; }
  [MaxLength(10)]
  public string? TECardNumber { get; set; }
  public DateTime? LastPaymentDate { get; set; }
  public DateTime? LastStmtDate { get; set; }
  public DateTime? DateOpen { get; set; }
  public DateTime? LastSaleDate { get; set; }
  public DateTime? HighSaleDate { get; set; }
  [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "maximumTwoDigitsAfterDecimalPoint")]
  [Range(0, 9999999999999999.99, ErrorMessage = "EighteenDigitsDecimalRange")]
  public decimal? HighStmtBalance { get; set; }
  [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "maximumTwoDigitsAfterDecimalPoint")]
  [Range(0, 9999999999999999.99, ErrorMessage = "EighteenDigitsDecimalRange")]
  public decimal? LastStmtBalance { get; set; }
  [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "maximumTwoDigitsAfterDecimalPoint")]
  [Range(0, 9999999999999999.99, ErrorMessage = "EighteenDigitsDecimalRange")]
  public decimal? LastPaymentBalance { get; set; }

  #region checkboxes
  public bool TaxExemptCheckbox
  {
    get => this.TaxExempt.GetValueOrDefault();
    set { this.TaxExempt = value; }
  }
  public bool ARCustomerCheckbox
  {
    get => this.ARCustomer.GetValueOrDefault();
    set { this.ARCustomer = value; }
  }
  public bool UsePurchaseOrderCheckbox
  {
    get => this.UsePurchaseOrder.GetValueOrDefault();
    set { this.UsePurchaseOrder = value; }
  }
  public bool MultiUsePurchaseOrderCheckbox
  {
    get => this.MultiUsePurchaseOrder.GetValueOrDefault();
    set { this.MultiUsePurchaseOrder = value; }
  }
  public bool UseFuelRebateCheckbox
  {
    get => this.UseFuelRebate.GetValueOrDefault();
    set { this.UseFuelRebate = value; }
  }
  public bool UseFuelRebateDiscountCheckbox
  {
    get => this.UseFuelRebateDiscount.GetValueOrDefault();
    set { this.UseFuelRebateDiscount = value; }
  }
  #endregion
}
