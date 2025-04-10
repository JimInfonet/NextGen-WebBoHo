namespace InfonetCSCR.Services;

public class PLUMastService : IPLUMastService
{
  private readonly IUnitOfWork _unitOfWork;

  public PLUMastService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<PLUMast?> GetPLUMastByPLUCode(string pluCode)
  {
    PLUMast? pluMast = await _unitOfWork.PLUMasts.GetPLUMastByPLUCodeAsync(pluCode);
    _unitOfWork.Commit();
    return pluMast;
  }

  public async Task<IEnumerable<PLUMast>?> GetPLUCodesByPLUPrime(string pluPrime)
  {
    IEnumerable<PLUMast>? pluMasts = await _unitOfWork.PLUMasts.GetPLUCodesByPLUPrimeAsync(pluPrime);
    _unitOfWork.Commit();
    return pluMasts;
  }

  public async Task<string> AddPLUMast(PLUMast pluMast)
  {
    bool isPLUCodeExistsForPLUPrime = await _unitOfWork.PLUMasts.CheckPLUCodeExistsForPLUPrimeAsync(pluMast.PLUPrim, pluMast.PLUCode);
    if(isPLUCodeExistsForPLUPrime == true)
    {
      return "conflict";
    }

    await _unitOfWork.PLUMasts.AddPLUMastAsync(pluMast);
    bool result = _unitOfWork.Commit();
    if(result == true)
    {
      return "success";
    }

    return "failed";
  }

  public async Task<bool> DeletePLUMastByPLUCode(string pluCode)
  {
    await _unitOfWork.PLUMasts.DeletePLUMastByPLUCodeAsync(pluCode);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<PLUMast?>> GetPLUMast()
  {
    IEnumerable<PLUMast?> pluMast = await _unitOfWork.PLUMasts.GetPLUMast();
    _unitOfWork.Commit();
    return pluMast;
  }
}
