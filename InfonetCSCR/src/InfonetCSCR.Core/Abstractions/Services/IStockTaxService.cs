namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockTaxService
{
  Task<bool> InsertStockTaxByStockTaxData(IEnumerable<StockTax> stockTaxes);
  Task<bool> DeleteStockTaxByStockTaxData(IEnumerable<StockTax> stockTaxes);
  Task<double> CalculateTax(double? amount, string? stockCode, string? exptTaxPolicy, int? costDecPolicy);
  Task<double> GetItemsGstRate(string stockCode, string? exptTaxPolicy);
  Task<double> GetItemsHstRate(string stockCode, string? exptTaxPolicy, int? costDecPolicy);
  Task<double> CalculateGST(string exptTaxPolicy, double? extCost, string? stockCode);
  Task<IDictionary<string, IList<double>>> GetTaxRatesByStockCodes(
    string exptTaxPolicy,
    IEnumerable<string> stockCodes,
    bool? taxIncl = null,
    DateTime? purchaseDate = null);
}
