namespace InfonetCSCR.Core.Dtos;

public class CardDefinitionDto
{
  public int CardCode { get; set; } = default!;
  public string? Description { get; set; }
  public string? CardType { get; set; }
  public bool? VerifyCheckDigit { get; set; }
  public bool? Encode { get; set; }
  public bool? Negative { get; set; }
  public bool? Positive { get; set; }
  public bool? Expiry { get; set; }
  public double? LanguageDigit { get; set; }
  public bool? AllowPayment { get; set; }
  public double? MinLength { get; set; }
  public double? MaxLength { get; set; }
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
  public string? BankCardID { get; set; }
  public bool Call { get; set; } = default!;
  public bool Signature { get; set; } = default!;
  public double? DiscountRate { get; set; }
  public string? DiscountType { get; set; }
  public bool RefundAllowed { get; set; } = default!;
  public double? PurchaseLimit { get; set; }
  public double? FloorLimit { get; set; }
  public int? PrintCopies { get; set; }
  public string? ReceiptTotalText { get; set; }
  public bool LimitToSale { get; set; } = default!;
  public double? MaxCashBack { get; set; }
  public bool? AllowPayPump { get; set; }
  public bool? CardProductRestrict { get; set; }
  public bool? PrintVechicleNo { get; set; }
  public bool? PrintDriverNo { get; set; }
  public bool? PrintIdentificationNo { get; set; }
  public bool? PrintOdometer { get; set; }
  public bool? PrintUsage { get; set; }
  public string? OptionalDataProfilID { get; set; }
  public double? TransactionFeeAmount { get; set; }
  public double? TransactionFeePercentage { get; set; }
}
