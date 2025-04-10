namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITimesheetPayPeriodRepository
{
  Task<int> GetInitialEmployeeIdAsync();
  Task<bool> IncrementInitialEmployeeId(int updatedInitialEmployeeId);
}
