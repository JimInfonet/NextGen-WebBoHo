namespace InfonetCSCR.Core.Abstractions.Services;
public interface ICategoryService
{
  Task<IEnumerable<Department>> GetDepartmentsAsync();
  Task<IEnumerable<SubDepartment>> GetSubDepartmentsAsync(string department = "");
  Task<IEnumerable<SubDetail>> GetSubDetailsAsync(string department = "", string subDepartment = "");
  Task<bool> CheckDepartmentExistsByEodGroupId(string eodGroupId);
  Task<bool> UpdateEodDetailByDept(string eodDetail, string dept);
  Task<bool> UpdateEodCountDetailByDept(string eodCountDetail, string dept);
  Task<bool> AddDepartment(Department department);
  Task<bool> CheckDepartmentIdExists(string departmentId);
  Task<bool> DeleteCategoryByDepartmentId(string departmentId);
  Task<IEnumerable<ClCategory>> GetClCategoryListAsync();
  Task<IEnumerable<DiscCode>> GetDiscCodesListAsync();
  Task<IEnumerable<Branch>> GetBranchesListAsync();
  Task<IEnumerable<UnitOfMeasure>> GetUomListAsync();
}
