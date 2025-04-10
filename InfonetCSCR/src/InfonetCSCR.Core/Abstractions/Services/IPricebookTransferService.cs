namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPricebookTransferService
{
  Task<IEnumerable<Store>> GetPendingTransferStores(string dataTransferType);
  Task<int> InsertPriceDefinition(PriceDefinition model);
  Task<int> InsertPriceDefinitionBatch(List<PriceDefinition> models);
  Task<int> InsertPriceChangeTransfer(PriceChangeTransfer model);
  Task<int> InsertPriceChangeTransferBatch(List<PriceChangeTransfer> models);
  Task<int> PendingTransfers(PendingTransferDto model);
  Task<int> StockPendingTransfers(PendingTransferDto model);
  Task<int> PendingCommodityTransfers(PendingCommodityTransferDto model, string? userCode);
  Task<IEnumerable<PriceChangeTransferMinimalDto>> GetPriceChangeTransfers(string? storeId);
  Task<int> PendingHotButtonsTransfers(PendingHotButtonsTransferDto model, string? userCode);
  Task<int> PendingPromotionTransfers(PendingPromotionTransferDto model, string? userCode);
  Task<IEnumerable<CommodityTransferDto>> GetCommodityTransfers(string? storeId);
  Task<IEnumerable<HotButtonsTransferDto>> GetHotButtonsTransfers(string? storeId);
  Task<IEnumerable<PromotionTransfer>> GetPromotionTransfers(string? storeId);
  Task<int> PerformTransfer(string dataTransferType, PricebookTransferPoliciesDto policies, string? userCode);
  Task<int> StockTransfer();
  Task<int> DeleteFromTransferList(DeleteFromTransferListDto model);
  Task<IEnumerable<CostChangeTransferDto>> GetCostChangeTransfers(string? storeId);
  Task<IEnumerable<StockItemMinimal>?> GetLabelPrints();
  Task<bool> DeleteLabelPrint(string? stockCode);
}
