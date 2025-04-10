namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockTransferService
{
  Task<bool> AddStockTransferForBatchDelete(string stockCode, string userCode);
  Task<IEnumerable<string>> GetStoreIds();
  Task<IEnumerable<string>> GetRegionIds();
  Task<IEnumerable<string>> GetPriceGroupIds();
  Task<IEnumerable<string>> GetVendorIds();
  Task<StockTransferFilterDto> GetStockTransfersByStoreId(ProductSearchParameterDto productSearchParameterDto);
  Task<StockTransferFilterDto> GetStockTransfersByStoreIds(ProductSearchParameterDto productSearchParameterDto);
  Task<StockTransferFilterDto> GetStockTransfersByPriceGroupIds(ProductSearchParameterDto productSearchParameterDto);
  Task<StockTransferFilterDto> GetStockTransfersByRegionIds(ProductSearchParameterDto productSearchParameterDto);
  Task<StockTransferFilterDto> GetStockTransfersByVendorIds(ProductSearchParameterDto productSearchParameterDto);
}
