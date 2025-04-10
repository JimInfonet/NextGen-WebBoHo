namespace InfonetCSCR.Core.Abstractions.Services;

public interface IShiftService
{
  Task<IEnumerable<ShiftRecHeaderDto>> GetShiftIdAndDateByShiftDate(string shiftIDWithDate);
  Task<IList<string>> GetShiftNumberByShiftDate(string shiftDate);
}