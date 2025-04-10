namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class CommissionController : Controller
{
  private readonly ICommissionService _commissionService;

  public CommissionController(ICommissionService commissionService)
  {
    _commissionService = commissionService;
  }

  // GET api/Commission/GetCommissions/employeeID
  [HttpGet]
  [Route("GetCommissionList/{employeeID}")]
  public async Task<IActionResult> GetCommissionList(string employeeID)
  {
    var commissions = await _commissionService.GetCommissionListAsync(employeeID);
    return Ok(commissions);
  }

  // POST api/Commission/CommissionExists
  [HttpPost]
  [Route("CommissionExists")]
  public async Task<IActionResult> CommissionExists(Commission commission)
  {
    var result = await _commissionService.CommissionExistsAsync(commission);
    return Ok(result);
  }

  // POST api/Commission/AddCommission
  [HttpPost]
  [Route("AddCommission")]
  public async Task<IActionResult> AddCommission(Commission commission)
  {
    await _commissionService.AddCommissionAsync(commission);
    return Ok();
  }

  // POST api/Commission/EditCommission
  [HttpPost]
  [Route("EditCommission")]
  public async Task<IActionResult> EditCommission(Commission commission)
  {
    await _commissionService.EditCommissionAsync(commission);
    return Ok();
  }

  // POST api/Commission/DeleteCommission
  [HttpPost]
  [Route("DeleteCommission")]
  public async Task<IActionResult> DeleteCommission(Commission commission)
  {
    await _commissionService.DeleteCommissionAsync(commission);
    return Ok();
  }
}
