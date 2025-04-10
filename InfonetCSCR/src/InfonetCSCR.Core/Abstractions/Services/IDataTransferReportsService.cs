namespace InfonetCSCR.Core.Abstractions.Services;

public interface IDataTransferReportsService
{
  Task<IEnumerable<HotButtonsReportDto>> GetHotButtonsReportData();
  Task<IEnumerable<CommodityTransferReportDto>> GetCommodityTransferReportData();
  Task<IEnumerable<PriceChangeReportDto>> GetPriceChangeReportData();
  Task<IEnumerable<CostChangeReportDto>> GetCostchangeReportData();
  Task<IEnumerable<PromotionalDiscountTransferReportDto>> GetPromotionalDiscountTransferReportData();
  Task<IEnumerable<StockTransferReportDto>> GetStockTransferReportData();
}
