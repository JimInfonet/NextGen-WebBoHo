namespace InfonetCSCR.Core.Records;

public record TaxAmountCalculatorDataRecord
{
  public double PoDiscount { get; set; }
  public bool IsTaxIncluded { get; set; }
  public PurchaseDetail PurchaseDetail { get; set; }
  public PurchasingAndReceivingPoliciesDto PurchasingAndReceivingPolicies { get; set; }
  public IDictionary<string, IList<double>>? GetTaxRatesByStockCodesForGSTDictionary { get; set; }
  public IDictionary<string, IList<double>>? GetTaxRatesByStockCodesForHSTDictionary { get; set; }
  public IDictionary<string, IList<double>>? GetTaxRatesByStockCodesDictionary { get; set; }
}
