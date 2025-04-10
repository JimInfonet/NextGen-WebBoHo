namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICommissionService
{
	Task<IEnumerable<Commission>> GetCommissionListAsync(string employeeID);
	Task<bool> CommissionExistsAsync(Commission commission);
	Task AddCommissionAsync(Commission commission);
	Task EditCommissionAsync(Commission commission);
	Task DeleteCommissionAsync(Commission commission);
}
