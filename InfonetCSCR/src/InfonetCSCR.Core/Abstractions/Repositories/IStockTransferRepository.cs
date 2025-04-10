namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStockTransferRepository
{
  Task<int> GetBatchId();
  Task<IEnumerable<string>> GetStoreIdsAsync();
  Task<IEnumerable<string>> GetRegionIdsAsync();
  Task<IEnumerable<string>> GetPriceGroupIdsAsync();
  Task<IEnumerable<string>> GetVendorIdsAsync();
  Task<IEnumerable<StockTransferDto>> GetStockTransfersByStoreIdsAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<int> CountStockTransfersByStoreIdsAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<IEnumerable<StockTransferDto>> GetStockTransfersByRegionIdsAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<int> CountStockTransfersByRegionIdsAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<IEnumerable<StockTransferDto>> GetStockTransfersByPriceGroupIdsAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<int> CountStockTransfersByPriceGroupIdsAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<IEnumerable<StockTransferDto>> GetStockTransfersByVendorIdsAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<int> CountStockTransfersByVendorIdsAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<bool> AddStockTransferForBatchDeleteAsync(string stockCode, string userCode);
  Task<IEnumerable<StockTransferDto>> GetStockTransfersByStoreIdAsync(ProductSearchParameterDto productSearchParameterDto);
  Task<int> CountStockTransfersByStoreIdAsync(string storeId);
  Task<bool> UpdateStockTransferFlagAsync();
}
