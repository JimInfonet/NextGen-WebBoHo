namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class SubDetailController : ControllerBase
{
  private readonly ISubDetailService _subDetailService;

  public SubDetailController(ISubDetailService subDetailService)
  {
    _subDetailService = subDetailService;
  }

  // GET api/SubDetail/CheckDetailIdExistsForDepartmentAndSubDepartment/{departmentId}/{subDepartmentId}/{detailId}
  [HttpGet]
  [Route("CheckDetailIdExistsForDepartmentAndSubDepartment/{departmentId}/{subDepartmentId}/{detailId}")]
  public async Task<IActionResult> CheckDetailIdExistsForDepartmentAndSubDepartment(string departmentId, string subDepartmentId, string detailId)
  {
    bool isDetailIdExists = await _subDetailService.CheckDetailIdExistsForDepartmentAndSubDepartment(departmentId, subDepartmentId, detailId);
    return Ok(isDetailIdExists);
  }

  // POST api/SubDetail/AddDetail
  [HttpPost]
  [Route("AddDetail")]
  public async Task<IActionResult> AddDetail([FromBody] SubDetail subDetail)
  {
    bool result = await _subDetailService.AddDetail(subDetail);
    return Ok(true);
  }

  // POST api/SubDetail/DeleteDetail/{departmentId}/{subDepartmentId}/{detailId}
  [HttpPost]
  [Route("DeleteDetail/{departmentId}/{subDepartmentId}/{detailId}")]
  public async Task<IActionResult> DeleteDetail(string departmentId, string subDepartmentId, string detailId)
  {
    bool result = await _subDetailService.DeleteDetail(departmentId, subDepartmentId, detailId);
    return Ok(true);
  }

  // GET api/SubDetail/GetSubDetailsList
  [HttpGet]
  [Route("GetSubDetailsList")]
  public async Task<IActionResult> GetSubDetailsList()
  {
    return Ok(await _subDetailService.GetSubDetailsListAsync());
  }

  // POST api/SubDetail/GetSubDetailsByDepartments
  [HttpPost]
  [Route("GetSubDetailsByDepartments")]
  public async Task<IActionResult> GetSubDetailsByDepartments([FromBody] List<string> departments)
  {
    IEnumerable<SubDetail> subDetail = await _subDetailService.GetSubDetailsByDepartments(departments);
    return Ok(subDetail);
  }
}
