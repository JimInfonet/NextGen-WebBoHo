namespace InfonetCSCR.Services;

public class FuelService : IFuelService
{
  private readonly IUnitOfWork _unitOfWork;
  public FuelService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCPumpConnectionString);
  }

  public async Task<bool> CheckStockCodeExistsInGrade(string stockCode)
  {
    bool result = await _unitOfWork.Fuels.CheckStockCodeExistsInGradeAsync(stockCode);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<double> GetFuelPriceByStockCode(string stockCode)
  {
    double result = await _unitOfWork.Fuels.GetFuelPriceByStockCodeAsync(stockCode);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> CheckDepartmentExistsInFuelDept(string departmentId)
  {
    bool isDepartmentExistsInFuelDept = await _unitOfWork.Fuels.CheckDepartmentExistsInFuelDeptAsync(departmentId);
    _unitOfWork.Commit();
    return isDepartmentExistsInFuelDept;
  }
}
