namespace InfonetCSCR.Core.Abstractions.Services;
public interface IPLUMastService
{
  Task<PLUMast?> GetPLUMastByPLUCode(string pluCode);
  Task<IEnumerable<PLUMast>?> GetPLUCodesByPLUPrime(string pluPrime);
  Task<string> AddPLUMast(PLUMast pluMast);
  Task<bool> DeletePLUMastByPLUCode(string pluCode);
  Task<IEnumerable<PLUMast?>> GetPLUMast();
}

