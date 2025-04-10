namespace InfonetCSCR.Web.Models;

public class CardViewModel
{
  public int CardCode { get; set; } = default!;
  public string? Description { get; set; }
  public string? CardType { get; set; }
  public bool? VerifyCheckDigit { get; set; }
  public bool? Encode { get; set; }
  public bool? Negative { get; set; }
  public bool? Positive { get; set; }
  public bool? Expiry { get; set; }
  public int? LanguageDigit { get; set; }
  public bool? AllowPayment { get; set; }
  public int? MinLength { get; set; }
  public int? MaxLength { get; set; }
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
}
