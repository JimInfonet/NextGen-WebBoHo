namespace InfonetCSCR.Core.Models;

public class TierLevelPriceDifference
{
  public string StoreId { get; set; } = default!;
  public int Tier { get; set; } = default!;
  public int Level { get; set; } = default!;
  public double? CashDifference { get; set; }
  public double? CreditDifference { get; set; }
  public double? TaxExemptCashDifference { get; set; }
  public double? TaxExemptCreditDifference { get; set; }
}
