namespace InfonetCSCR.Core.Abstractions.Services;

public interface ISubDepartmentService
{
  Task<bool> CheckSubDepartmentIdExistsForDepartment(string departmentId, string subDepartmentId);
  Task<bool> AddSubDepartment(SubDepartment subDepartment);
  Task<bool> DeleteSubDepartment(string departmentId, string subDepartmentId);
  Task<IEnumerable<SubDepartment>> GetSubDepartmentsListAsync();
  Task<IEnumerable<SubDepartment>> GetSubDepartmentsByDepartments(List<string> departments);
}
