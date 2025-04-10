namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICommissionService
{
	Task<IEnumerable<CommissionViewModel>?> GetCommisionListAsync();
	Task<SelectList> GetEmployeeSelectListAsync();
	Task<SelectList> GetDepartmentSelectListAsync();
	Task<SelectList> GetSubDeptSelectListAsync(string department);
	Task<SelectList> GetSubDetailSelectListAsync(string department, string subDept);
	Task<IEnumerable<StockCodesForModalViewModel>> GetStockItemsByCriteriaAsync(StockItemsCriteriaViewModel criteria);
	Task<bool> CommissionExistsAsync(CommissionViewModel commission);
	Task<string> AddEditCommissionAsync(CommissionViewModel commissionDetails);
	Task<string> DeleteCommissionAsync(CommissionViewModel commission);
}
