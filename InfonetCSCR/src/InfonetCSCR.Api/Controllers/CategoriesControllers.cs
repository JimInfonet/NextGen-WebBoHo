namespace InfonetCSCR.Api.Controllers;

[Route("api/categories")]
[ApiController]
[Authorize]
public class CategoriesController : ControllerBase
{
  private readonly ICategoryService _categoryService;
  private readonly IFuelService _fuelService;

  public CategoriesController(
    ICategoryService categoryService,
    IFuelService fuelService)
  {
    _categoryService = categoryService;
    _fuelService = fuelService;
  }

  // GET api/categories/departments
  [HttpGet]
  [Route("departments")]
  public async Task<IActionResult> GetDepartments()
  {
    IEnumerable<Department> departments = await _categoryService.GetDepartmentsAsync();
    return Ok(departments);
  }

  // GET api/categories/subdepartments
  [HttpGet]
  [Route("subdepartments")]
  public async Task<IActionResult> GetSubDepartments(string? department = "")
  {
    IEnumerable<SubDepartment> subDepartments = await _categoryService.GetSubDepartmentsAsync(department);
    return Ok(subDepartments);
  }

  // GET api/categories/subdetail
  [HttpGet]
  [Route("subdetail")]
  public async Task<IActionResult> GetSubDetails(string? department = "", string? subDepartment = "")
  {
    IEnumerable<SubDetail> subDetails = await _categoryService.GetSubDetailsAsync(department, subDepartment);
    return Ok(subDetails);
  }

  // GET api/categories/CheckDepartmentExistsByEodGroupId/{eodGroupId}
  [HttpGet]
  [Route("CheckDepartmentExistsByEodGroupId/{eodGroupId}")]
  public async Task<IActionResult> CheckDepartmentExistsByEodGroupId(string eodGroupId)
  {
    bool isDepartmentExistsByEodGroupId = await _categoryService.CheckDepartmentExistsByEodGroupId(eodGroupId);
    return Ok(isDepartmentExistsByEodGroupId);
  }

  // POST api/categories/UpdateEodDetailByDept/{eodDetail}/{dept}
  [HttpPost]
  [Route("UpdateEodDetailByDept/{eodDetail}/{dept}")]
  public async Task<IActionResult> UpdateEodDetailByDept(string eodDetail, string dept)
  {
    bool result = await _categoryService.UpdateEodDetailByDept(eodDetail, dept);
    return Ok(result);
  }

  // POST api/categories/UpdateEodCountDetailByDept/{eodCountDetail}/{dept}
  [HttpPost]
  [Route("UpdateEodCountDetailByDept/{eodCountDetail}/{dept}")]
  public async Task<IActionResult> UpdateEodCountDetailByDept(string eodCountDetail, string dept)
  {
    bool result = await _categoryService.UpdateEodCountDetailByDept(eodCountDetail, dept);
    return Ok(result);
  }

  // GET api/categories/CheckDepartmentIdExists/{departmentId}
  [HttpGet]
  [Route("CheckDepartmentIdExists/{departmentId}")]
  public async Task<IActionResult> CheckDepartmentIdExists(string departmentId)
  {
    bool isDepartmentIdExists = await _categoryService.CheckDepartmentIdExists(departmentId);
    return Ok(isDepartmentIdExists);
  }

  // POST api/categories/AddDepartment
  [HttpPost]
  [Route("AddDepartment")]
  public async Task<IActionResult> AddDepartment(Department department)
  {
    bool result = await _categoryService.AddDepartment(department);
    return Ok(result);
  }

  // GET api/categories/CheckDepartmentExistsInFuelDept/{departmentId}
  [HttpGet]
  [Route("CheckDepartmentExistsInFuelDept/{departmentId}")]
  public async Task<IActionResult> CheckDepartmentExistsInFuelDept(string departmentId)
  {
    bool isDepartmentExistsInFuelDept = await _fuelService.CheckDepartmentExistsInFuelDept(departmentId);
    return Ok(isDepartmentExistsInFuelDept);
  }

  // POST api/categories/DeleteCategoryByDepartmentId/{departmentId}
  [HttpPost]
  [Route("DeleteCategoryByDepartmentId/{departmentId}")]
  public async Task<IActionResult> DeleteCategoryByDepartmentId(string departmentId)
  {
    bool result = await _categoryService.DeleteCategoryByDepartmentId(departmentId);
    return Ok(result);
  }
  
  // GET api/categories/GetCalCategoryList
  [HttpGet]
  [Route("GetCalCategoryList")]
  public async Task<IActionResult> GetClCategoryList()
  {
    return Ok(await _categoryService.GetClCategoryListAsync());
  }

  // GET api/categories/GetDiscCodesList
  [HttpGet]
  [Route("GetDiscCodesList")]
  public async Task<IActionResult> GetDiscCodesList()
  {
    return Ok(await _categoryService.GetDiscCodesListAsync());
  }

  // GET api/categories/GetBranchesList
  [HttpGet]
  [Route("GetBranchesList")]
  public async Task<IActionResult> GetBranchesList()
  {
    return Ok(await _categoryService.GetBranchesListAsync());
  }

  // GET api/categories/GetUomList
  [HttpGet]
  [Route("GetUomList")]
  public async Task<IActionResult> GetUomList()
  {
    return Ok(await _categoryService.GetUomListAsync());
  }
}
