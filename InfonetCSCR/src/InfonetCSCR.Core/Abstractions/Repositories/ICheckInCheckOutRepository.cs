namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICheckInCheckOutRepository
{
  Task<IEnumerable<CheckInCheckOutUserDto>> GetCurrentUsersAsync();
  Task<IEnumerable<TimesheetDetailDto>> GetTimeSheetDetails(string userCode, DateTime shiftDate);
  Task<IEnumerable<TimesheetDetailDto>> GetTimeSheetDetailsByReportId(string userCode, int reportId);
  Task<IEnumerable<TimesheetDetailDto>> GetTimeSheetDetailsForLastDay(string userCode);
  Task<IEnumerable<TimesheetDetailDto>> GetTimeSheetDetailsOrderByTimeIn(string userCode);
  Task<TimesheetDetailDto> GetLastTimeSheetDetailsForLastEntry(DateTime shiftDate, string userCode);
  Task<DateTime?> GetLastCheckIn(string userCode);
  Task<TimesheetDetailDto?> GetLastCheckInDetails(string userCode, DateTime shiftDate);
  Task<IEnumerable<DateTime>> GetTimeOutsByShiftDate(string userCode, DateTime shiftDate);
  Task<bool> AddComment(DateTime shiftDate, string comment, string userCode);
  Task<int> GetEmpCommissionRecordCount();
  Task<EmpCommissionData?> GetEmployeeCommissionData(EmpCommissionData model, bool notAll);
  Task UpdateTimesheetDetailsCommissions(double commission, DateTime shiftDate, string userCode);
  Task<IEnumerable<CommissionProduct>> GetAllProducts(DateTime shiftDate, string userCode);
  Task<double?> GetCommissionFromTimeSheet(DateTime shiftDate, string userCode);
  Task<int> ForceCheckOut(TimeSpan timeOut, double hours, DateTime shiftDate, string userCode, TimeSpan timeIn, int reportId);
  Task<TimeSheetPayPeriod?> GetFirstTimeSheetPayPeriod();
  Task<bool> UpdateFirstPayPeriod(TimeSheetPayPeriod model);
  Task<bool> AddNewTimeSheetHeader(TimeSheetPayPeriod model, string userCode);
  Task<string> GetProvince();
  Task<IEnumerable<TimeSheetHolidays>> GetTimeSheetHolidays(DateTime shiftDate, string? province = null);
  Task<IEnumerable<TimeSheetHolidays>> GetTimeSheetHolidaysInRange(DateTime? periodStarting, DateTime? periodEnding, string? province = null);
  Task<bool> InsertTimeSheetDetail(TimesheetDetailDto model);
  Task<int> UpdateTimeInOfTimeSheetDetail(DateTime timeIn, int reportId, DateTime shiftDate, string userCode);
  Task<bool> UpdateTimeSheetDetail(TimesheetDetailDto model, TimeSpan? newTimeIn = null);
  Task<int> UpdateTimeSheetHeader(int reportId, string userCode);
}
