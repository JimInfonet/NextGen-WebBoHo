namespace InfonetCSCR.Services;

public class DataTransferReportsService : IDataTransferReportsService
{
  private readonly IUnitOfWork _unitOfWork;

  public DataTransferReportsService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCTransferConnectionString);
  }

  public async Task<IEnumerable<CommodityTransferReportDto>> GetCommodityTransferReportData()
  {
    IEnumerable<CommodityTransferReportDto> commodityTransferReportData = await _unitOfWork.DataTransferReports.GetCommodityTransferReportDataAsync();
    _unitOfWork.Commit();
    return commodityTransferReportData;
  }

  public async Task<IEnumerable<PriceChangeReportDto>> GetPriceChangeReportData()
  {
    IEnumerable<PriceChangeReportDto> priceChangeReportData = await _unitOfWork.DataTransferReports.GetPriceChangeReportDataAsync();
    _unitOfWork.Commit();
    return priceChangeReportData;
  }

  public async Task<IEnumerable<HotButtonsReportDto>> GetHotButtonsReportData()
  {
    IEnumerable<HotButtonsReportDto> hotButtonsReportData = await _unitOfWork.DataTransferReports.GetHotButtonsReportDataAsync();
    _unitOfWork.Commit();
    return hotButtonsReportData;
  }

  public async Task<IEnumerable<PromotionalDiscountTransferReportDto>> GetPromotionalDiscountTransferReportData()
  {
    IEnumerable<PromotionalDiscountTransferReportDto> promotionalDiscountTransferReportData = await _unitOfWork.DataTransferReports.GetPromotionalDiscountTransferReportDataAsync();
    _unitOfWork.Commit();
    return promotionalDiscountTransferReportData;
  }

  public async Task<IEnumerable<StockTransferReportDto>> GetStockTransferReportData()
  {
    IEnumerable<StockTransferReportDto> stockTransferReport = await _unitOfWork.DataTransferReports.GetStockTransferReportDataAsync();
    _unitOfWork.Commit();
    return stockTransferReport;
  }

  public async Task<IEnumerable<CostChangeReportDto>> GetCostchangeReportData()
  {
    IEnumerable<CostChangeReportDto> costChangeReportData = await _unitOfWork.DataTransferReports.GetCostChangeReportDataAsync();
    _unitOfWork.Commit();
    return costChangeReportData;
  }
}
