namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IFuelRepository
{
  Task<bool> CheckStockCodeExistsInGradeAsync(string stockCode);
  Task<double> GetFuelPriceByStockCodeAsync(string stockCode);
  Task<bool> CheckDepartmentExistsInFuelDeptAsync(string departmentId);
  Task<string?> GetFuelDepartmentAsync();
}
