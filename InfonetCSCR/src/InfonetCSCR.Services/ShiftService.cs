namespace InfonetCSCR.Services;

public class ShiftService : IShiftService
{
  private readonly IUnitOfWork _unitOfWork;

  public ShiftService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransConnectionString);
  }

  public async Task<IEnumerable<ShiftRecHeaderDto>> GetShiftIdAndDateByShiftDate(string shiftDate)
  {
    IEnumerable<ShiftRecHeaderDto> shiftRecHeaders = await _unitOfWork.Shift.GetShiftIDandDateByShiftDateAsync(shiftDate);
    _unitOfWork.Commit();
    return shiftRecHeaders;
  }

  public async Task<IList<string>> GetShiftNumberByShiftDate(string shiftDate)
  {
    IEnumerable<int> closedShifts = await _unitOfWork.Shift.GetShiftNumbersAsync();
    List<Task<string>> tasks = new();
    foreach (int closeShift in closedShifts)
    {
      tasks.Add(_unitOfWork.Shift.GetShiftNumberByShiftAndDateAsync(shiftDate, closeShift));
    }
    var shiftNumbers = (await Task.WhenAll(tasks)).ToList();
    shiftNumbers.Sort();
    _unitOfWork.Commit();
    return shiftNumbers;
  }
}