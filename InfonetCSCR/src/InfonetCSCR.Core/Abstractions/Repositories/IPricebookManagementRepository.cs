namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPricebookManagementRepository
{
  Task<IEnumerable<LabelPrintDto>?> GetLabelPrintingReportDataAsync();
  Task<IEnumerable<LabelPrintDto>?> GetLabelPrintingStandardReportDataAsync();
}
