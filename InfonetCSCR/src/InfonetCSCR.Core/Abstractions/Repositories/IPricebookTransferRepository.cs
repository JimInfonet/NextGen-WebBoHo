namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPricebookTransferRepository
{
  Task<int> GetNewBatchId(string dataTransferType);
  Task<IEnumerable<Store>> GetPendingTransferStoresAsync(string dataTransferType);
  Task<int> InsertPriceDefinition(PriceDefinition model);
  Task<int> UpdatePriceDefinition(PriceDefinition model);
  Task<int> InsertPriceDefinitionBatch(List<PriceDefinition> models);
  Task<int> InsertPriceChangeTransfer(PriceChangeTransfer model);
  Task<int> InsertPriceChangeTransferBatch(List<PriceChangeTransfer> models);
  Task<int> InsertStockDefinition(StockDefinition model);
  Task<int> InsertStockDefinitionBatch(List<StockDefinition> models);
  Task<int> InsertCommodityTransferBatch(List<CommodityTransferDto> models, string? userCode);
  Task<IEnumerable<PriceChangeTransferMinimalDto>> GetPriceChangeTransfersAsync(string? storeId);
  Task<int> InsertHotButtonsTransferBatch(List<HotButtonsTransferDto> models, string? userCode);
  Task<int> InsertPromotionTransferBatch(List<PromotionTransfer> models);
  Task<IEnumerable<CommodityTransferDto>> GetCommodityTransfersAsync(string? storeId);
  Task<IEnumerable<HotButtonsTransferDto>> GetHotButtonsTransfersAsync(string? storeId);
  Task<IEnumerable<PromotionTransfer>> GetPromotionTransfersAsync(string? storeId);
  Task<int> DeleteFromTransferList(DeleteFromTransferListDto model);
  Task<IEnumerable<PriceDefinition>> GetPriceDefinitions();
  Task<bool> CheckPriceDefinitionExists(PriceDefinition priceDefinition);
  Task<bool> InsertCostChangeTransferAsync(CostChangeTransfer model);
  Task<int> PerformPriceChangeTransfer(PricebookTransferPoliciesDto policies, string? userCode);
  Task<int> PerformCommodityTransfer();
  Task<int> PerformHotButtonsTransfer();
  Task<int> PerformPromotionTransfer();
  Task<IEnumerable<CostChangeTransferDto>> GetCostChangeTransfersAsync(string? storeId);
  Task<IEnumerable<CostChangeTransferDto>> GetAllCostChangeTransfersAsync();
  Task<int> PerformCostChangeTransfer(PricebookTransferPoliciesDto policies, string? userCode);
  Task<IEnumerable<PriceChangeTransfer>> GetPriceChangeTransfer(PriceChangeTransfer model);
  Task<int> InsertCostChangeTransferBatch(List<CostChangeTransfer> tempCostChangeTransfers);
  Task<int> DeleteLabelPrintingTransferBatch(List<StockItemMinimal> stockItems);
  Task<int> InsertLabelPrintingTransferBatch(List<StockItemMinimal> stockItems);
  Task<bool> DeleteLabelPrintAsync(string? stockCode);
  Task<IEnumerable<StockItemMinimal>?> GetLabelPrintsAsync();
}
