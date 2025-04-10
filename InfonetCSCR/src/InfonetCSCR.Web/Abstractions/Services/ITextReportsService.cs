namespace InfonetCSCR.Web.Abstractions.Services;

public interface ITextReportsService
{
  Task<SelectList> GetAllTextReportsListAsync();
  Task<SelectList> GetAllStoresListAsync();
  Task<string> GetTextReport(TextReportParamsViewModel textReportParams);
}
