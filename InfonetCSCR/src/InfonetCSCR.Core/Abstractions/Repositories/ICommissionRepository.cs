namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICommissionRepository
{
	Task<IEnumerable<Commission>> GetCommissionListAsync(string employeeID);
	Task<bool> CommissionExistsAsync(Commission commission);
	Task AddCommissionAsync(Commission commission);
	Task AddEmpCommissionDataAsync(Commission commission);
	Task EditCommissionAsync(Commission commission);
	Task EditEmpCommissionDataAsync(Commission commission);
	Task DeleteCommissionAsync(Commission commission);
	Task DeleteEmpCommissionDataAsync(Commission commission);
}
