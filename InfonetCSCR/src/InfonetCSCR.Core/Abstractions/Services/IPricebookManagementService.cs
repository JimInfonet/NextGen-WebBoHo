namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPricebookManagementService
{
  Task<IEnumerable<LabelPrintDto>> GetLabelPrintingReportData();
  Task<IEnumerable<LabelPrintDto>> GetLabelPrintingStandardReportData();
}
