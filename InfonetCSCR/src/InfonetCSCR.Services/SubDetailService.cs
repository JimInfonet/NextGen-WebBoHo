namespace InfonetCSCR.Services;

public class SubDetailService : ISubDetailService
{
  private readonly IUnitOfWork _unitOfWork;

  public SubDetailService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<bool> CheckDetailIdExistsForDepartmentAndSubDepartment(string departmentId, string subDepartmentId, string detailId)
  {
    bool isDetailIdExists = await _unitOfWork.SubDetails.CheckDetailIdExistsForDepartmentAndSubDepartmentAsync(departmentId, subDepartmentId, detailId);
    _unitOfWork.Commit();
    return isDetailIdExists;
  }

  public async Task<bool> AddDetail(SubDetail subDetail)
  {
    bool isAdded = await _unitOfWork.SubDetails.AddDetailAsync(subDetail);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteDetail(string departmentId, string subDepartmentId, string detailId)
  {
    await _unitOfWork.Policy.DeletePSetAsync(departmentId, subDepartmentId, detailId);
    await _unitOfWork.StockItems.UpdateDeptSubDeptSubDetailsToNullInStockMasterAsync(departmentId, subDepartmentId, detailId);
    await _unitOfWork.SubDetails.DeleteSubDetailAsync(departmentId, subDepartmentId, detailId);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<SubDetail>> GetSubDetailsListAsync()
  {
    var subDetailsList = await _unitOfWork.SubDetails.GetSubDetailsListAsync();
    _unitOfWork.Commit();
    return subDetailsList;
  }

  public async Task<IEnumerable<SubDetail>?> GetSubDetailsByDepartments(List<string> departments)
  {
    IEnumerable<SubDetail>? subDetails = await _unitOfWork.SubDetails.GetSubDetailsByDepartmentsAsync(departments);
    _unitOfWork.Commit();
    return subDetails;
  }
}
