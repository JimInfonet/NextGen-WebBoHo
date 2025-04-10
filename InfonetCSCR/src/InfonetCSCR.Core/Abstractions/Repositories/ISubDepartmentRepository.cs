namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ISubDepartmentRepository
{
  Task<bool> CheckSubDepartmentIdExistsForDepartmentAsync(string departmentId, string subDepartmentId);
  Task<bool> AddSubDepartmentAsync(SubDepartment subDepartment);
  Task<bool> DeleteSubDepartmentAsync(string departmentId, string? subDepartmentId = null);
  Task<IEnumerable<SubDepartment>> GetSubDepartmentsListAsync();
  Task<IEnumerable<SubDepartment>?> GetSubDepartmentsByDepartmentsAsync(List<string> departments);
}
