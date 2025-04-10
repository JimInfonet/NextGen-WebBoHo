namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITaxMasterRepository
{
  Task<IEnumerable<TaxMasterDto>> GetTaxMastersAsync();
  Task<IEnumerable<TaxMasterDto>> GetStockTaxByStockCodeAndStoreAsync(string stockCode, string storeId);
  Task<IEnumerable<ProductTaxDto>> GetProductTaxesAsync();
  Task<IEnumerable<double>> GetProductTaxRateByTaxCodeAndTaxNameAsync(string taxCode, string taxName, bool? taxIncl = null);
  Task<IEnumerable<TaxMasterDto>> GetProductTaxeRateDataByTaxCodesAndTaxNamesAsync(
        IList<(string taxCode, string taxName)> taxCodeTaxNamePairs, bool? taxIncl = null);
  Task<IEnumerable<TaxMaster>> GetTaxesAsync();
  Task<bool> AddTaxAsync(TaxMaster tax);
  Task<bool> UpdateTaxAsync(TaxMaster tax);
  Task<bool> DeleteTaxAsync(string taxName);
  Task<bool> DeleteTaxRateAsync(string taxName, string? taxCode = null);
  Task<bool> DeleteStockTaxAsync(string taxName, string? taxCode = null);
  Task<bool> DeleteAssocTaxAsync(string taxName, string? taxCode = null);
  Task<bool> TaxNameAlreadyExistsAsync(string taxName, string? taxCode);
  Task<int> GetTaxUsageCountAsync(string taxName, string? taxCode = null);
  Task<IEnumerable<TaxMaster>> GetTaxesOrderByTaxNameAsync();
}
