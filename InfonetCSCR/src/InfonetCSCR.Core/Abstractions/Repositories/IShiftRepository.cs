namespace InfonetCSCR.Core.Abstractions.Repositories;
public interface IShiftRepository
{
  Task<IEnumerable<ShiftRecHeaderDto>> GetShiftIDandDateByShiftDateAsync(string shiftDate);
  Task<string> GetShiftNumberByShiftAndDateAsync(string shiftDate, int shift);
  Task<IEnumerable<int>> GetShiftNumbersAsync();
  Task<bool> UpdateRepostedByShiftNumberAndShiftIDAsync(int? shiftID, int? shiftNumber);
}