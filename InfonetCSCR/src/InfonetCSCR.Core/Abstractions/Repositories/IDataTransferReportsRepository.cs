namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IDataTransferReportsRepository
{
  Task<IEnumerable<HotButtonsReportDto>> GetHotButtonsReportDataAsync();
  Task<IEnumerable<CommodityTransferReportDto>> GetCommodityTransferReportDataAsync();
  Task<IEnumerable<PriceChangeReportDto>> GetPriceChangeReportDataAsync();
  Task<IEnumerable<CostChangeReportDto>> GetCostChangeReportDataAsync();
  Task<IEnumerable<PromotionalDiscountTransferReportDto>> GetPromotionalDiscountTransferReportDataAsync();
  Task<IEnumerable<StockTransferReportDto>> GetStockTransferReportDataAsync();
}
