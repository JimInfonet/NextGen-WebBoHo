namespace InfonetCSCR.Services;

public class TransSaleTendService : ITransSaleTendService
{
  private readonly IUnitOfWork _unitOfWork;

  public TransSaleTendService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransConnectionString);
  }

  public async Task<bool> CheckTranscationExistsForTendName(string tendName)
  {
    bool isTranscationExistsForTendName = await _unitOfWork.TransSaleTends.CheckTranscationExistsForTendNameAsync(tendName);
    _unitOfWork.Commit();
    return isTranscationExistsForTendName;
  }
}
