namespace InfonetCSCR.Web.Abstractions.Services;

public interface IPricebookManagementService
{
  Task<IEnumerable<SubDepartmentViewModel>?> GetSubDepartmentsByDepartments(List<string> departments);
  Task<IEnumerable<SubDetailViewModel>?> GetSubDetailsByDepartments(List<string> departments);
  Task<IEnumerable<VendorViewModel>> GetVendorsOrderByColumnNumber(int columnNumber);
}
