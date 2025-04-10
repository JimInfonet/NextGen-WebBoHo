namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITimesheetReportsService
{
  Task<List<TimeSheetHeader>> GetReports(GetReportsDto model);
  Task<TimesheetHeaderWithUserDetailsDto> GetTimesheetHeaderDetails(int reportId);
  Task<List<OutstandingStortageDto>> GetOutstandingShortages(string userCode, int reportId, DateTime periodStarting);
  Task<bool> UpdateShortages();
  Task<bool> UpdateMissingReportIdInShortages(string userCode, int reportId, DateTime periodStarting);
  Task<List<TimesheetDetailDto>> GetTimeSheetDetailsByReportId(string userCode, int reportId);
  Task<bool> UpdateUniformPurchases(string userCode, double uniformPurchases, DateTime periodStarting, DateTime periodEnding);
  Task<bool> MissingCheckOutsExist(string userCode, DateTime periodEnding);
  Task<bool> MissingCommentExist(string userCode);
  Task<bool> PostPayPeriod(string userCode, DateTime periodStarting);
  Task<GeneratePYFileReturnDto> GetDataToGeneratePYFile(GeneratePYFileDto model);
  Task<UpdateShiftFormReturnDto> UpdateShiftForm(UpdateShiftFormDto model);
  Task<TimeSheetReportDto> GetTimeSheetReportData(long reportId, string userCode, DateTime shiftDate);
}
