namespace InfonetCSCR.Core.Abstractions.Repositories;
public interface ICategoryRepository
{
  Task<IEnumerable<Department>> GetDepartmentsAsync();
  Task<IEnumerable<SubDepartment>> GetSubDepartmentsAsync(string department = "");
  Task<IEnumerable<SubDetail>> GetSubDetailsAsync(string department = "", string subDepartment = "");
  Task<bool> CheckDepartmentExistsByEodGroupIdAsync(string eodGroupId);
  Task<bool> MakeEODGroupNullInDeptAsync(string eodGroupId);
  Task<bool> UpdateEODGroupByDeptAsync(string eodGroupId, string dept);
  Task<bool> UpdateEodDetailByDeptAsync(string eodDetail, string dept);
  Task<bool> UpdateEodCountDetailByDeptAsync(string eodCountDetail, string dept);
  Task<bool> AddDepartmentAsync(Department department);
  Task<bool> CheckDepartmentIdExistsAsync(string departmentId);
  Task<bool> DeleteCategoryByDepartmentIdAsync(string departmentId);
  Task<IEnumerable<ClCategory>> GetClCategoryListAsync();
  Task<IEnumerable<DiscCode>> GetDiscCodesListAsync();
  Task<IEnumerable<Branch>> GetBranchesListAsync();
  Task<IEnumerable<UnitOfMeasure>> GetUomListAsync();
}
