namespace InfonetCSCR.Services;

public class TillsService : ITillsService
{
  private readonly IUnitOfWork _unitOfWork;

  public TillsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<string> GetPumpFeatures()
  {
    string pumpFeatures = await _unitOfWork.Tills.GetPumpFeaturesAsync();
    _unitOfWork.Commit();
    return pumpFeatures;
  }

  public async Task<int> GetMaxTill(int tillNo)
  {
    int mTill = await _unitOfWork.Tills.GetMaxTillAsync(tillNo);
    _unitOfWork.Commit();
    return mTill;
  }

  public async Task<IList<Till>> GetTills(int maxTill, bool isCashBonusPolicy)
  {
    IList<Till> tills = await _unitOfWork.Tills.GetTillsAsync(maxTill, isCashBonusPolicy);
    _unitOfWork.Commit();
    return tills;
  }

  public async Task<bool> AddTill(Till till)
  {
    // Delete all records from all tables in tills database for till TillNumber
    string[] tables = TillTables.Tables;
    await _unitOfWork.Tills.ClearTillAsync(tables, till.TillNum);

    // Add till
    bool isAdded = await _unitOfWork.Tills.AddTillAsync(till);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<int> GetTillsCount(int tillNo)
  {
    int tillCount = await _unitOfWork.Tills.GetTillsCountAsync(tillNo);
    _unitOfWork.Commit();
    return tillCount;
  }

  public async Task<bool> DeleteTill(int tillNo)
  {
    // Delete all records from all tables in tills database for till TillNumber
    string[] tables = TillTables.Tables;
    await _unitOfWork.Tills.ClearTillAsync(tables, tillNo);

    // Delete Till
    bool isDeleted = await _unitOfWork.Tills.DeleteTillAsync(tillNo);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> UpdateTillByFlipActiveOrFlipProcess(Till till)
  {
    bool isUpdated = await _unitOfWork.Tills.UpdateTillByFlipActiveOrFlipProcessAsync(till);
    _unitOfWork.Commit();
    return isUpdated;
  }
}
