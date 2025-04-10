namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CommissionController : Controller
{
	private readonly ICommissionService _commissionService;

	public CommissionController(ICommissionService commissionService)
	{
		_commissionService = commissionService;
	}

	public IActionResult Index()
	{
		return View();
	}

	public async Task<IActionResult> GetCommissionList()
	{
		var commissions = await _commissionService.GetCommisionListAsync();
		return Ok(commissions);
	}

	public async Task<IActionResult> GetCommissionModal()
	{
		ViewData["EmployeeList"] = await _commissionService.GetEmployeeSelectListAsync();
		ViewData["DepartmentList"] = await _commissionService.GetDepartmentSelectListAsync();
		return PartialView("_CommissionModal");
	}

	public async Task<IActionResult> GetSubDeptList(string department)
	{
		return Json(await _commissionService.GetSubDeptSelectListAsync(department));
	}

	public async Task<IActionResult> GetSubDetailList(string department, string subDept)
	{
		return Json(await _commissionService.GetSubDetailSelectListAsync(department, subDept));
	}

	public async Task<IActionResult> GetStockItemsByCriteria(StockItemsCriteriaViewModel criteria)
	{
		var stockItems = await _commissionService.GetStockItemsByCriteriaAsync(criteria);
		return Ok(stockItems);
	}

	public async Task<bool> CommissionExists(CommissionViewModel commission)
	{
		return await _commissionService.CommissionExistsAsync(commission);
	}

	public async Task<string> AddEditCommission(CommissionViewModel commissionDetails)
	{
		return await _commissionService.AddEditCommissionAsync(commissionDetails);
	}

	public async Task<string> DeleteCommission(CommissionViewModel commissionDetails)
	{
		return await _commissionService.DeleteCommissionAsync(commissionDetails);
	}
}
