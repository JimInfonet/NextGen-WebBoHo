namespace InfonetCSCR.Services;

public class PricebookManagementService : IPricebookManagementService
{
  private readonly IUnitOfWork _unitOfWork;

  public PricebookManagementService(IApplicationConfig config)
  {
    _unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<LabelPrintDto>?> GetLabelPrintingReportData()
  {
    IEnumerable<LabelPrintDto>? stockItems = await _unitOfWork.PricebookManagement.GetLabelPrintingReportDataAsync();
    _unitOfWork.Commit();
    return stockItems;
  }

  public async Task<IEnumerable<LabelPrintDto>?> GetLabelPrintingStandardReportData()
  {
    IEnumerable<LabelPrintDto>? stockItems = await _unitOfWork.PricebookManagement.GetLabelPrintingStandardReportDataAsync();
    _unitOfWork.Commit();
    return stockItems;
  }
}
