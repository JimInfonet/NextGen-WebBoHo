namespace InfonetCSCR.Web.Models;

public class TenderCardViewModel
{
  public string TendCode { get; set; } = default!;
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
  public int? CardCode { get; set; }
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
