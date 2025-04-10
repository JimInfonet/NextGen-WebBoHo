namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class SubDepartmentController : ControllerBase
{
  private readonly ISubDepartmentService _subDepartmentService;

  public SubDepartmentController(ISubDepartmentService subDepartmentService)
  {
    _subDepartmentService = subDepartmentService;
  }

  // GET api/SubDepartment/CheckSubDepartmentIdExistsForDepartment/{departmentId}/{subDepartmentId}
  [HttpGet]
  [Route("CheckSubDepartmentIdExistsForDepartment/{departmentId}/{subDepartmentId}")]
  public async Task<IActionResult> CheckSubDepartmentIdExistsForDepartment(string departmentId, string subDepartmentId)
  {
    bool isSubDepartmentIdExists = await _subDepartmentService.CheckSubDepartmentIdExistsForDepartment(departmentId, subDepartmentId);
    return Ok(isSubDepartmentIdExists);
  }

  // POST api/SubDepartment/AddSubDepartment
  [HttpPost]
  [Route("AddSubDepartment")]
  public async Task<IActionResult> AddSubDepartment(SubDepartment subDepartment)
  {
    bool result = await _subDepartmentService.AddSubDepartment(subDepartment);
    return Ok(true);
  }

  // POST api/SubDepartment/DeleteSubDepartment/{departmentId}/{subDepartmentId}
  [HttpPost]
  [Route("DeleteSubDepartment/{departmentId}/{subDepartmentId}")]
  public async Task<IActionResult> DeleteSubDepartment(string departmentId, string subDepartmentId)
  {
    bool result = await _subDepartmentService.DeleteSubDepartment(departmentId, subDepartmentId);
    return Ok(true);
  }

  // GET api/SubDepartment/GetSubDepartmentsList
  [HttpGet]
  [Route("GetSubDepartmentsList")]
  public async Task<IActionResult> GetSubDepartmentsList()
  {
    return Ok(await _subDepartmentService.GetSubDepartmentsListAsync());
  }

  // POST api/SubDepartment/GetSubDepartmentsByDepartments
  [HttpPost]
  [Route("GetSubDepartmentsByDepartments")]
  public async Task<IActionResult> GetSubDepartmentsByDepartments([FromBody] List<string> departments)
  {
    IEnumerable<SubDepartment> subDepartments = await _subDepartmentService.GetSubDepartmentsByDepartments(departments);
    return Ok(subDepartments);
  }
}
