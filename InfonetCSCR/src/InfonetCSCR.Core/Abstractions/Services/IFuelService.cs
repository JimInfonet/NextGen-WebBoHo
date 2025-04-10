namespace InfonetCSCR.Core.Abstractions.Services;

public interface IFuelService
{
  Task<bool> CheckStockCodeExistsInGrade(string stockCode);
  Task<double> GetFuelPriceByStockCode(string stockCode);
  Task<bool> CheckDepartmentExistsInFuelDept(string departmentId);
}
