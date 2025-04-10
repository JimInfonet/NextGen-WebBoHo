namespace InfonetCSCR.Services;

public class CommissionService : ICommissionService
{
	private readonly IUnitOfWork _unitOfWork;

	public CommissionService(IApplicationConfig applicationConfig)
	{
		_unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
	}

	public async Task<IEnumerable<Commission>> GetCommissionListAsync(string employeeID)
	{
		var commissions = await _unitOfWork.Commission.GetCommissionListAsync(employeeID);
		_unitOfWork.Commit();
		return commissions;
	}

	public async Task<bool> CommissionExistsAsync(Commission commission)
	{
		var result = await _unitOfWork.Commission.CommissionExistsAsync(commission);
		_unitOfWork.Commit();
		return result;
	}

	public async Task AddCommissionAsync(Commission commission)
	{
		await _unitOfWork.Commission.AddCommissionAsync(commission);
		await _unitOfWork.Commission.AddEmpCommissionDataAsync(commission);
		_unitOfWork.Commit();
	}

	public async Task EditCommissionAsync(Commission commission)
	{
		await _unitOfWork.Commission.EditCommissionAsync(commission);
		await _unitOfWork.Commission.EditEmpCommissionDataAsync(commission);
		_unitOfWork.Commit();
	}

	public async Task DeleteCommissionAsync(Commission commission)
	{
		await _unitOfWork.Commission.DeleteCommissionAsync(commission);
		await _unitOfWork.Commission.DeleteEmpCommissionDataAsync(commission);
		_unitOfWork.Commit();
	}
}
