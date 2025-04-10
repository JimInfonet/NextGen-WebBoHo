namespace InfonetCSCR.Core.Records;

public record PurchaseDetailUpdateParameterRecord
{
  public PurchaseDetail? PurchaseDetail { get; set; }
  public IDictionary<string, IList<double>> GetTaxRatesByStockCodesForGSTDictionary { get; set; }
  public IDictionary<string, IList<double>> GetTaxRatesByStockCodesForHSTDictionary { get; set; }
  public IDictionary<string, IList<double>> GetTaxRatesByStockCodesDictionary { get; set; }
  public IDictionary<string, double> GetChargeByPartNoDictionary { get; set; }
  public IDictionary<string, StockCostInfo> StockCostInfoDictionary { get; set; }
  public PurchasingAndReceivingPoliciesDto PurchasingAndReceivingPoliciesDto { get; set; }
  public double PoDiscount { get; set; }
  public bool IsTaxIncluded { get; set; }
  public int FieldDataUpdated { get; set; }
}
