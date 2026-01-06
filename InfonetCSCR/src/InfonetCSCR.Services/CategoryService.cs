namespace InfonetCSCR.Services;

public class CategoryService : ICategoryService
{
  private readonly IUnitOfWork _unitOfWork;

  public CategoryService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<Department>> GetDepartmentsAsync()
  {
    IEnumerable<Department> departments = await _unitOfWork.Category.GetDepartmentsAsync();
    _unitOfWork.Commit();
    return departments;
  }

  public async Task<IEnumerable<SubDepartment>> GetSubDepartmentsAsync(string department = "")
  {
    IEnumerable<SubDepartment> subDepartments = await _unitOfWork.Category.GetSubDepartmentsAsync(department);
    _unitOfWork.Commit();
    return subDepartments;
  }

  public async Task<IEnumerable<SubDetail>> GetSubDetailsAsync(string department = "", string subDepartment = "")
  {
    IEnumerable<SubDetail> subDetails = await _unitOfWork.Category.GetSubDetailsAsync(department, subDepartment);
    _unitOfWork.Commit();
    return subDetails;
  }

  public async Task<bool> CheckDepartmentExistsByEodGroupId(string eodGroupId)
  {
    bool isDepartmentExistsByEodGroupId = await _unitOfWork.Category.CheckDepartmentExistsByEodGroupIdAsync(eodGroupId);
    _unitOfWork.Commit();
    return isDepartmentExistsByEodGroupId;
  }

  public async Task<bool> UpdateEodDetailByDept(string eodDetail, string dept)
  {
    await _unitOfWork.Category.UpdateEodDetailByDeptAsync(eodDetail, dept);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> UpdateEodCountDetailByDept(string eodCountDetail, string dept)
  {
    await _unitOfWork.Category.UpdateEodCountDetailByDeptAsync(eodCountDetail, dept);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> CheckDepartmentIdExists(string departmentId)
  {
    bool isDepartmentIdExists = await _unitOfWork.Category.CheckDepartmentIdExistsAsync(departmentId);
    _unitOfWork.Commit();
    return isDepartmentIdExists;
  }

  public async Task<bool> AddDepartment(Department department)
  {
    await _unitOfWork.Category.AddDepartmentAsync(department);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeleteCategoryByDepartmentId(string departmentId)
  {
    await _unitOfWork.Policy.DeletePSetAsync(departmentId);
    await _unitOfWork.StockItems.UpdateDeptSubDeptSubDetailsToNullInStockMasterAsync(departmentId);
    await _unitOfWork.Category.DeleteCategoryByDepartmentIdAsync(departmentId);
    await _unitOfWork.SubDepartments.DeleteSubDepartmentAsync(departmentId);
    await _unitOfWork.SubDetails.DeleteSubDetailAsync(departmentId);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<ClCategory>> GetClCategoryListAsync()
  {
    var calCateogryList = await _unitOfWork.Category.GetClCategoryListAsync();
    _unitOfWork.Commit();
    return calCateogryList;
  }

  public async Task<IEnumerable<DiscCode>> GetDiscCodesListAsync()
  {
    var discCodeList = await _unitOfWork.Category.GetDiscCodesListAsync();
    _unitOfWork.Commit();
    return discCodeList;
  }

  public async Task<IEnumerable<Branch>> GetBranchesListAsync()
  {
    var branchesList = await _unitOfWork.Category.GetBranchesListAsync();
    _unitOfWork.Commit();
    return branchesList;
  }

  public async Task<IEnumerable<UnitOfMeasure>> GetUomListAsync()
  {
    var uomList = await _unitOfWork.Category.GetUomListAsync();
    _unitOfWork.Commit();
    return uomList;
  }
}
