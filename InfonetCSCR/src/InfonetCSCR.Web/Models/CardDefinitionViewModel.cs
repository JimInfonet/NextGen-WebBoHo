namespace InfonetCSCR.Web.Models;

public class CardDefinitionViewModel
{
  public int CardCode { get; set; } = default!;
  public string? Description { get; set; }
  public string? CardType { get; set; }
  public bool? VerifyCheckDigit { get; set; }
  public bool? Encode { get; set; }
  public bool? Negative { get; set; }
  public bool? Positive { get; set; }
  public bool? Expiry { get; set; }
  [Range(-2147483648, 2147483647, ErrorMessage = "languageDigitPositionRange")]
  public double? LanguageDigit { get; set; }
  public bool? AllowPayment { get; set; }
  [Range(0, 255, ErrorMessage = "maxLengthRange")]
  public double? MinLength { get; set; }
  [Range(0, 255, ErrorMessage = "minLengthRange")]
  public double? MaxLength { get; set; }
  [RegularExpression("^(?!.*[Cc].*[Cc])[012]*[Cc]?[012]*$", ErrorMessage = "checkDigitMaskError")]
  [MaxLength(50)]
  public string? CheckMask { get; set; }
  public int? UsageIDPosition { get; set; }
  public int? UsageIDLength { get; set; }
  public int? PromptIDPosition { get; set; }
  public int? PromptIDLength { get; set; }
  public string? DefaultPromptCode { get; set; }
  public string? DefaultUsageCode { get; set; }
  public bool? UserEnteredRestriction { get; set; }
  public int? ProductPerTransaction { get; set; }
  public string? ServiceTypeCode { get; set; }
  public bool? VerifyThirdPartySw { get; set; }
  public bool? VerifyCardNumber { get; set; }
  public string? GiftType { get; set; }
  public bool? EncryptCardNumber { get; set; }
  public bool AskForDriverID { get; set; } = default!;
  public string? TendCode { get; set; }
  public int? ReportGroup { get; set; }
  [Required(ErrorMessage = "pleaseInputBankCardID")]
  [MaxLength(50, ErrorMessage = "maximumLengthOfBankCardID")]
  public string? BankCardID { get; set; }
  public bool Call { get; set; } = default!;
  public bool Signature { get; set; } = default!;
  [Range(-214748.3648, 214748.3647, ErrorMessage = "discRateRange")]
  public double? DiscountRate { get; set; }
  [MaxLength(1)]
  public string? DiscountType { get; set; }
  public bool RefundAllowed { get; set; } = default!;
  [Range(-214748.3648, 214748.3647, ErrorMessage = "purchaseLimitRange")]
  public double? PurchaseLimit { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "floorLimitRange")]
  public double? FloorLimit { get; set; }
  [Range(0, 255, ErrorMessage = "printCopiesRange")]
  public int? PrintCopies { get; set; }
  [MaxLength(50)]
  public string? ReceiptTotalText { get; set; }
  public bool LimitToSale { get; set; } = default!;
  [Range(-214748.3648, 214748.3647, ErrorMessage = "maxCashBackRange")]
  public double? MaxCashBack { get; set; }
  public bool? AllowPayPump { get; set; }
  public bool? CardProductRestrict { get; set; }
  public bool? PrintVechicleNo { get; set; }
  public bool? PrintDriverNo { get; set; }
  public bool? PrintIdentificationNo { get; set; }
  public bool? PrintOdometer { get; set; }
  public bool? PrintUsage { get; set; }
  [MaxLength(10)]
  public string? OptionalDataProfilID { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "transactionFeeRange")]
  public double? TransactionFeeAmount { get; set; }
  [Range(0, 100, ErrorMessage = "percentageValueRange")]
  public double? TransactionFeePercentage { get; set; }

  #region checkboxes

  public bool VerifyCheckDigitCheckBox
  {
    get => this.VerifyCheckDigit.GetValueOrDefault();
    set { this.VerifyCheckDigit = value; }
  }

  public bool EncodeCheckBox
  {
    get => this.Encode.GetValueOrDefault();
    set { this.Encode = value; }
  }

  public bool NegativeCheckBox
  {
    get => this.Negative.GetValueOrDefault();
    set { this.Negative = value; }
  }

  public bool PositiveCheckBox
  {
    get => this.Positive.GetValueOrDefault();
    set { this.Positive = value; }
  }

  public bool ExpiryCheckBox
  {
    get => this.Expiry.GetValueOrDefault();
    set { this.Expiry = value; }
  }

  public bool AllowPaymentCheckBox
  {
    get => this.AllowPayment.GetValueOrDefault();
    set { this.AllowPayment = value; }
  }

  public bool UserenteredrestrictionCheckBox
  {
    get => this.UserEnteredRestriction.GetValueOrDefault();
    set { this.UserEnteredRestriction = value; }
  }

  public bool VerifyThirdPartySwCheckBox
  {
    get => this.VerifyThirdPartySw.GetValueOrDefault();
    set { this.VerifyThirdPartySw = value; }
  }

  public bool VerifyCardNumberCheckBox
  {
    get => this.VerifyCardNumber.GetValueOrDefault();
    set { this.VerifyCardNumber = value; }
  }

  public bool EncryptCardNumberCheckBox
  {
    get => this.EncryptCardNumber.GetValueOrDefault();
    set { this.EncryptCardNumber = value; }
  }

  public bool AskForDriverIDCheckBox
  {
    get => this.AskForDriverID;
    set { this.AskForDriverID = value; }
  }

  public bool CallCheckBox
  {
    get => this.Call;
    set { this.Call = value; }
  }

  public bool SignatureCheckBox
  {
    get => this.Signature;
    set { this.Signature = value; }
  }

  public bool RefundAllowedCheckBox
  {
    get => this.RefundAllowed;
    set { this.RefundAllowed = value; }
  }

  public bool LimitToSaleCheckBox
  {
    get => this.LimitToSale;
    set { this.LimitToSale = value; }
  }

  public bool AllowPayPumpCheckBox
  {
    get => this.AllowPayPump.GetValueOrDefault();
    set { this.AllowPayPump = value; }
  }

  public bool CardProductRestrictCheckBox
  {
    get => this.CardProductRestrict.GetValueOrDefault();
    set { this.CardProductRestrict = value; }
  }

  public bool PrintVechicleNoCheckBox
  {
    get => this.PrintVechicleNo.GetValueOrDefault();
    set { this.PrintVechicleNo = value; }
  }

  public bool PrintDriverNoCheckBox
  {
    get => this.PrintDriverNo.GetValueOrDefault();
    set { this.PrintDriverNo = value; }
  }

  public bool PrintIdentificationNoCheckBox
  {
    get => this.PrintIdentificationNo.GetValueOrDefault();
    set { this.PrintIdentificationNo = value; }
  }

  public bool PrintOdometerCheckBox
  {
    get => this.PrintOdometer.GetValueOrDefault();
    set { this.PrintOdometer = value; }
  }

  public bool PrintUsageCheckBox
  {
    get => this.PrintUsage.GetValueOrDefault();
    set { this.PrintUsage = value; }
  }

  #endregion

}

