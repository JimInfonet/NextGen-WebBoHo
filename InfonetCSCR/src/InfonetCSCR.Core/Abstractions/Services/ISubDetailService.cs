namespace InfonetCSCR.Core.Abstractions.Services;

public interface ISubDetailService
{
  Task<bool> CheckDetailIdExistsForDepartmentAndSubDepartment(string departmentId, string subDepartmentId, string detailId);
  Task<bool> AddDetail(SubDetail subDetail);
  Task<bool> DeleteDetail(string departmentId, string subDepartmentId, string detailId);
  Task<IEnumerable<SubDetail>> GetSubDetailsListAsync();
  Task<IEnumerable<SubDetail>> GetSubDetailsByDepartments(List<string> departments);
}
