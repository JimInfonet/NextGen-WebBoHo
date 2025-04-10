namespace InfonetCSCR.Services;

public class TillsSaleTendService : ITillsSaleTendService
{
  private readonly IUnitOfWork _unitOfWork;

  public TillsSaleTendService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTillsConnectionString);
  }

  public async Task<bool> CheckTranscationExistsForTendName(string tendName)
  {
    bool isTranscationExistsForTendName = await _unitOfWork.TilllsSaleTends.CheckTranscationExistsForTendNameAsync(tendName);
    _unitOfWork.Commit();
    return isTranscationExistsForTendName;
  }
}
