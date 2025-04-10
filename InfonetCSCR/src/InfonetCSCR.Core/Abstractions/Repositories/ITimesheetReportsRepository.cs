namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITimesheetReportsRepository
{
  Task<IEnumerable<TimeSheetHeader>> GetReports(string status, string? userCode = null);
  Task<IEnumerable<TimeSheetHeader>> GetReportsBySignature(bool not);
  Task<TimesheetHeaderWithUserDetailsDto> GetTimesheetHeaderDetails(int reportId);
  Task<IEnumerable<ShortageDto>> GetShortages();
  Task<IEnumerable<OutstandingStortageDto>> GetOutstandingShortages(string userCode, int reportId, DateTime periodStarting);
  Task<int> UpdateReportIdInShortages(string userCode, int reportId, DateTime shiftDate);
  Task<int> UpdateMissingReportIdInShortages(string userCode, int reportId, DateTime periodStarting);
  Task<int> UpdateUniformPurchases(string userCode, double uniformPurchases, DateTime periodStarting, DateTime periodEnding);
  Task<bool> MissingCheckOutsExist(string userCode, DateTime periodEnding);
  Task<bool> MissingCommentExist(string userCode);
  Task<bool> PostPayPeriod(string userCode, DateTime periodStarting);
  Task<IEnumerable<TimeSheetHeader>> GetTimesheetHeadersForPosting(string userCode, DateTime periodStarting, DateTime periodEnding);
  Task<IEnumerable<ShortageDto>> GetShortagesForPosting(string userCode, DateTime periodStarting, int reportId);
  Task<double> GetCashShortages(string userCode, int reportId);
  Task<double> GetPriorShortages(string userCode, int reportId, DateTime periodStarting);
  Task<double> GetTotalHours(string userCode, int reportId, bool? stat = null);
  Task<IEnumerable<double>> GetOvertimes(string userCode, int reportId);
  Task<double> GetTotalCommissions(string userCode, int reportId);
  Task<IEnumerable<TimesheetDetailDto>> GetTimesheetDetailsBasedOnShiftType(string userCode, int reportId, bool splitShift);
  Task<IEnumerable<TimeSheetMainReportDto>> GetTimeSheetReportDataAsync(long reportId, string userCode);
  Task<IEnumerable<ShortageDto>> GetShortagesReportData(long reportId, string userCode, DateTime shiftDate);
  Task<IEnumerable<TimesheetDetail>> GetTimesheetDetailReportData(long reportId, string userCode);
}
