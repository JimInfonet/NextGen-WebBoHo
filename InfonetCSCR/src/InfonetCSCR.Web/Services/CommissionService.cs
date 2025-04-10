namespace InfonetCSCR.Web.Services;

public class CommissionService : ICommissionService
{
	private readonly IHttpService _http;
	private readonly IStringLocalizer<CommissionService> _localizer;
	private readonly IPolicyService _policyService;

	public CommissionService(IHttpService http, IStringLocalizer<CommissionService> localizer, IPolicyService policyService)
	{
		_http = http;
		_localizer = localizer;
		_policyService = policyService;
	}

	public async Task<IEnumerable<CommissionViewModel>?> GetCommisionListAsync()
	{
		string url = $"Commission/GetCommissionList/{_localizer.GetString("all")}";
		var commissions = await _http.GetAndParseAsync<IEnumerable<CommissionViewModel>?>(url);

		foreach (var commission in commissions)
		{
			commission.EmployeeID = HttpUtility.HtmlEncode(commission.EmployeeID);
			commission.Department = HttpUtility.HtmlEncode(commission.Department);
			commission.SubDept = HttpUtility.HtmlEncode(commission.SubDept);
			commission.SubDetail = HttpUtility.HtmlEncode(commission.SubDetail);
			commission.StockCode = HttpUtility.HtmlEncode(commission.StockCode);
			commission.FormattedApply = _localizer.GetString(commission.Apply.ToString().ToLower());
		}

		return commissions;
	}

	public async Task<SelectList> GetEmployeeSelectListAsync()
	{
		const string url = "User/GetAllUserDetails?timeSheetsAndHaveUserIdPolicy=false";
		var employees = await _http.GetAndParseAsync<IEnumerable<UserDetailsViewModel>>(url);

		foreach(var employee in employees)
		{
			employee.Name = $"{employee.Code}-{employee.Name}";
		}

		var employeeList = employees?.ToList() ?? new List<UserDetailsViewModel>();
		var allOption = _localizer.GetString("all");
		employeeList.Insert(0, new UserDetailsViewModel { Code = allOption, Name = allOption });
		var employeeSelectList = new SelectList(employeeList, "Code", "Name", allOption);
		return employeeSelectList;
	}

	public async Task<SelectList> GetDepartmentSelectListAsync()
	{
		const string url = "Categories/Departments";
		var departmens = await _http.GetAndParseAsync<IEnumerable<DepartmentViewModel>>(url);

		foreach(var department in departmens)
		{
			department.DeptName = $"{department.DeptId}-{department.DeptName}";
		}

		var departmenList = departmens?.ToList() ?? new List<DepartmentViewModel>();
		var allOption = _localizer.GetString("all");
		departmenList.Insert(0, new DepartmentViewModel { DeptId = allOption, DeptName = allOption });
		var departmentSelectList = new SelectList(departmenList, "DeptId", "DeptName", allOption);
		return departmentSelectList;
	}

	public async Task<SelectList> GetSubDeptSelectListAsync(string department)
	{
		var url = $"Categories/Subdepartments?department={department}";
		var subDepartments = await _http.GetAndParseAsync<IEnumerable<SubDepartmentViewModel>>(url);

		foreach ( var subDept in subDepartments)
		{
			subDept.SubDeptName = $"{subDept.SubDeptId}-{subDept.SubDeptName}";
		}

		var subDepartmentList = subDepartments?.ToList() ?? new List<SubDepartmentViewModel>();
		var allOption = _localizer.GetString("all");
		subDepartmentList.Insert(0, new SubDepartmentViewModel { SubDeptId = allOption, SubDeptName = allOption });
		var subDepartmentSelectList = new SelectList(subDepartmentList, "SubDeptId", "SubDeptName", allOption);
		return subDepartmentSelectList;
	}

	public async Task<SelectList> GetSubDetailSelectListAsync(string department, string subDept)
	{
		var url = $"Categories/Subdetail?department={department}&subdepartment={subDept}";
		var subDetails = await _http.GetAndParseAsync<IEnumerable<SubDetailViewModel>>(url);

		foreach( var subDetail in subDetails)
		{
			subDetail.SubDetailName = $"{subDetail.SubDetailId}-{subDetail.SubDetailName}";
		}

		var subDetailList = subDetails?.ToList() ?? new List<SubDetailViewModel>();
		var allOption = _localizer.GetString("all");
		subDetailList.Insert(0, new SubDetailViewModel { SubDetailId = allOption, SubDetailName = allOption });
		var subDetailSelectList = new SelectList(subDetailList, "SubDetailId", "SubDetailName", allOption);

		return subDetailSelectList;
	}

	public async Task<IEnumerable<StockCodesForModalViewModel>> GetStockItemsByCriteriaAsync(StockItemsCriteriaViewModel criteria)
	{
		var allOption = _localizer.GetString("all");
		criteria.AllOption = allOption.Value;
		criteria.SellInactivePolicy = _policyService.GetPolicy(PolicyName.SELL_INACT);
		const string url = "StockItems/GetStockItemsByCriteria";
		var stockCodes = await _http.PostAndParseAsync<IEnumerable<StockCodesForModalViewModel>>(url, criteria);
		var stockCodeList = stockCodes?.ToList() ?? new List<StockCodesForModalViewModel>();
		stockCodeList.Insert(0, new StockCodesForModalViewModel { StockCode = HttpUtility.HtmlEncode(allOption) });
		return stockCodeList;
	}

	public async Task<bool> CommissionExistsAsync(CommissionViewModel commission)
	{
		const string url = "Commission/CommissionExists";
		return await _http.PostAndParseAsync<bool>(url, commission);
	}

	public async Task<string> AddEditCommissionAsync(CommissionViewModel commissionDetails)
	{
		var url = "Commission/AddCommission";
		var response = _localizer.GetString("commissionAdded");

		if (commissionDetails.IsEdit)
		{
			url = "Commission/EditCommission";
			response = _localizer.GetString("commissionUpdated");
		}

		commissionDetails.AllOption = _localizer.GetString("all");
		await _http.PostAsync(url, commissionDetails);
		return response;
	}

	public async Task<string> DeleteCommissionAsync(CommissionViewModel commission)
	{
		const string url = "Commission/DeleteCommission";
		var response = _localizer.GetString("commissionDeleted");
		commission.AllOption = _localizer.GetString("all");
		await _http.PostAsync(url, commission);
		return response;
	}
}
