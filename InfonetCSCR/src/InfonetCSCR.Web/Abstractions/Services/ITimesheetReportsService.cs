namespace InfonetCSCR.Web.Abstractions.Services;

public interface ITimesheetReportsService
{
  Task<(string contents, string storeCode)> GeneratePYFile(GeneratePYFileDtoViewModel model, CheckInCheckOutPoliciesDtoViewModel policies);
}
