namespace InfonetCSCR.Services;

public class SubDepartmentService : ISubDepartmentService
{
  private readonly IUnitOfWork _unitOfWork;

  public SubDepartmentService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<bool> CheckSubDepartmentIdExistsForDepartment(string departmentId, string subDepartmentId)
  {
    bool isSubDepartmentIdExists = await _unitOfWork.SubDepartments.CheckSubDepartmentIdExistsForDepartmentAsync(departmentId, subDepartmentId);
    _unitOfWork.Commit();
    return isSubDepartmentIdExists;
  }

  public async Task<bool> AddSubDepartment(SubDepartment subDepartment)
  {
    await _unitOfWork.SubDepartments.AddSubDepartmentAsync(subDepartment);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeleteSubDepartment(string departmentId, string subDepartmentId)
  {
    await _unitOfWork.Policy.DeletePSetAsync(departmentId, subDepartmentId);
    await _unitOfWork.StockItems.UpdateDeptSubDeptSubDetailsToNullInStockMasterAsync(departmentId, subDepartmentId);
    await _unitOfWork.SubDepartments.DeleteSubDepartmentAsync(departmentId, subDepartmentId);
    await _unitOfWork.SubDetails.DeleteSubDetailAsync(departmentId, subDepartmentId);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<SubDepartment>> GetSubDepartmentsListAsync()
  {
    var subDepartmentsList = await _unitOfWork.SubDepartments.GetSubDepartmentsListAsync();
    _unitOfWork.Commit();
    return subDepartmentsList;
  }

  public async Task<IEnumerable<SubDepartment>?> GetSubDepartmentsByDepartments(List<string> departments)
  {
    IEnumerable<SubDepartment>? subDepartments = await _unitOfWork.SubDepartments.GetSubDepartmentsByDepartmentsAsync(departments);
    _unitOfWork.Commit();
    return subDepartments;
  }
}
