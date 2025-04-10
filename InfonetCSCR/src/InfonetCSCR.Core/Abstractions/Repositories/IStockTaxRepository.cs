namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStockTaxRepository
{
  Task<bool> AddStockTaxByStockCodeAsync(string stockCode, string baseStockCode);
  Task<bool> AddStockTaxAsync(string stockCode, string taxName, string taxCode, string storeId);
  Task<bool> DeleteStockTaxByStockCodeAsync(string stockCode);
  Task<bool> DeleteStockTaxByStockCodeAndStoreIdAsync(string stockCode, string storeId);
  Task<bool> InsertStockTaxByStockTaxDataAsync(IEnumerable<StockTax> stockTaxes);
  Task<bool> DeleteStockTaxByStockTaxDataAsync(IEnumerable<StockTax> stockTaxes);
  Task<IEnumerable<StockTax>> GetStockTaxesByStockCodeAsync(string stockCode);
  Task<IEnumerable<StockTax>> GetStockTaxesByStockCodesAsync(IEnumerable<string> stockCodes);
}
