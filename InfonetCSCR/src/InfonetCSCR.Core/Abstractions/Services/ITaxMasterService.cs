namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITaxMasterService
{
  Task<IEnumerable<TaxMasterDto>> GetTaxMasters();
  Task<IEnumerable<TaxMasterDto>> GetStockTaxByStockCodeAndStore(string stockCode, string storeId);
  Task<IEnumerable<ProductTaxDto>> GetProductTaxes();
  Task<IEnumerable<TaxMaster>> GetTaxes();
  Task<bool> AddTax(TaxMaster tax);
  Task<bool> UpdateTax(TaxMaster tax);
  Task<bool> DeleteTax(string taxName);
  Task<bool> TaxNameAlreadyExists(string taxName, string? taxCode = null);
  Task<int> GetTaxUsageCount(string taxName, string? taxCode);
  Task<bool> DeleteTaxRate(string taxName, string? taxCode = null);
  Task<IEnumerable<TaxMaster>> GetTaxesOrderByTaxNameAsync();
}
