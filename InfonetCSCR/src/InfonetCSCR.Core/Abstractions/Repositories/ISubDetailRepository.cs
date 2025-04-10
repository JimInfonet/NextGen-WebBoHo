namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ISubDetailRepository
{
  Task<bool> CheckDetailIdExistsForDepartmentAndSubDepartmentAsync(string departmentId, string subDepartmentId, string detailId);
  Task<bool> AddDetailAsync(SubDetail subDetail);
  Task<bool> DeleteSubDetailAsync(string departmentId, string? subDepartmentId = null, string? detailId = null);
  Task<IEnumerable<SubDetail>> GetSubDetailsListAsync();
  Task<IEnumerable<SubDetail>?> GetSubDetailsByDepartmentsAsync(List<string> departments);
}
